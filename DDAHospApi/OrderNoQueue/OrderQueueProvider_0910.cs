//using DDAApi.DataAccess;
//using DDAApi.WebApi.Model;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DDAApi.OrderNoQueue
//{

//    public class OrderQueueProvider_0910 : IOrderNoQueueProvider
//    {
//        private readonly AppDbContext _ctx;

//        private static readonly ConcurrentQueue<OrderNoOption> _orderNoQueue = new ConcurrentQueue<OrderNoOption>();

//        public OrderQueueProvider_0910(AppDbContext ctx)
//        {
//            this._ctx = ctx;
//        }
        
//        public static int StartYear { get; set; }
//        public char YearLetter { get; set; }
//        public int MaxFixNo { get; set; }
//        public DateTime GenerateDate { get; set; }
       

//        public int Count => _orderNoQueue.Count;

//        public bool IsEmpty => _orderNoQueue.IsEmpty;

//        public string GetNewOrderNo() {
//            lock (this) {
//                OrderNoOption orderNoOption = new OrderNoOption();

//                if (_orderNoQueue.IsEmpty) {
//                    Refilled();
//                }

//                var result = _orderNoQueue.TryDequeue(out orderNoOption);
//                if (result)
//                {
//                    var now = DateTime.Now;
//                    if (now.Date.Equals(orderNoOption.GenerateDate))
//                    {
//                        return orderNoOption.OrderNo;
//                    }
//                    else if (now.Year > orderNoOption.GenerateDate.Year)
//                    {
//                        YearLetter++;
//                        MaxFixNo = 0;
//                        GenerateDate = now.Date;

//                        ChangeSeed(new OrderNoParts { YearLetter = YearLetter, CurrentMaxFixNo = MaxFixNo, GenerateDate = GenerateDate });
//                        _orderNoQueue.TryDequeue(out orderNoOption);
//                        return orderNoOption.OrderNo;
//                    }
//                    else //if (now.Date.CompareTo(GenerateDate.Date) > 0)
//                    {
//                        MaxFixNo = 0;
//                        GenerateDate = now.Date;

//                        ChangeSeed(new OrderNoParts { YearLetter = YearLetter, CurrentMaxFixNo = MaxFixNo, GenerateDate = GenerateDate });
//                        _orderNoQueue.TryDequeue(out orderNoOption);
//                        return orderNoOption.OrderNo;
//                    }
//                }
//                else
//                {
//                    return "";
//                }

//            }
//        }

//        public void ChangeSeed(OrderNoParts parts)
//        {
//            YearLetter = parts.YearLetter;
//            MaxFixNo = parts.CurrentMaxFixNo + 10;
//            GenerateDate = parts.GenerateDate;
//            _orderNoQueue.Clear();

//            for (int i = 1; i <= 10; i++)
//            {
//                string temp = $"000{parts.CurrentMaxFixNo + i}";
//                string fix = temp.Substring(temp.Length - 4, 4);
//                _orderNoQueue.Enqueue(new OrderNoOption { OrderNo = $"#{YearLetter}{GenerateDate.ToString("MMdd")}{fix}",
//                                                            GenerateDate = GenerateDate });
//            }
//        }

//        private bool Refilled() {
//            lock (this) {
//                if (_orderNoQueue.IsEmpty)
//                {
//                    for (int i = 1; i <= 10; i++)
//                    {
//                        string temp = $"000{MaxFixNo + i}";
//                        string fix = temp.Substring(temp.Length-4, 4);
//                        _orderNoQueue.Enqueue(new OrderNoOption { OrderNo = $"#{YearLetter}{GenerateDate.ToString("MMdd")}{fix}",
//                                                                    GenerateDate = GenerateDate });
//                    }

//                    MaxFixNo = MaxFixNo + 10;
//                    return true;
//                }
//                else {
//                    return false;
//                }
                
//            }
//        }

        
//    }
//}
