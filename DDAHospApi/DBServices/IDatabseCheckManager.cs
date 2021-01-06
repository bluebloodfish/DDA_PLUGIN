using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.DBServices
{
    public interface IDatabseCheckManager
    {
        void Run();
        void Stop();
        bool IsRunning { get; }

    }
}
