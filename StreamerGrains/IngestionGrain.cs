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
            // Post data directly into device's stream.
            IStreamProvider streamProvider = base.GetStreamProvider("SMSProvider");
            var digestivetract = GrainFactory.GetGrain<IDigestionGrain>(this.GetPrimaryKeyString());

            //var strmId = new Guid("7128ABF9-D945-4DDF-8E2D-DD27EABCD902");
            //var streamId = Guid.NewGuid();



            if(this.strmId.Equals(Guid.Empty))
            {
                throw new Exception("gggggggggggggggggggggggggggggggggggggggggggggg");
            }
            await digestivetract.LinkToMouth(this.strmId);
            IAsyncStream<Food> foodStream = streamProvider.GetStream<Food>(this.strmId, this.GetPrimaryKeyString());

            await foodStream.OnNextAsync(food);
        }

        public Task<Guid> PrepareFoodRoute()
        {
            this.strmId = Guid.NewGuid();
            return Task.FromResult(this.strmId);
        }
    }
}
