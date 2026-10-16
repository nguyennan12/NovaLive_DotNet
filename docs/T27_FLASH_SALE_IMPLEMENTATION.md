# T27 — Atomic Flash Sale Reserve

## Hiện trạng trước thay đổi

- T22 đã có: CheckoutCommandHandler quản lý transaction, khóa kho thường, tạo đơn/payment/outbox, dọn giỏ và rollback Result thất bại.
- T26 đã có: FlashSaleCampaign/FlashSaleItem, IFlashSaleRepository, FlashSalesErrors và quản lý campaign/đăng ký/duyệt. Task breakdown vẫn ghi MISSING, không phản ánh code hiện tại.
- T23 đã có hủy đơn buyer/seller; OrderReservationReleaser có TODO Flash Sale.
- Không có SettleOrderPaymentCommand, InventorySyncConsumer hay TimeoutOrderRollbackWorker. Tuy nhiên hai webhook MoMo/VietQR đã thực hiện settlement trực tiếp, nên T27 nối confirm vào cả hai.
- Có hạ tầng PostgreSQL thật bằng NOVALIVE_TEST_POSTGRES và PostgresFact, không có Testcontainers. Tái sử dụng hạ tầng hiện hữu, không thêm package.

## Thiết kế và tính nhất quán

- TryReserveAsync thực thi đúng một UPDATE có tham số: Approved và quantity - (reserved_qty + sold_qty) >= request_qty. Bổ sung EXISTS trong cùng module kiểm tra campaign đang hoạt động theo cùng quy ước T26: status Scheduled/Active trong khung giờ hợp lệ, loại Ended/Cancelled; thời gian DB dùng clock_timestamp().
- Affected rows phải bằng 1; bằng 0 trả FlashSalesErrors.SoldOut. Mọi mutation stock yêu cầu transaction của caller, không tự commit hay mở transaction con. InvalidOperationException chỉ báo lỗi sử dụng API hạ tầng khi thiếu transaction, không dùng cho lỗi nghiệp vụ.
- Checkout chuẩn bị giá/voucher/phí ship trước transaction; trong transaction kiểm tra cart/kho, advisory lock và kiểm tra limit theo ID tăng dần rồi reserve theo cùng thứ tự, ngay trước bước tạo đơn. Lỗi về sau rollback cả Flash Sale, Inventories, ledger, đơn, payment, voucher, cart và outbox.
- Preview và checkout sử dụng FlashSaleItems.flash_price hiện hành, qua PriceCalculator hiện hữu. Checkout kiểm tra lại campaign/item/giá và tập item active sau bước chuẩn bị; không dùng giá CartItems. Giá đổi hoặc item/campaign không còn hợp lệ thì trả Result lỗi để client tính lại.
- Một SubOrder chứa ít nhất một dòng Flash Sale có order_source = FlashSale. Dòng thường trong cùng SubOrder vẫn dùng SellPrice và không giữ quota Flash Sale.
- Giới hạn mỗi buyer: SUM(OrderItems.quantity) theo flash_sale_item_id, nối ParentOrders/SubOrders trong repository Order; chỉ đếm Reserved/Sold, loại SubOrder Cancelled. ID item xác định duy nhất campaign+SKU. Không có join Order trong FlashSaleRepository.
- Trước khi đếm, pg_advisory_xact_lock(hashtextextended(buyerId + flashSaleItemId)) khóa đến hết transaction. Với isolation mặc định Read Committed của checkout, request sau đọc được đơn đã commit của request trước. Hash collision chỉ làm tăng chờ, không làm mất bảo vệ. Kiểm tra used + requested <= limit với long.
- Confirm/Release là UPDATE có điều kiện reserved_qty >= qty. IFlashSaleStockService nạp dòng đơn bền vững, CAS FlashSaleState từ Reserved sang Sold/Released, sau đó cập nhật counter trong cùng transaction. Retry cùng trạng thái trả thành công, trạng thái đối nghịch trả ReservationConflict. Nếu counter lỗi, caller phải propagate Result để rollback cả marker. Service sắp xếp flash item ID rồi order item ID.
- Low-level ConfirmSoldAsync/ReleaseReservedAsync chỉ đảm bảo điều kiện counter; mọi luồng nghiệp vụ phải gọi stock service để có idempotency theo từng dòng đơn.

## Migration và khác biệt có chủ đích

