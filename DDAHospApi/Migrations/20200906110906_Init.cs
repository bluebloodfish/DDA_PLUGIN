using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;

namespace DDAApi.Migrations
{
    public partial class Init : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                
                name: "TT_ApiSetting",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    HttpBaseUrl = table.Column<string>(nullable: false),
                    OnlineOrderStartYear = table.Column<int>(nullable: false),

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TT_ApiSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TT_OrderH_Log",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrderNo = table.Column<string>(nullable: true),
                    CreateDateTime = table.Column<DateTime>(nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(nullable: false),
                    OpKind = table.Column<int>(nullable: false),
                    OpName = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    TTOrderId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TT_OrderH_Log", x => x.Id);
                });

           
            migrationBuilder.Sql(sql:
                @"Create trigger [dbo].[TT_OrderH_PaidAmount] ON [dbo].[OrderH] 
                For Update 
                As 
                Begin 
	                SET NOCOUNT ON; 
	
	                Declare @url nvarchar(Max);
	                Select top 1 @url = [HttpBaseUrl] FROM [TT_ApiSetting] order by ID; 
	                Set @url = ltrim(rtrim(@url));
	
	                If Len(@url) > 0
	                Begin
		                Declare @orderNo varchar(10), @opName nvarchar(15); 
		                Declare @oldAmount float, @newAmount float, @amount float; 
		                Declare @ttOrderId nvarchar(8); 
		                Declare @tips float;

		                select  @tips = tips, @ttOrderId = bookingno, @orderNo = orderno, @opName = opName, 
                                @newAmount = PaidAmount, @amount = Amount From inserted; 
	
		                If UPDATE(PaidAmount) and LEFT(@orderNo, 1) = '#' And @amount <= 0
		                Begin
			                Declare @cancelCount int;
							Set @cancelCount = (Select count(*) from TT_OrderH_Log where OrderNo = @orderNo and OpKind = 0);

			                If @cancelCount = 0 
			                Begin
				                INSERT INTO TT_OrderH_Log([OrderNo], TTOrderId, [CreateDateTime],[ModifiedDateTime],[OpKind],[OpName],[Status]) 
						                VALUES (@orderNo, @ttOrderId, GETDATE(), GETDATE(), 0, @opName, 0); 
				                Declare @dataJson nvarchar(Max); set @dataJson = '{""id"": ' + @ttOrderId + '}'
                                set @url = @url + 'api_v1/HospOrder/cancelorder';
                            exec dbo.Fn_Http_Post @url, @dataJson, 'application/json','application/json';
                            End
                        End

                    End
                End");


            migrationBuilder.Sql(sql:
                @"Create FUNCTION [dbo].[FN_HTTP_POST](
                @URL VARCHAR(256),
                @DATA VARCHAR(2000),
                @REQ_H_ACCEPT VARCHAR(256),
                @REQ_H_CONTENT_TYPE VARCHAR(256))
                RETURNS VARCHAR(5000)
                AS 
                BEGIN

                    DECLARE 
                    @object int,
                    @returnStatus int,
                    @returnText varchar(5000),
                    @errMsg varchar(2000),
                    @httpStatus varchar(20);
 
                    EXEC @returnStatus = SP_OACreate 'Msxml2.ServerXMLHTTP.3.0',@object OUT;  
    
                    IF @returnStatus <> 0  
                    BEGIN  
                        EXEC SP_OAGetErrorInfo @object, @errMsg OUT, @returnText OUT;
                        RETURN ('Initialize object failed. ' + @errMsg + ISNULL(@returnText,''));  
                    END  
    
                    EXEC @returnStatus= SP_OAMethod @object,'open',NULL,'post',@URL,'false';

                    IF @returnStatus <> 0
                    BEGIN
                    EXEC SP_OAGetErrorInfo @object, @errMsg OUT, @returnText OUT;
                    RETURN ('Create connection failed, ' + @errMsg + ISNULL(@returnText, ''));
                    END

                    EXEC @returnStatus=SP_OAMethod @object,'setRequestHeader',NULL,'Accept',@REQ_H_ACCEPT;
                    EXEC @returnStatus=SP_OAMethod @object,'setRequestHeader',NULL,'Content-Type',@REQ_H_CONTENT_TYPE;
                    EXEC @returnStatus=SP_OAMethod @object,'setRequestHeader',NULL,'Content-Length','1000000';

                    ExEC @returnStatus= SP_OAMethod @object,'send',NULL,@DATA;
                    IF @returnStatus <> 0 
                    BEGIN 
                    EXEC SP_OAGetErrorInfo @object, @errMsg OUT, @returnText OUT;
                    RETURN ('Request error. ' + @errMsg + ISNULL(@returnText, ''));
                    END

                    EXEC @returnStatus = SP_OAGetProperty @Object, 'Status', @httpStatus OUT;

                    IF @returnStatus <> 0
                    BEGIN
                        EXEC sp_OAGetErrorInfo @Object, @errMsg OUT, @returnText OUT;
                        RETURN ('Failed to get http code ' + @errMsg + ISNULL(@returnText, ''));
                    END

                    IF @httpStatus <> 200
                    BEGIN
                        RETURN ('Access Error，HTTP Status Code: ' + @httpStatus);
                    END

                    EXEC @returnStatus= SP_OAGetProperty @object,'responseText',@returnText OUT;

                    IF @returnStatus <> 0 
                    BEGIN 
                    EXEC SP_OAGetErrorInfo @object, @errMsg OUT, @returnText OUT;
                    RETURN ('Failed to get return message ' + @errMsg + ISNULL(@returnText, ''));
                    END

 
                RETURN @returnText;
                END ");

            migrationBuilder.Sql(sql: @"ALTER TABLE OrderH ALTER COLUMN CustomerName NVARCHAR (512);");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropTable(
                name: "TT_ApiSetting");

            migrationBuilder.DropTable(
                name: "TT_OrderH_Log");

            migrationBuilder.Sql("Drop trigger TT_OrderH_PaidAmount");

            migrationBuilder.Sql("drop function FN_HTTP_POST");

            migrationBuilder.Sql(sql: @"ALTER TABLE OrderH ALTER COLUMN CustomerName NVARCHAR (30);");

        }
    }
}
