using DDAApi.DataAccess;
using DDAApi.WebApi.Model;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.OrderNoQueue
{
    public class OrderNoOption{
        public string OrderNo { get; set; }
        public DateTime GenerateDate { get; set; }
    }

    public class OrderNoQueueProvider: IOrderNoQueueProvider
    {
        private readonly AppDbContext _ctx;

        private static readonly ConcurrentQueue<OrderNoOption> _orderNoQueue = new ConcurrentQueue<OrderNoOption>();
        private IServiceProvider _serviceProvider;

        public OrderNoQueueProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
        
        public static int StartYear { get; set; }
        public char YearLetter { get; set; }
        public int MaxFixNo { get; set; }
        public DateTime GenerateDate { get; set; }
       

        public int Count => _orderNoQueue.Count;

        public bool IsEmpty => _orderNoQueue.IsEmpty;

        //public string GetNewOrderNo() {
        //    lock (this) {
        //        OrderNoOption orderNoOption = new OrderNoOption();

        //        if (_orderNoQueue.IsEmpty) {
        //            Refilled();
        //        }

        //        var result = _orderNoQueue.TryDequeue(out orderNoOption);
        //        if (result)
        //        {
        //            var now = DateTime.Now;
        //            if (now.Date.Equals(orderNoOption.GenerateDate))
        //            {
        //                return orderNoOption.OrderNo;
        //            }
        //            else if (now.Year > orderNoOption.GenerateDate.Year)
        //            {
        //                YearLetter++;
        //                MaxFixNo = 0;
        //                GenerateDate = now.Date;

        //                ChangeSeed(new OrderNoParts { YearLetter = YearLetter, CurrentMaxFixNo = MaxFixNo, GenerateDate = GenerateDate });
        //                _orderNoQueue.TryDequeue(out orderNoOption);
        //                return orderNoOption.OrderNo;
        //            }
        //            else //if (now.Date.CompareTo(GenerateDate.Date) > 0)
        //            {
        //                MaxFixNo = 0;
        //                GenerateDate = now.Date;

        //                ChangeSeed(new OrderNoParts { YearLetter = YearLetter, CurrentMaxFixNo = MaxFixNo, GenerateDate = GenerateDate });
        //                _orderNoQueue.TryDequeue(out orderNoOption);
        //                return orderNoOption.OrderNo;
        //            }
        //        }
        //        else
        //        {
        //            return "";
        //        }

        //    }
        //}


        public string GetNewOrderNo()
        {
            lock (this)
            {
                OrderNoOption orderNoOption = new OrderNoOption();

                if (_orderNoQueue.IsEmpty)
                {
                    Refilled();
                }

                var result = _orderNoQueue.TryDequeue(out orderNoOption);
                if (result)
                {
                    var now = DateTime.Now;
                    if (now.Date.Equals(orderNoOption.GenerateDate))
                    {
                        return orderNoOption.OrderNo;
                    }
                    else if (now.Year != orderNoOption.GenerateDate.Year)
                    {
                        if (now.Year > (StartYear + 25) || now.Year < StartYear)
                        {
                            return "";
                        }
                        else
                        {
                            var orderParts = InitOrderNoParts(now);
                            if (orderParts.YearLetter == ' ')
                            {
                                return "";
                            }
                            else {
                                ChangeSeed(orderParts);
                                _orderNoQueue.TryDequeue(out orderNoOption);
                                return orderNoOption.OrderNo;
                            }

                            
                        }
                    }
                    else //if (now.Date.CompareTo(GenerateDate.Date) > 0)
                    {
                        MaxFixNo = 0;
                        GenerateDate = now.Date;

                        ChangeSeed(new OrderNoParts { YearLetter = YearLetter, CurrentMaxFixNo = MaxFixNo, GenerateDate = GenerateDate });
                        _orderNoQueue.TryDequeue(out orderNoOption);
                        return orderNoOption.OrderNo;
                    }
                }
                else
                {
                    return "";
                }

            }
        }
        public void ChangeSeed(OrderNoParts parts)
        {
            YearLetter = parts.YearLetter;
            MaxFixNo = parts.CurrentMaxFixNo + 10;
            GenerateDate = parts.GenerateDate;
            _orderNoQueue.Clear();

            for (int i = 1; i <= 10; i++)
            {
                string temp = $"000{parts.CurrentMaxFixNo + i}";
                string fix = temp.Substring(temp.Length - 4, 4);
                _orderNoQueue.Enqueue(new OrderNoOption { OrderNo = $"#{YearLetter}{GenerateDate.ToString("MMdd")}{fix}",
                                                            GenerateDate = GenerateDate });
            }
        }

        private bool Refilled() {
            if (_orderNoQueue.IsEmpty)
            {
                for (int i = 1; i <= 10; i++)
                {
                    string temp = $"000{MaxFixNo + i}";
                    string fix = temp.Substring(temp.Length-4, 4);
                    _orderNoQueue.Enqueue(new OrderNoOption { OrderNo = $"#{YearLetter}{GenerateDate.ToString("MMdd")}{fix}",
                                                                GenerateDate = GenerateDate });
                }

                MaxFixNo = MaxFixNo + 10;
                return true;
            }
            else {
                return false;
            }
        }



        public OrderNoParts InitOrderNoParts(DateTime now)
        {
            string datePart = now.ToString("yyMMdd");
            int currentYear = now.Year;

            if (currentYear < StartYear || (currentYear > StartYear + 25))
            {
                return new OrderNoParts { YearLetter = ' '};
            }
            else
            {
                int yd = currentYear - StartYear;
                char yearLetter = (char)(65 + yd); //65 = 'A'

                using (var scope = this._serviceProvider.CreateScope())
                {
                    var _ctx = scope.ServiceProvider.GetService<AppDbContext>();
                    string currentMaxOrderNo = _ctx.HospOrderHeads
                    .Where(x => x.OrderDate.Date.Equals(now.Date) && x.OrderNo.StartsWith("#"))
                    .Select(x => x.OrderNo).OrderByDescending(x => x).FirstOrDefault();

                    if (string.IsNullOrEmpty(currentMaxOrderNo))
                    {
                        return new OrderNoParts { YearLetter = yearLetter, CurrentMaxFixNo = 0, GenerateDate = now.Date };
                    }
                    else
                    {
                        int no = int.Parse(currentMaxOrderNo.Substring(6));
                        return new OrderNoParts { YearLetter = yearLetter, CurrentMaxFixNo = no, GenerateDate = now.Date};

                    }
                }



            }

            
        }

    }
}