- Thêm nullable OrderItems.flash_sale_item_id và flash_sale_state, cùng index; không thêm bảng usage. Không thể chỉ suy campaign từ thời gian và order_source một cách chính xác cho đơn trộn dòng thường/flash, lịch sử campaign hay confirm/release lặp. Hai trường này giữ liên kết và marker bền vững theo dòng đơn; legacy orders giữ null. Đã cân nhắc dùng JSON snapshot hoặc OrderStatusHistories để không đổi schema: snapshot cần bất biến, còn status history mô tả trạng thái SubOrder; dùng chúng làm marker quota từng dòng sẽ trộn nghĩa và khó kiểm soát CAS. Hai cột nullable là thay đổi nhỏ, có kiểu và query/index trực tiếp.
- Bổ sung CHECK cho quantity/per_user_limit dương và 0 <= reserved/sold, reserved+sold <= quantity vì EF mapping hiện tại còn thiếu các CHECK trong thiết kế DB.
- Service nhận subOrderIds và tự nạp (flashSaleItemId, qty) từ OrderItems, thay vì tin cặp ID/số lượng do event truyền lên.
- Giữ tên lỗi FlashSalesErrors của T26; không tạo FlashSaleErrors trùng chức năng. Không thêm Command/Query, endpoint, permission hay nghiệp vụ T22/T26 thay thế.
- Giữ quy ước effective Active của T26: Scheduled đã tới giờ cũng hoạt động, không đợi worker đổi cột status sang Active (worker chưa tồn tại). Đồng bộ IsActiveAt, public query và reserve; sửa trường hợp Ended vẫn được xem active trong khung giờ cũ. Đây là giả định tích hợp có chủ đích so với cách hiểu chỉ chấp nhận literal Active. Scheduled trước giờ, Ended/Cancelled hoặc đã hết giờ đều không reserve; SKU ngoài campaign active vẫn mua theo giá thường.

## Hook đã nối và điểm gọi cho các task sau

- MoMo/VietQR success: ConfirmSoldAsync(subOrderIds) trước credit wallet/outbox, trong TransactionBehavior của webhook. Failure rollback settlement.
- T23 hủy buyer/seller: OrderReservationReleaser gọi ReleaseReservedAsync(subOrderIds) sau release kho thường, trong transaction hủy hiện hữu.
- T32 nếu tách settlement riêng: gọi ConfirmSoldAsync sau kiểm tra thanh toán/trạng thái đơn; propagate Result lỗi, không tự trừ quota lần nữa.
- T47 chưa có: nếu worker gọi confirm lại sau PaymentSuccessEvent, gọi stock service trong transaction; marker Sold biến retry thành no-op.
- T49 chưa có: với payment online Pending quá 15 phút, xác thực/chuyển trạng thái order/payment và gọi ReleaseReservedAsync trong cùng transaction với release kho thường. Không triển khai job trong T27.
- T30 chưa có InitiatePaymentCommand riêng; API tạo QR/payment URL hiện hữu không làm thay đổi quota. Không thêm nhiệm vụ T30.
- COD: SubOrder Confirmed, Payment Pending và ExpiredAt null; quota giữ Reserved cho tới đối soát COD hoặc hủy hợp lệ. T49 phải loại COD khỏi timeout online 15 phút. T48/COD reconciliation tương lai gọi ConfirmSoldAsync. Lựa chọn khác là chuyển Sold ngay khi Confirmed, nhưng cần chính sách hoàn quota COD riêng; chưa mở rộng phạm vi này.

## Kiểm thử

Kết quả cuối ngày 16/10/2026 trên PostgreSQL **17.11** (Docker image postgres:17):

| Project | Pass | Fail | Skip |
| --- | ---: | ---: | ---: |
| NovaLive.Domain.Tests | 63 | 0 | 0 |
| NovaLive.Application.Tests | 369 | 0 | 0 |
| NovaLive.Infrastructure.Tests | 198 | 0 | 0 |
| NovaLive.Api.Tests | 112 | 0 | 0 |
| **Tổng** | **742** | **0** | **0** |

- `dotnet build NovaLive.sln --no-restore -m:1 -p:UseSharedCompilation=false`: thành công (lượt cuối 0 error, 0 warning; các lượt recompile trước có nullable warnings sẵn có ở PaymentsController và một số test, không thêm warning mới).
- `dotnet test NovaLive.sln --no-restore -m:1 -p:UseSharedCompilation=false`: exit 0, toàn bộ pass, không skip.
- AC-006: 1000 request qty=1, đúng 10 thành công/990 thất bại, reserved=10, sold=0. Request qty=3: đúng 3 thành công, reserved=9, sold=0.
- Kết quả TRX tại `.test-results/t27-verified/` (không đưa output sinh tự động vào Git).
- Lượt khảo sát đầu chạy thêm trên PostgreSQL 18.6 cũng pass; kết quả chấp nhận cuối sử dụng PostgreSQL 17.11. Không dùng DB ứng dụng thật.

Bộ test bao gồm: reserve thành công/hết suất/limit/campaign đổi trạng thái, giá live và đơn trộn, COD; service retry/conflict; PostgreSQL 1000 requests/10 suất, qty=3, same-buyer race, confirm/release song song, bảo toàn suất đơn khác, rollback marker khi counter lỗi, rollback mọi bảng và rollback reserve đầu khi item thứ hai hết suất.

