using DDAApi.HospModel;
using DDAApi.OrderNoQueue;
using DDAApi.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public class HospOrderManage : IHospOrderManage
    {
        private readonly AppDbContext _ctx;

        public HospOrderManage(AppDbContext ctx)
        {
            this._ctx = ctx;
        }


        public async Task<int> SaveOrder(HospOrder order) {

            this._ctx.HospOrderHeads.Add(order.OrderHead);

            this._ctx.HospOrderItems.AddRange(order.OrderItems);
            if (order.RecvAcctList != null && order.RecvAcctList.Count > 0 
                &&  !string.IsNullOrEmpty(order.RecvAcctList[0].OrderNo)) {
                this._ctx.HospRecvAcctList.Add(order.RecvAcctList[0]);
            }

            return await this._ctx.SaveChangesAsync();

        }


        public async Task<int> CreateOrderItems(List<HospOrderItem> items) {
            this._ctx.HospOrderItems.AddRange(items);
            return await this._ctx.SaveChangesAsync();
        }

        public async Task<int> UpdateOrderItems(List<HospOrderItem> items)
        {
            this._ctx.HospOrderItems.UpdateRange(items);
            return await this._ctx.SaveChangesAsync();
        }

        public async Task<int> MarkVoidOrderItems(string orderNo)
        {
            var orderItems = this._ctx.HospOrderItems.Where(x => x.OrderNo.ToLower() == orderNo.ToLower()).ToList();
            if (orderItems != null && orderItems.Count() > 0) {
                foreach (var item in orderItems) {
                    item.VoidFlag = true;
                }
                this._ctx.HospOrderItems.UpdateRange(orderItems);
            }
            
            return await this._ctx.SaveChangesAsync();
        }

        

        public async Task<int> CreateRecvAcct(HospRecvAcct recvAcct)
        {
            this._ctx.HospRecvAcctList.Add(recvAcct);
            return await this._ctx.SaveChangesAsync();
        }


        public HospOrderHead GetHospOrderHead(string orderNo)
        {
            return this._ctx.HospOrderHeads.Where(x => x.OrderNo.ToLower() == orderNo.ToLower())
                               .FirstOrDefault();
        }


        public string GetOrderNo()
        {
            string currentMaxOrderNo = this._ctx.HospOrderHeads
                .Where(x => x.OrderDate.Date.Equals(DateTime.Now.Date))
                .Select(x => x.OrderNo).OrderByDescending(x => x).FirstOrDefault();

            string datePart = DateTime.Now.ToString("yyMMdd");

            if (string.IsNullOrEmpty(currentMaxOrderNo))
            {

                return $"{datePart}0001";
            }
            else
            {
                string fix = "";
                int no = int.Parse(currentMaxOrderNo.Substring(6));
                no++;
                string fixTemp = "000" + no.ToString();
                fix = fixTemp.Substring(fixTemp.Length - 4, 4);
                return $"{datePart}{fix}";
            }
        }


        public string GetNewOrderNo()
        {
            OrderNoDatePair pair = new OrderNoDatePair();
            pair = this._ctx.HospOrderHeads.AsNoTracking()
            .Where(x => x.OrderNo.StartsWith("#"))
            .Select(x => new OrderNoDatePair { MaxOrderNo = x.OrderNo, OrderDateTime = x.OrderDate })
            .OrderByDescending(x => x.MaxOrderNo).FirstOrDefault();

            string datePart = DateTime.Now.ToString("MMdd");

            if (pair == null)
            {

                return $"#A{datePart}0001";
            }
            else
            {
                char yearLetter = pair.MaxOrderNo[1];

                if (DateTime.Now.Date.Equals(pair.OrderDateTime.Date))
                {
                    string fix = "";
                    int no = int.Parse(pair.MaxOrderNo.Substring(6));
                    no++;
                    string fixTemp = "000" + no.ToString();
                    fix = fixTemp.Substring(fixTemp.Length - 4, 4);
                    return $"#{yearLetter}{datePart}{fix}";
                }
                else
                {
                    if (DateTime.Now.Year.CompareTo(pair.OrderDateTime.Year) > 0)
                    {
                        yearLetter++;
                        return $"#{yearLetter}{datePart}0001";
                    }
                    else
                    {
                        return $"#{yearLetter}{datePart}0001";
                    }

                }
            }
        }

        
        public Task UpdateNotesfor8287(string notes, string orderNo) {
            var pNotes = new SqlParameter("@notes", notes);
            var pOrderNo = new SqlParameter("@orderNo", orderNo);
            return this._ctx.Database.ExecuteSqlCommandAsync("Update OrderH Set Notes = @notes Where OrderNo = @orderNo", pNotes, pOrderNo);
        }


        public HospOrder GetOccupiedTalbeOrder(string tableNo)
        {
            var orderHead = this._ctx.HospOrderHeads
                            .Where(x => x.OrderDate.Date.Equals(DateTime.Now.Date) && x.BillKind == 0 && !x.Credit && x.TableNo.ToUpper() == tableNo.ToUpper())
                            .FirstOrDefault();
            
            var orderNo = orderHead.OrderNo;

            var orderItems = this._ctx.HospOrderItems.Where(x => x.OrderNo == orderNo).ToList();

            return new HospOrder
            {
                OrderHead = orderHead,
                OrderItems = orderItems
            };
        }



        public async Task<int> MergeOrder(HospOrder order)
        {
            this._ctx.HospOrderHeads.Update(order.OrderHead);

            this._ctx.HospOrderItems.AddRange(order.OrderItems);

            return await this._ctx.SaveChangesAsync();
        }
    }


    public class OrderNoDatePair {
        public string MaxOrderNo { get; set; }
        public DateTime OrderDateTime { get; set; }
    }
}
