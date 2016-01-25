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

        public override Task OnActivateAsync()
        {
            this.ReadStateAsync();

            return base.OnActivateAsync();
        }



        public async Task FeedMe(Food food)
        {
            // Post data directly into device's stream.
            IStreamProvider streamProvider = base.GetStreamProvider("SMSProvider");


            if(this.strmId.Equals(Guid.Empty))
            {
                throw new Exception("gggggggggggggggggggggggggggggggggggggggggggggg");
            }
            IAsyncStream<Food> foodStream = streamProvider.GetStream<Food>(this.strmId, this.GetPrimaryKeyString());

             var digestivetract = GrainFactory.GetGrain<IDigestionGrain>(this.GetPrimaryKeyString());
            await digestivetract.LinkToMouth(this.strmId);





           await foodStream.OnNextAsync(food);
            //foodStream.OnErrorAsync(new Exception());
            await foodStream.OnCompletedAsync();
        }

        public Task PrepareFoodRoute(Guid streamId)
        {
            this.strmId = streamId;
            this.WriteStateAsync().Wait();

            return TaskDone.Done;
        }
    }






}
