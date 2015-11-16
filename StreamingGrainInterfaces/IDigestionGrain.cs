using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingGrainInterfaces
{
    public interface IDigestionGrain : IGrainWithGuidKey
    {
        Task LinkToStomach();
        //Task BecomeConsumer(Guid streamId, string streamNamespace, string providerToUse);

        //Task StopConsuming();

        //Task<int> GetNumberConsumed();
    }



}
