using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.HospModel
{
    [Table("TableSet")]
    public class HospTableSet
    {
        public int Status { get; set; }
        public string TableNo { get; set; }
        public Int16 Seats { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public bool FontBold { get; set; }
        public bool FontItalic { get; set; }
        public bool FontUnderline { get; set; }
        public bool FontStrikeout { get; set; }
        public int ButtonShape { get; set; }
        public int ButtonWidth { get; set; }
        public int ButtonHeight { get; set; }
        public int ButtonX { get; set; }
        public int ButtonY { get; set; }
        public bool PropertyFlag { get; set; }
        public string Description { get; set; }
        public int PageFlag { get; set; }
        public int PDAPosition { get; set; }
        public double MinimumChargePerTable { get; set; }
        public int ServiceStatus { get; set; }
        public string IPAddress { get; set; }
        public bool SelfOrderStatus { get; set; }
        public bool TerminalConnected { get; set; }
        public string TableLockerName { get; set; }
        public bool OnlineOrderTable { get; set; }
    }

    [Table("TablePage")]
    public class HospTablePage
    {
        public int PageNo { get; set; }
        public string Descritpion { get; set; }
    }
}
