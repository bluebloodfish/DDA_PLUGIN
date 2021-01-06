using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public class DDAVersionManager : IDDAVersionManager
    {
        private readonly AppDbContext _ctx;

        public DDAVersionManager(AppDbContext ctx)
        {
            this._ctx = ctx;

        }

        public int GetDDAVersion()
        {
            var ddaVersion = this._ctx.DDAVersion.FirstOrDefault();
            if (ddaVersion != null)
            {
                return (int) (Math.Round(ddaVersion.Version, 3)*1000);
            }
            else {
                return 0;
            }
        }
    }
}
