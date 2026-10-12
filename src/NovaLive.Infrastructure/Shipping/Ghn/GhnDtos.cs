using System.Text.Json.Serialization;

namespace NovaLive.Infrastructure.Shipping.Ghn;

public sealed class GhnApiResponse<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("code_message_value")]
    public string? CodeMessageValue { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public sealed class GhnFeeRequest
{
    [JsonPropertyName("from_district_id")]
    public int FromDistrictId { get; set; }

    [JsonPropertyName("from_ward_code")]
    public string FromWardCode { get; set; } = string.Empty;

    [JsonPropertyName("to_district_id")]
    public int ToDistrictId { get; set; }

    [JsonPropertyName("to_ward_code")]
    public string ToWardCode { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; } = 10;

    [JsonPropertyName("width")]
    public int Width { get; set; } = 10;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 10;

    [JsonPropertyName("service_type_id")]
    public int ServiceTypeId { get; set; } = 2;

    [JsonPropertyName("insurance_value")]
    public int InsuranceValue { get; set; }
}

public sealed class GhnFeeData
{
    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("service_fee")]
    public decimal ServiceFee { get; set; }

    [JsonPropertyName("insurance_fee")]
    public decimal InsuranceFee { get; set; }
}

public sealed class GhnCreateOrderRequest
{
    [JsonPropertyName("payment_type_id")]
    public int PaymentTypeId { get; set; } = 1; // 1: Người gửi trả cước (Shop/Sàn trả theo hợp đồng)

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("required_note")]
    public string RequiredNote { get; set; } = "KHONGCHOXEMHANG";

    [JsonPropertyName("from_name")]
    public string FromName { get; set; } = string.Empty;

    [JsonPropertyName("from_phone")]
    public string FromPhone { get; set; } = string.Empty;

    [JsonPropertyName("from_address")]
    public string FromAddress { get; set; } = string.Empty;

    [JsonPropertyName("from_ward_name")]
    public string FromWardName { get; set; } = string.Empty;

    [JsonPropertyName("from_district_name")]
    public string FromDistrictName { get; set; } = string.Empty;

    [JsonPropertyName("to_name")]
    public string ToName { get; set; } = string.Empty;

    [JsonPropertyName("to_phone")]
    public string ToPhone { get; set; } = string.Empty;

    [JsonPropertyName("to_address")]
    public string ToAddress { get; set; } = string.Empty;

    [JsonPropertyName("to_ward_code")]
    public string ToWardCode { get; set; } = string.Empty;

    [JsonPropertyName("to_district_id")]
    public int ToDistrictId { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; } = 10;

    [JsonPropertyName("width")]
    public int Width { get; set; } = 10;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 10;

    [JsonPropertyName("service_type_id")]
    public int ServiceTypeId { get; set; } = 2;

    [JsonPropertyName("cod_amount")]
    public int CodAmount { get; set; }

    [JsonPropertyName("client_order_code")]
    public string? ClientOrderCode { get; set; }

    [JsonPropertyName("items")]
    public List<GhnItemRequest> Items { get; set; } = [];
}

public sealed class GhnItemRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }
}

public sealed class GhnCreateOrderData
{
    [JsonPropertyName("order_code")]
    public string OrderCode { get; set; } = string.Empty;

    [JsonPropertyName("total_fee")]
    public decimal TotalFee { get; set; }

    [JsonPropertyName("expected_delivery_time")]
    public DateTime? ExpectedDeliveryTime { get; set; }
}

public sealed class GhnGenTokenRequest
{
    [JsonPropertyName("order_codes")]
    public List<string> OrderCodes { get; set; } = [];
}

public sealed class GhnGenTokenData
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