Lệnh tái chạy (NOVALIVE_TEST_POSTGRES trỏ PostgreSQL test có quyền tạo/xóa database):

```powershell
dotnet build NovaLive.sln
dotnet test NovaLive.sln --no-build --logger trx
```

Mỗi test host tạo database có tên riêng và xóa sau test. Concurrency test tạo 1000 task bắt đầu cùng một gate, mỗi task DbContext/transaction riêng; pool tối đa 32 connection cho phép server xử lý hàng đợi như cấu hình API thực tế.

## Tự review theo checklist yêu cầu

- [x] File mới viết tay có một type, tên file khớp; migration/designer tuân theo convention EF hiện hữu.
- [x] Handler không inject DbContext; repository không join chéo module.
- [x] Lỗi nghiệp vụ dùng FlashSalesErrors và implicit Result.
- [x] SQL reserve có tham số, một conditional UPDATE và kiểm tra affected rows.
- [x] Reserve dùng transaction checkout; test rollback cả Result failure và exception sau mutation.
- [x] Confirm/release chống âm và idempotent bằng CAS dòng đơn.
- [x] Concurrency test 1000 request / 10 suất pass trên PostgreSQL 17.11.
- [x] FR-FS-005..009, NFR-REL-001/002, AC-006 có code và test tương ứng.

## File thêm/sửa

- `src/NovaLive.Application/Abstractions/Persistence/Repositories/IFlashSaleRepository.cs`
- `src/NovaLive.Application/Abstractions/Persistence/Repositories/IOrderItemRepository.cs`
- `src/NovaLive.Application/Abstractions/Services/IFlashSaleStockService.cs`
- `src/NovaLive.Application/DependencyInjection.cs`
- `src/NovaLive.Application/UseCases/FlashSales/FlashSaleStockService.cs`
- `src/NovaLive.Application/UseCases/FlashSales/FlashSalesErrors.cs`
- `src/NovaLive.Application/UseCases/Orders/CheckoutDraftBuilder.cs`
- `src/NovaLive.Application/UseCases/Orders/Commands/Checkout/CheckoutCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Orders/OrderReservationReleaser.cs`
- `src/NovaLive.Application/UseCases/Orders/PreparedCheckout.cs`
- `src/NovaLive.Application/UseCases/Orders/Queries/CalculateCheckoutDraft/CalculateCheckoutDraftQueryHandler.cs`
- `src/NovaLive.Application/UseCases/Payments/Commands/HandleMomoIpn/HandleMomoIpnCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Payments/Commands/HandleVietQrWebhook/HandleVietQrWebhookCommandHandler.cs`
- `src/NovaLive.Domain/Common/FlashSaleReservationState.cs`
- `src/NovaLive.Domain/FlashSales/FlashSaleCampaign.cs`
- `src/NovaLive.Domain/Orders/OrderItem.cs`
- `src/NovaLive.Domain/Orders/SubOrder.cs`
- `src/NovaLive.Infrastructure/Persistence/Configurations/FlashSaleConfiguration.cs`
- `src/NovaLive.Infrastructure/Persistence/Configurations/OrderConfiguration.cs`
- `src/NovaLive.Infrastructure/Persistence/Migrations/20261016021624_AddFlashSaleOrderReservations.Designer.cs`
- `src/NovaLive.Infrastructure/Persistence/Migrations/20261016021624_AddFlashSaleOrderReservations.cs`
- `src/NovaLive.Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs`
- `src/NovaLive.Infrastructure/Persistence/Repositories/FlashSaleRepository.cs`
- `src/NovaLive.Infrastructure/Persistence/Repositories/OrderItemRepository.cs`
- `tests/NovaLive.Application.Tests/FlashSales/CheckoutFlashSaleTests.cs`
- `tests/NovaLive.Application.Tests/FlashSales/FlashSaleStockServiceTests.cs`
- `tests/NovaLive.Application.Tests/Payments/HandleMomoIpnCommandHandlerTests.cs`
- `tests/NovaLive.Application.Tests/Payments/HandleVietQrWebhookCommandHandlerTests.cs`
- `tests/NovaLive.Application.Tests/UseCases/Orders/CheckoutCommandTestContext.cs`
- `tests/NovaLive.Application.Tests/UseCases/Orders/CheckoutTestContext.cs`
- `tests/NovaLive.Application.Tests/UseCases/Orders/OrderOperationsTestContext.cs`
- `tests/NovaLive.Domain.Tests/FlashSales/FlashSaleCampaignTests.cs`
- `tests/NovaLive.Infrastructure.Tests/FlashSales/PostgresFlashSaleTests.cs`
- `tests/NovaLive.Infrastructure.Tests/Orders/CheckoutPersistenceTests.cs`
- `docs/T27_FLASH_SALE_IMPLEMENTATION.md`
