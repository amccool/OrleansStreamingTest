using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digestion
{
    public sealed class BasicOrleansInitializer
    {
        private readonly static TraceSource _traceSource = new TraceSource("ProvingGroundActorDirectMessaging.BasicOrleansInitializer", SourceLevels.Error);

        public static event EventHandler<EventArgs> OrleansInitComplete //pass-through to OrleansClientGlobalWrapper
        {
            add { OrleansClientGlobalWrapper.OrleansInitComplete += value; }
            remove { OrleansClientGlobalWrapper.OrleansInitComplete -= value; }
        }

        public static bool IsInitialized
        {
            get
            {
                return OrleansClientGlobalWrapper.IsInitialized;
            }
        }

        #region Singleton
        private BasicOrleansInitializer() { }

        private static readonly Lazy<BasicOrleansInitializer> lazy = new Lazy<BasicOrleansInitializer>(() => new BasicOrleansInitializer());
        private static BasicOrleansInitializer Instance { get { return lazy.Value; } }
        #endregion Singleton

        public static void BeginInit()
        {
            OrleansClientGlobalWrapper.EnsureInitialized();
            _traceSource.TraceEvent(TraceEventType.Verbose, 57, "Orleans Init has started");
        }
    }

}
