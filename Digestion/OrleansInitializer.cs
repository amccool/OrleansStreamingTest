using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digestion
{
    public static class OrleansInitializer
    {
        private readonly static TraceSource _traceSource = new TraceSource("R5Ent.ProvingGround.ProvingGroundCommFactory", SourceLevels.Error);

        public static event EventHandler<EventArgs> OrleansInitComplete;

        public static bool IsInitialized
        {
            get
            {
                return BasicOrleansInitializer.IsInitialized;
            }
        }

        public static void BeginInit()
        {
#if USE_SERVICE_BUS
#else
            BasicOrleansInitializer.OrleansInitComplete += OrleansInitializer_OrleansInitComplete;
            BasicOrleansInitializer.BeginInit();
            _traceSource.TraceEvent(TraceEventType.Verbose, 57, "OrleansInitializer.BeginInit() started");
#endif //USE_SERVICE_BUS
        }

        private static void OrleansInitializer_OrleansInitComplete(object sender, EventArgs e)
        {
            OrleansInitComplete(sender, e);
        }
    }
}
