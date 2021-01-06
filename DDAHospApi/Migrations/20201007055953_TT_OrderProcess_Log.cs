using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DDAApi.Migrations
{
    public partial class TT_OrderProcess_Log : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.CreateTable(
                name: "TT_OrderProcess_Log",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LogDateTime = table.Column<DateTime>(nullable: false),
                    DDAOrderNo = table.Column<string>(nullable: true),
                    PlatformOrderNo = table.Column<string>(nullable: true),
                    TTOrderId = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    StatusNotes = table.Column<string>(nullable: true),
                    ErrorId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TT_OrderProcess_Log", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropTable(
                name: "TT_OrderProcess_Log");

        }
    }
}
