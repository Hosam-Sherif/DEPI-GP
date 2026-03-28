using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

////kl;

namespace Mazaad.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commission_Policies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commission_Policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndustryTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndustryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Material_Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    unit_of_measure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Material_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    industry_id = table.Column<int>(type: "int", nullable: false),
                    company_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    commercial_reg_num = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tax_registration_num = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address_details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_IndustryTypes_industry_id",
                        column: x => x.industry_id,
                        principalTable: "IndustryTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "App_Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    job_title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    last_login_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_App_Users_Companies_company_id",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    min_order_quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    available_quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    purity_percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Listings_Companies_company_id",
                        column: x => x.company_id,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Listings_Material_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Material_Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    reference_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    reference_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_App_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "App_Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    listing_id = table.Column<int>(type: "int", nullable: false),
                    placed_by_user_id = table.Column<int>(type: "int", nullable: false),
                    buyer_company_id = table.Column<int>(type: "int", nullable: false),
                    bid_amount_per_unit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_bid_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    winning_bid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bids_App_Users_placed_by_user_id",
                        column: x => x.placed_by_user_id,
                        principalTable: "App_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bids_Companies_buyer_company_id",
                        column: x => x.buyer_company_id,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bids_Listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "Listings",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Chat_Channels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    listing_id = table.Column<int>(type: "int", nullable: false),
                    seller_company_id = table.Column<int>(type: "int", nullable: false),
                    buyer_company_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chat_Channels_Companies_buyer_company_id",
                        column: x => x.buyer_company_id,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Channels_Companies_seller_company_id",
                        column: x => x.seller_company_id,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chat_Channels_Listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "Listings",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    seller_company_id = table.Column<int>(type: "int", nullable: false),
                    buyer_company_id = table.Column<int>(type: "int", nullable: false),
                    bid_id = table.Column<int>(type: "int", nullable: false),
                    applied_policy_id = table.Column<int>(type: "int", nullable: false),
                    agreed_quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    agreed_unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    platform_fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    order_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Bids_bid_id",
                        column: x => x.bid_id,
                        principalTable: "Bids",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Commission_Policies_applied_policy_id",
                        column: x => x.applied_policy_id,
                        principalTable: "Commission_Policies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Companies_buyer_company_id",
                        column: x => x.buyer_company_id,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Companies_seller_company_id",
                        column: x => x.seller_company_id,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    channel_id = table.Column<int>(type: "int", nullable: false),
                    sender_user_id = table.Column<int>(type: "int", nullable: false),
                    message_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_App_Users_sender_user_id",
                        column: x => x.sender_user_id,
                        principalTable: "App_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Chat_Channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "Chat_Channels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    transaction_reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_App_Users_company_id",
                table: "App_Users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_buyer_company_id",
                table: "Bids",
                column: "buyer_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_listing_id",
                table: "Bids",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_placed_by_user_id",
                table: "Bids",
                column: "placed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Channels_buyer_company_id",
                table: "Chat_Channels",
                column: "buyer_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Channels_listing_id",
                table: "Chat_Channels",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Channels_seller_company_id",
                table: "Chat_Channels",
                column: "seller_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_industry_id",
                table: "Companies",
                column: "industry_id");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_category_id",
                table: "Listings",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_company_id",
                table: "Listings",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_channel_id",
                table: "Messages",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_sender_user_id",
                table: "Messages",
                column: "sender_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_applied_policy_id",
                table: "Orders",
                column: "applied_policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_bid_id",
                table: "Orders",
                column: "bid_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_buyer_company_id",
                table: "Orders",
                column: "buyer_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_seller_company_id",
                table: "Orders",
                column: "seller_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_order_id",
                table: "Payments",
                column: "order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Chat_Channels");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropTable(
                name: "Commission_Policies");

            migrationBuilder.DropTable(
                name: "App_Users");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Material_Categories");

            migrationBuilder.DropTable(
                name: "IndustryTypes");
        }
    }
}
