using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDAApi.CancelOrderQueue
{
    public interface ICancelOrderQueueManager
    {
        void Run();
        void Stop();
        bool IsRunning { get; }
        int Count { get; }
    }
}
