using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Digestion
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    Orleans.GrainClient.Initialize();
                }
                catch (Exception ex)
                {
                    Orleans.GrainClient.Uninitialize();
                    //throw;
                }
            }
        

























            HostFactory.Run(x =>
            {
                x.Service<DigestionServer>(s =>
                {
                    //doesn't require a constructor for instantiation
                    s.ConstructUsing(name => new DigestionServer());
                    s.WhenStarted(tc =>
                    {
                        tc.BeginInitOrleans();
                        tc.Start();
                    });
                    s.WhenStopped(tc => tc.Stop());
                });

                //x.RunAsLocalSystem();
                //x.UseAssemblyInfoForServiceInfo();
                //x.SetServiceName(@"ProvingGroundTcpHostService");
                //x.StartAutomaticallyDelayed();

                //EN-100, utilize the retry mechanism to establish the orleans connection
                //also the orleans service may not even be running on the same machine.
                //x.DependsOn("OrleansSiloHostService");
            });
        }
    }
}
