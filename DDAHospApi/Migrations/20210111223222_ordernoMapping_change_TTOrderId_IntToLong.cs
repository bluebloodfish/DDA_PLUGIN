using Microsoft.EntityFrameworkCore.Migrations;

namespace DDAApi.Migrations
{
    public partial class ordernoMapping_change_TTOrderId_IntToLong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "TTId",
                table: "TT_OrderNoMapping",
                nullable: false,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TTId",
                table: "TT_OrderNoMapping",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
