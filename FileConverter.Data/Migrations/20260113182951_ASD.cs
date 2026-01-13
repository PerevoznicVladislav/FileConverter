using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileConverter.Data.Migrations
{
    /// <inheritdoc />
    public partial class ASD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    monthly_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    max_conversion_per_month = table.Column<int>(type: "int", nullable: false),
                    max_upload_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    description = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Plans__BE9F8F1D0E389267", x => x.plan_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__B9BE370FBAE22FAA", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Conversions",
                columns: table => new
                {
                    conversion_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    from_type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    to_type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    input_file_name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    input_path = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    output_path = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Conversi__E4E07B3F8969CC21", x => x.conversion_id);
                    table.ForeignKey(
                        name: "FK_Conversions_Users",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "MonthlyUsage",
                columns: table => new
                {
                    monthly_usage_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    month = table.Column<int>(type: "int", nullable: false),
                    conversions_used = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MonthlyU__BFE86465821FB67E", x => x.monthly_usage_id);
                    table.ForeignKey(
                        name: "FK_MonthlyUsage_Users",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "UserPlan",
                columns: table => new
                {
                    user_plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserPlan__7E75D75BB299C1AC", x => x.user_plan_id);
                    table.ForeignKey(
                        name: "FK_UserPlan_Plans",
                        column: x => x.plan_id,
                        principalTable: "Plans",
                        principalColumn: "plan_id");
                    table.ForeignKey(
                        name: "FK_UserPlan_Users",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversions_user_id",
                table: "Conversions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_MonthlyUsage_User_Year_Month",
                table: "MonthlyUsage",
                columns: new[] { "user_id", "year", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Plans_Name",
                table: "Plans",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPlan_plan_id",
                table: "UserPlan",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "UQ_UserPlan_User",
                table: "UserPlan",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Email",
                table: "Users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conversions");

            migrationBuilder.DropTable(
                name: "MonthlyUsage");

            migrationBuilder.DropTable(
                name: "UserPlan");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
