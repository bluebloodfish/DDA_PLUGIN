using DDAApi.HospModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAApi.DataAccess
{
    public class ProfileManager: IProfileManager
    {
        private readonly AppDbContext _ctx;

        public ProfileManager(AppDbContext ctx)
        {
            this._ctx = ctx;

        }

        public Profile GetProfile()
        {
            return this._ctx.DDAProfile.FirstOrDefault();
            
        }
    }
}
