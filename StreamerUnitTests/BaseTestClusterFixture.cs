using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.Serialization;
using Orleans.TestingHost;

namespace StreamerUnitTests
{
    public abstract class BaseTestClusterFixture : IDisposable
    {
        static BaseTestClusterFixture()
        {
            TestClusterOptions.DefaultTraceToConsole = false;
        }

        protected BaseTestClusterFixture()
        {
            GrainClient.Uninitialize();
            SerializationManager.InitializeForTesting();
            var testCluster = CreateTestCluster();
            if (testCluster.Primary == null)
            {
                testCluster.Deploy();
            }
            this.HostedCluster = testCluster;
        }

        protected abstract TestCluster CreateTestCluster();

        public TestCluster HostedCluster { get; private set; }

        public virtual void Dispose()
        {
            // 			Stopwatch profile = Stopwatch.StartNew();
            GrainClient.Uninitialize();
            //GrainClient.HardKill();
            foreach (var silo in HostedCluster.GetActiveSilos())
                HostedCluster.KillSilo(silo);
            //this.HostedCluster.StopAllSilos();
            // 			profile.Stop();
            // 			System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "kookyfile.txt"), $"BaseTestClusterFixture: HostedCluster.StopAllSilos() took {profile.Elapsed.TotalSeconds} seconds\r\n");
        }
    }

}
