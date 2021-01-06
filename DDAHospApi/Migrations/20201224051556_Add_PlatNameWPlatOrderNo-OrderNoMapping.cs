using Microsoft.EntityFrameworkCore.Migrations;

namespace DDAApi.Migrations
{
    public partial class Add_PlatNameWPlatOrderNoOrderNoMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlatName",
                table: "TT_OrderNoMapping",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlatOrderNo",
                table: "TT_OrderNoMapping",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "PointsUploaded",
                table: "OrderH",
                nullable: true,
                oldClrType: typeof(bool));

            migrationBuilder.Sql(sql:
                @"ALTER trigger [dbo].[TT_OrderH_PaidAmount] ON [dbo].[OrderH] 
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
	
		                If UPDATE(PaidAmount) And @amount <= 0
		                Begin
			                Declare @cancelCount int;
							Set @cancelCount = (Select count(*) from TT_OrderH_Log where OrderNo = @orderNo and OpKind = 0);

			                If @cancelCount = 0 
			                Begin
                                Insert Into TT_OrderH_Log ([OrderNo],[CreateDateTime],[ModifiedDateTime],[OpKind],[OpName],[Status],[TTOrderId],[Message],[Retried])
								Select @orderNo, getdate(), getdate(), 0, @opName, 0, TTID, '', 0
								from [TT_OrderNoMapping] m
								where m.orderno = @orderNo
                            End
                        End

                    End
                End");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlatName",
                table: "TT_OrderNoMapping");

            migrationBuilder.DropColumn(
                name: "PlatOrderNo",
                table: "TT_OrderNoMapping");

            migrationBuilder.AlterColumn<bool>(
                name: "PointsUploaded",
                table: "OrderH",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            migrationBuilder.Sql(sql:
                @"Alter trigger [dbo].[TT_OrderH_PaidAmount] ON [dbo].[OrderH] 
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
        }
    }
}
