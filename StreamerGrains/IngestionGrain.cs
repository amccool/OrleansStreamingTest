using Orleans;
using StreamingGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOData;
using Orleans.Streams;
using Orleans.Providers;

namespace StreamerGrains
{
    [StorageProvider(ProviderName="guts")]
    public class IngestionGrain : Grain<GutState>, IIngestionGrain
    {
        private Guid strmId;

        public async Task FeedMe(Food food)
        {

            var digestivetract = GrainFactory.GetGrain<IDigestionGrain>(base.IdentityString);

            //var strmId = new Guid("7128ABF9-D945-4DDF-8E2D-DD27EABCD902");
            var streamId = Guid.NewGuid();

            await digestivetract.LinkToMouth(streamId);



            // Post data directly into device's stream.
            IStreamProvider streamProvider = base.GetStreamProvider("SMSProvider");

            IAsyncStream<Food> foodStream = streamProvider.GetStream<Food>(streamId, base.IdentityString);
            await foodStream.OnNextAsync(food);
        }

        public Task<Guid> GetFoodRoute()
        {
            return Task.FromResult(strmId);
        }
    }
}
