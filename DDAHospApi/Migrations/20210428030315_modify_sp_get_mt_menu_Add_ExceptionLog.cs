using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DDAApi.Migrations
{
    public partial class modify_sp_get_mt_menu_Add_ExceptionLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TT_Exception_Log",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LogDateTime = table.Column<DateTime>(nullable: false),
                    Source = table.Column<string>(nullable: true),
                    ErrorNumber = table.Column<int>(nullable: false),
                    ErrorState = table.Column<int>(nullable: false),
                    ErrorProcedure = table.Column<string>(nullable: true),
                    ErrorLine = table.Column<long>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TT_Exception_Log", x => x.Id);
                });


            var sp = @"ALTER procedure sp_GetMenuItemForMt   @page_index Bigint
                        AS 
                        BEGIN TRY
                        WITH OrderedMenuItem AS  
                        (  
                            SELECT ItemCode, Description1, Description2, Description3, Description4, Category, TaxRate,
                                    price, price1, price2, price3, 
                                    subdescription,  subdescription1, subdescription2, subdescription3, 
                                    HappyHourPrice1, HappyHourPrice2, HappyHourPrice3, HappyHourPrice4,
                                    MainPosition, OnlyShowOnSubMenu, PicturePath, Instruction, Multiple, Active,
                                    ROW_NUMBER() OVER (ORDER BY ItemCode) AS RowNumber  
                            FROM MenuItem   
                        )   

                        SELECT ItemCode, Description1, Description2, Description3, Description4, Category, TaxRate,
                                    price, price1, price2, price3, 
                                    subdescription,  subdescription1, subdescription2, subdescription3, 
                                    HappyHourPrice1, HappyHourPrice2, HappyHourPrice3, HappyHourPrice4,
                                    MainPosition, OnlyShowOnSubMenu, PicturePath, Instruction, Multiple, Active   
                        FROM OrderedMenuItem   
                        WHERE RowNumber BETWEEN (@page_index*50+1) AND ((@page_index+1)*50);  
                        End Try
                        BEGIN CATCH
                            INSERT INTO TT_Exception_Log
                              ([LogDateTime], [Source], [ErrorNumber], [ErrorState], [ErrorLine], [ErrorProcedure], [ErrorMessage])
                            VALUES
                              (GETDATE(),
                               'sp_GetMenuItemForMt',
                               ERROR_NUMBER(),
                               ERROR_STATE(),
                               
                               ERROR_LINE(),
                               ERROR_PROCEDURE(),
                               ERROR_MESSAGE());
                        END CATCH";

            migrationBuilder.Sql(sp);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TT_Exception_Log");
        }
    }
}
