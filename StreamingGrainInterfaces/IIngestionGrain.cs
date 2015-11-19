using DTOData;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingGrainInterfaces
{
    public interface IIngestionGrain : IGrainWithStringKey
    {
        Task FeedMe(Food food);

        Task PrepareFoodRoute(Guid streamId);



        //Task BecomeProducer(Guid streamId, string streamNamespace, string providerToUse);

        //Task StartPeriodicProducing();

        //Task StopPeriodicProducing();

        //Task<int> GetNumberProduced();

        //Task ClearNumberProduced();
        //Task Produce();
    }


}
