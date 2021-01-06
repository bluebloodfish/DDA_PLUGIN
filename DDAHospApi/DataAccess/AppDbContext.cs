using DDAApi.HospModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<Category> Catregories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<HospOrderHead> HospOrderHeads { get; set; }
        public DbSet<HospOrderItem> HospOrderItems { get; set; }
        public DbSet<HospRecvAcct> HospRecvAcctList { get; set; }
        public DbSet<DDAVersion> DDAVersion { get; set; }
        public DbSet<TT_OrderH_Log> TT_OrderH_Logs { get; set; }
        public DbSet<TT_ApiSetting> TT_ApiSettings { get; set; }
        public DbSet<TT_OrderProcess_Log> TT_OrderProcess_Logs { get; set; }
        public DbSet<TT_OrderNoMapping> TT_OrderNoMappings { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<HospOrderItem>().HasKey(i => new { i.OrderNo, i.IDNo, i.ItemCode});
            builder.Entity<HospRecvAcct>().HasKey(i => new { i.OrderNo, i.IDNo });
            builder.Entity<DDAVersion>().HasKey(i => i.Version);
        }

       

    }
}
