using Microsoft.EntityFrameworkCore.Migrations;

namespace DDAApi.Migrations
{
    public partial class OrderProcess_Log_Add_JsonStr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JsonStr",
                table: "TT_OrderProcess_Log",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ErrorLine",
                table: "TT_Exception_Log",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JsonStr",
                table: "TT_OrderProcess_Log");

            migrationBuilder.AlterColumn<long>(
                name: "ErrorLine",
                table: "TT_Exception_Log",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
