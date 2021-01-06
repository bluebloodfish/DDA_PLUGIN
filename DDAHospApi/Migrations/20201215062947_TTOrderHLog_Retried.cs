using Microsoft.EntityFrameworkCore.Migrations;

namespace DDAApi.Migrations
{
    public partial class TTOrderHLog_Retried : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "TT_OrderH_Log",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Retried",
                table: "TT_OrderH_Log",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "TT_OrderH_Log");

            migrationBuilder.DropColumn(
                name: "Retried",
                table: "TT_OrderH_Log");
        }
    }
}
