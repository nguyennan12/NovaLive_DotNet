using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaLive.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    old_data_json = table.Column<string>(type: "jsonb", nullable: true),
                    new_data_json = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "carts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    icon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "discount_usages",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discount_usages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "discounts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    min_order_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    max_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    max_uses = table.Column<int>(type: "integer", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    per_user_limit = table.Column<int>(type: "integer", nullable: false),
                    applies_to = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    target_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: true),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flash_sale_campaigns",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    banner_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flash_sale_campaigns", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flash_sale_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flash_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    per_user_limit = table.Column<int>(type: "integer", nullable: false),
                    reserved_qty = table.Column<int>(type: "integer", nullable: false),
                    sold_qty = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_flash_sale_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    qty_on_hand = table.Column<int>(type: "integer", nullable: false),
                    reserved_qty = table.Column<int>(type: "integer", nullable: false),
                    min_stock = table.Column<int>(type: "integer", nullable: false),
                    last_updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_histories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    qty_before = table.Column<int>(type: "integer", nullable: false),
                    qty_change = table.Column<int>(type: "integer", nullable: false),
                    reserved_before = table.Column<int>(type: "integer", nullable: false),
                    reserved_change = table.Column<int>(type: "integer", nullable: false),
                    qty_after = table.Column<int>(type: "integer", nullable: false),
                    ref_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ref_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "livestream_comments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_question = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_livestream_comments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "livestream_products",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flash_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantity_limit = table.Column<int>(type: "integer", nullable: true),
                    sold_in_live = table.Column<int>(type: "integer", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false),
                    pinned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_livestream_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "livestream_sessions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    host_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    agora_channel_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    viewer_count = table.Column<int>(type: "integer", nullable: false),
                    peak_viewer_count = table.Column<int>(type: "integer", nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    playback_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_livestream_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    ref_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ref_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_snapshot_json = table.Column<string>(type: "jsonb", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    original_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_return_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    return_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    reason_detail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_return_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_returns",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    evidence_urls = table.Column<List<string>>(type: "text[]", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    seller_response = table.Column<string>(type: "text", nullable: true),
                    admin_note = table.Column<string>(type: "text", nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_returns", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_status_histories",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    to_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_by_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_status_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parent_orders",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    shipping_address_json = table.Column<string>(type: "jsonb", nullable: false),
                    total_item_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_shipping_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parent_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_escrows",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    held_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    platform_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    hold_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    released_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    refunded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_escrows", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    transaction_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    gateway_response = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_attributes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    spu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attr_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    attr_value = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_attributes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    device_info = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "review_images",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    media_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_review_images", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<short>(type: "smallint", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    seller_reply = table.Column<string>(type: "text", nullable: true),
                    seller_replied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    edit_count = table.Column<short>(type: "smallint", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reviews", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "seller_payouts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    bank_account = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    transfer_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    processed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seller_payouts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shipping_orders",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    service_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tracking_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pickup_address_json = table.Column<string>(type: "jsonb", nullable: false),
                    delivery_address_json = table.Column<string>(type: "jsonb", nullable: false),
                    cod_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    weight_gram = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    webhook_payload = table.Column<string>(type: "jsonb", nullable: true),
                    estimated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    picked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipping_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_addresses",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    warehouse_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    contact_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    province_id = table.Column<int>(type: "integer", nullable: true),
                    province_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    district_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ward_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ward_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    detail_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    is_return = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_addresses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_followers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_followers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_verifications",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_by = table.Column<Guid>(type: "uuid", nullable: false),
                    id_card_front = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    id_card_back = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    business_license = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    bank_account = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bank_branch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewer_note = table.Column<string>(type: "text", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_verifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_wallet_transactions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    balance_before = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    balance_after = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ref_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ref_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_wallet_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_wallets",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    holding_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    locked_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_wallets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shops",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    banner_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tax_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    rating_avg = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    rating_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shops", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sku_images",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sku_images", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skus",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    spu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    attributes_json = table.Column<string>(type: "jsonb", nullable: true),
                    original_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sell_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    weight_gram = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spus",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    brand = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    attributes_config_json = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sub_orders",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_order_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    warehouse_address_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    item_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shop_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    platform_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sub_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    seller_earnings = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cancel_reason = table.Column<string>(type: "text", nullable: true),
                    cancelled_by = table.Column<Guid>(type: "uuid", nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    shipped_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sub_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_addresses",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    province_id = table.Column<int>(type: "integer", nullable: true),
                    province_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    district_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ward_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ward_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    detail_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_addresses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_otps",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    otp_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    otp_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_otps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    birthday = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_seller = table.Column<bool>(type: "boolean", nullable: false),
                    account_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id",
                schema: "public",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_shop_id",
                schema: "public",
                table: "cart_items",
                columns: new[] { "cart_id", "shop_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_sku_id",
                schema: "public",
                table: "cart_items",
                columns: new[] { "cart_id", "sku_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carts_user_id",
                schema: "public",
                table: "carts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_id",
                schema: "public",
                table: "categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                schema: "public",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_discount_usages_discount_id_parent_order_id",
                schema: "public",
                table: "discount_usages",
                columns: new[] { "discount_id", "parent_order_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_discount_usages_user_id_discount_id",
                schema: "public",
                table: "discount_usages",
                columns: new[] { "user_id", "discount_id" });

            migrationBuilder.CreateIndex(
                name: "ix_discounts_code",
                schema: "public",
                table: "discounts",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_discounts_shop_id_is_active_valid_from_valid_to",
                schema: "public",
                table: "discounts",
                columns: new[] { "shop_id", "is_active", "valid_from", "valid_to" },
                filter: "is_active = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_flash_sale_campaigns_status_start_at_end_at",
                schema: "public",
                table: "flash_sale_campaigns",
                columns: new[] { "status", "start_at", "end_at" });

            migrationBuilder.CreateIndex(
                name: "ix_flash_sale_items_campaign_id_sku_id",
                schema: "public",
                table: "flash_sale_items",
                columns: new[] { "campaign_id", "sku_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flash_sale_items_campaign_id_status",
                schema: "public",
                table: "flash_sale_items",
                columns: new[] { "campaign_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_flash_sale_items_sku_id",
                schema: "public",
                table: "flash_sale_items",
                column: "sku_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_shop_id",
                schema: "public",
                table: "inventories",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_sku_id",
                schema: "public",
                table: "inventories",
                column: "sku_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventory_histories_ref_type_ref_id",
                schema: "public",
                table: "inventory_histories",
                columns: new[] { "ref_type", "ref_id" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_histories_sku_id_created_at",
                schema: "public",
                table: "inventory_histories",
                columns: new[] { "sku_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_livestream_comments_session_id_created_at",
                schema: "public",
                table: "livestream_comments",
                columns: new[] { "session_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_livestream_products_session_id_display_order",
                schema: "public",
                table: "livestream_products",
                columns: new[] { "session_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ix_livestream_products_session_id_sku_id",
                schema: "public",
                table: "livestream_products",
                columns: new[] { "session_id", "sku_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_livestream_sessions_agora_channel_name",
                schema: "public",
                table: "livestream_sessions",
                column: "agora_channel_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_livestream_sessions_shop_id_status",
                schema: "public",
                table: "livestream_sessions",
                columns: new[] { "shop_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read_created_at",
                schema: "public",
                table: "notifications",
                columns: new[] { "user_id", "is_read", "created_at" },
                filter: "is_read = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_sku_id",
                schema: "public",
                table: "order_items",
                column: "sku_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_sub_order_id",
                schema: "public",
                table: "order_items",
                column: "sub_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_returns_status",
                schema: "public",
                table: "order_returns",
                column: "status",
                filter: "status IN ('Pending','AdminDispute')");

            migrationBuilder.CreateIndex(
                name: "ix_order_returns_sub_order_id",
                schema: "public",
                table: "order_returns",
                column: "sub_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_status_histories_sub_order_id_changed_at",
                schema: "public",
                table: "order_status_histories",
                columns: new[] { "sub_order_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at_created_at",
                schema: "public",
                table: "outbox_messages",
                columns: new[] { "processed_at", "created_at" },
                filter: "processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_parent_orders_buyer_id_created_at",
                schema: "public",
                table: "parent_orders",
                columns: new[] { "buyer_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_parent_orders_order_code",
                schema: "public",
                table: "parent_orders",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_escrows_hold_until",
                schema: "public",
                table: "payment_escrows",
                column: "hold_until",
                filter: "status = 'Holding'");

            migrationBuilder.CreateIndex(
                name: "ix_payment_escrows_shop_id_status",
                schema: "public",
                table: "payment_escrows",
                columns: new[] { "shop_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_escrows_sub_order_id",
                schema: "public",
                table: "payment_escrows",
                column: "sub_order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_parent_order_id",
                schema: "public",
                table: "payments",
                column: "parent_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_transaction_ref",
                schema: "public",
                table: "payments",
                column: "transaction_ref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permissions_code",
                schema: "public",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permissions_resource_id_action",
                schema: "public",
                table: "permissions",
                columns: new[] { "resource_id", "action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_attributes_spu_id",
                schema: "public",
                table: "product_attributes",
                column: "spu_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                schema: "public",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_revoked_at",
                schema: "public",
                table: "refresh_tokens",
                columns: new[] { "user_id", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "ix_resources_code",
                schema: "public",
                table: "resources",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_review_images_review_id",
                schema: "public",
                table: "review_images",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_order_item_id",
                schema: "public",
                table: "reviews",
                column: "order_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reviews_shop_id_rating",
                schema: "public",
                table: "reviews",
                columns: new[] { "shop_id", "rating" },
                filter: "is_visible = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_sku_id_rating",
                schema: "public",
                table: "reviews",
                columns: new[] { "sku_id", "rating" },
                filter: "is_visible = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_id_permission_id",
                schema: "public",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                schema: "public",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_seller_payouts_shop_id_status",
                schema: "public",
                table: "seller_payouts",
                columns: new[] { "shop_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_shipping_orders_provider_tracking_code",
                schema: "public",
                table: "shipping_orders",
                columns: new[] { "provider", "tracking_code" },
                filter: "tracking_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_shipping_orders_status_updated_at",
                schema: "public",
                table: "shipping_orders",
                columns: new[] { "status", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "ix_shipping_orders_sub_order_id",
                schema: "public",
                table: "shipping_orders",
                column: "sub_order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_addresses_shop_id",
                schema: "public",
                table: "shop_addresses",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_followers_shop_id_created_at",
                schema: "public",
                table: "shop_followers",
                columns: new[] { "shop_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_shop_followers_shop_id_user_id",
                schema: "public",
                table: "shop_followers",
                columns: new[] { "shop_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_followers_user_id_created_at",
                schema: "public",
                table: "shop_followers",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_shop_verifications_shop_id",
                schema: "public",
                table: "shop_verifications",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_verifications_status",
                schema: "public",
                table: "shop_verifications",
                column: "status",
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_shop_wallet_transactions_ref_type_ref_id",
                schema: "public",
                table: "shop_wallet_transactions",
                columns: new[] { "ref_type", "ref_id" });

            migrationBuilder.CreateIndex(
                name: "ix_shop_wallet_transactions_wallet_id_created_at",
                schema: "public",
                table: "shop_wallet_transactions",
                columns: new[] { "wallet_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_shop_wallets_shop_id",
                schema: "public",
                table: "shop_wallets",
                column: "shop_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shops_owner_id",
                schema: "public",
                table: "shops",
                column: "owner_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shops_slug",
                schema: "public",
                table: "shops",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shops_status",
                schema: "public",
                table: "shops",
                column: "status",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_sku_images_sku_id",
                schema: "public",
                table: "sku_images",
                column: "sku_id");

            migrationBuilder.CreateIndex(
                name: "ix_skus_shop_id_is_active",
                schema: "public",
                table: "skus",
                columns: new[] { "shop_id", "is_active" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_skus_shop_id_sku_code",
                schema: "public",
                table: "skus",
                columns: new[] { "shop_id", "sku_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skus_spu_id",
                schema: "public",
                table: "skus",
                column: "spu_id",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_spus_category_id",
                schema: "public",
                table: "spus",
                column: "category_id",
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_spus_shop_id_status",
                schema: "public",
                table: "spus",
                columns: new[] { "shop_id", "status" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_sub_orders_parent_order_id",
                schema: "public",
                table: "sub_orders",
                column: "parent_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_sub_orders_shop_id_status",
                schema: "public",
                table: "sub_orders",
                columns: new[] { "shop_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_sub_orders_sub_order_code",
                schema: "public",
                table: "sub_orders",
                column: "sub_order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_addresses_user_id",
                schema: "public",
                table: "user_addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_otps_user_id_otp_type_expires_at",
                schema: "public",
                table: "user_otps",
                columns: new[] { "user_id", "otp_type", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id",
                schema: "public",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id_role_id",
                schema: "public",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "public",
                table: "users",
                column: "email",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_phone",
                schema: "public",
                table: "users",
                column: "phone",
                unique: true,
                filter: "phone IS NOT NULL AND deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cart_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "carts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "discount_usages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "discounts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "flash_sale_campaigns",
                schema: "public");

            migrationBuilder.DropTable(
                name: "flash_sale_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "inventories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "inventory_histories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "livestream_comments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "livestream_products",
                schema: "public");

            migrationBuilder.DropTable(
                name: "livestream_sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "order_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "order_return_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "order_returns",
                schema: "public");

            migrationBuilder.DropTable(
                name: "order_status_histories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "parent_orders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "payment_escrows",
                schema: "public");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_attributes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "resources",
                schema: "public");

            migrationBuilder.DropTable(
                name: "review_images",
                schema: "public");

            migrationBuilder.DropTable(
                name: "reviews",
                schema: "public");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "seller_payouts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shipping_orders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shop_addresses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shop_followers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shop_verifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shop_wallet_transactions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shop_wallets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shops",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sku_images",
                schema: "public");

            migrationBuilder.DropTable(
                name: "skus",
                schema: "public");

            migrationBuilder.DropTable(
                name: "spus",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sub_orders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_addresses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_otps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
