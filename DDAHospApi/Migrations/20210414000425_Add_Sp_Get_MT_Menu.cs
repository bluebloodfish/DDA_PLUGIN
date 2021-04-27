using Microsoft.EntityFrameworkCore.Migrations;

namespace DDAApi.Migrations
{
    public partial class Add_Sp_Get_MT_Menu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"create procedure sp_GetMenuItemForMt   @page_index Bigint
                        AS 
                        BEGIN
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
                        end";

            migrationBuilder.Sql(sp);

            var sp2 = @"create procedure sp_GetCategoryForMt 
	                        @page_index Bigint
                        AS 
                        BEGIN
                        WITH OrderedCategory AS  
                        (  
                            SELECT Code, Enable, Category, Category1, Category2, Category3, ShowOnMainMenu, ShowOnPhoneOrderMenu, ShowOnPOSMenu,
                            ROW_NUMBER() OVER (ORDER BY Code) AS RowNumber  
                            FROM Category   
                        )   

                        SELECT Code, Enable, Category, Category1, Category2, Category3, ShowOnMainMenu, ShowOnPhoneOrderMenu, ShowOnPOSMenu
                        FROM OrderedCategory   
                        WHERE RowNumber BETWEEN (@page_index*50+1) AND ((@page_index+1)*50);  
                        end";

            migrationBuilder.Sql(sp2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
