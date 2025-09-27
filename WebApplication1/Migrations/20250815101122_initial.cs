using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "business_units",
                columns: table => new
                {
                    business_unit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_units", x => x.business_unit_id);
                });

            migrationBuilder.CreateTable(
                name: "production_lines",
                columns: table => new
                {
                    production_line_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    business_unit_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_lines", x => x.production_line_id);
                    table.ForeignKey(
                        name: "FK_production_lines_business_units_business_unit_id",
                        column: x => x.business_unit_id,
                        principalTable: "business_units",
                        principalColumn: "business_unit_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "machines",
                columns: table => new
                {
                    machine_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    production_line_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "operasional")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_machines", x => x.machine_id);
                    table.ForeignKey(
                        name: "FK_machines_production_lines_production_line_id",
                        column: x => x.production_line_id,
                        principalTable: "production_lines",
                        principalColumn: "production_line_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    avatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    business_unit_id = table.Column<int>(type: "int", nullable: true),
                    production_line_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_users_business_units_business_unit_id",
                        column: x => x.business_unit_id,
                        principalTable: "business_units",
                        principalColumn: "business_unit_id");
                    table.ForeignKey(
                        name: "FK_users_production_lines_production_line_id",
                        column: x => x.production_line_id,
                        principalTable: "production_lines",
                        principalColumn: "production_line_id");
                });

            migrationBuilder.CreateTable(
                name: "damage_reports",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    machine_id = table.Column<int>(type: "int", nullable: false),
                    reported_by = table.Column<int>(type: "int", nullable: false),
                    reported_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    status = table.Column<bool>(type: "bit", nullable: true),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_reports", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_damage_reports_machines_machine_id",
                        column: x => x.machine_id,
                        principalTable: "machines",
                        principalColumn: "machine_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_damage_reports_users_reported_by",
                        column: x => x.reported_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    action_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repair_logs",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    report_id = table.Column<int>(type: "int", nullable: false),
                    kyt_approval_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    repair_completion_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_status = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repair_logs", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_repair_logs_damage_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "damage_reports",
                        principalColumn: "report_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repair_schedules",
                columns: table => new
                {
                    schedule_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    report_id = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    schedule_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_status = table.Column<bool>(type: "bit", nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repair_schedules", x => x.schedule_id);
                    table.ForeignKey(
                        name: "FK_repair_schedules_damage_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "damage_reports",
                        principalColumn: "report_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_repair_schedules_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "kyt_reports",
                columns: table => new
                {
                    kyt_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_id = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    approval_status = table.Column<bool>(type: "bit", nullable: true),
                    dangerous_mode = table.Column<int>(type: "int", nullable: false),
                    prepare_process = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    prepare_prediction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    prepare_control = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    main_process = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    main_prediction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    main_control = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    confirm_process = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    confirm_prediction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    confirm_control = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    analysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    action = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    reviewed_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyt_reports", x => x.kyt_id);
                    table.ForeignKey(
                        name: "FK_kyt_reports_repair_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "repair_schedules",
                        principalColumn: "schedule_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_kyt_reports_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_kyt_reports_users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "kyt_report_technicians",
                columns: table => new
                {
                    kyt_id = table.Column<int>(type: "int", nullable: false),
                    technician_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyt_report_technicians", x => new { x.kyt_id, x.technician_id });
                    table.ForeignKey(
                        name: "FK_kyt_report_technicians_kyt_reports_kyt_id",
                        column: x => x.kyt_id,
                        principalTable: "kyt_reports",
                        principalColumn: "kyt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_kyt_report_technicians_users_technician_id",
                        column: x => x.technician_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_damage_reports_machine_id",
                table: "damage_reports",
                column: "machine_id");

            migrationBuilder.CreateIndex(
                name: "IX_damage_reports_reported_by",
                table: "damage_reports",
                column: "reported_by");

            migrationBuilder.CreateIndex(
                name: "IX_kyt_report_technicians_technician_id",
                table: "kyt_report_technicians",
                column: "technician_id");

            migrationBuilder.CreateIndex(
                name: "IX_kyt_reports_created_by",
                table: "kyt_reports",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_kyt_reports_reviewed_by",
                table: "kyt_reports",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_kyt_reports_schedule_id",
                table: "kyt_reports",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_machines_production_line_id",
                table: "machines",
                column: "production_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_lines_business_unit_id",
                table: "production_lines",
                column: "business_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_repair_logs_report_id",
                table: "repair_logs",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_repair_schedules_created_by",
                table: "repair_schedules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_repair_schedules_report_id",
                table: "repair_schedules",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_business_unit_id",
                table: "users",
                column: "business_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_production_line_id",
                table: "users",
                column: "production_line_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kyt_report_technicians");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "repair_logs");

            migrationBuilder.DropTable(
                name: "kyt_reports");

            migrationBuilder.DropTable(
                name: "repair_schedules");

            migrationBuilder.DropTable(
                name: "damage_reports");

            migrationBuilder.DropTable(
                name: "machines");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "production_lines");

            migrationBuilder.DropTable(
                name: "business_units");
        }
    }
}
