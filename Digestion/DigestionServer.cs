using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digestion
{
    class DigestionServer
    {
        private readonly static TraceSource _traceSource = new TraceSource("ProvingGroundServerInterface.ProvingGroundServer", SourceLevels.Error);

        public void BeginInitOrleans()
        {
            OrleansInitializer.OrleansInitComplete += OrleansClientGlobalWrapper_OrleansInitComplete;
            OrleansInitializer.BeginInit();
        }

        public void Start()
        {
            var interval = Observable.Interval(TimeSpan.FromMilliseconds(500));
            interval.Subscribe(
                async o =>
                {
                    try
                    {
                        var mg = Orleans.GrainClient.GrainFactory.GetGrain<IManagementGrain>(0);
                        var silos = await mg.GetHosts();

                        var activesilos = from s in silos
                                          where s.Value == SiloStatus.Active
                                          select s.Key;

                        var rs = await mg.GetRuntimeStatistics(activesilos.ToArray());
                        foreach (var item in rs)
                        {
                            Console.WriteLine(string.Format("client count:{0}", item.ClientCount));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        //throw;

                        OrleansClientGlobalWrapper.RequestReinitialization();
                    }
                },
                () => Console.WriteLine("completed")
                );
        }




        private static void OrleansClientGlobalWrapper_OrleansInitComplete(object sender, EventArgs e)
        {
            _traceSource.TraceEvent(TraceEventType.Information, 57, "Orleans Initialized");
        }

        public void Stop()
        {
        }

    }
}
