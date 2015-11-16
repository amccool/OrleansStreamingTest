using Orleans;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ingestion
{
    class Program
    {
        static void Main(string[] args)
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));


            policy.Execute(() => InitUs());




        }
        static void InitUs()
        {
            GrainClient.Initialize("ClientConfiguration.xml");
        }

    }
}
