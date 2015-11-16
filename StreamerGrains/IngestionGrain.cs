using Orleans;
using StreamingGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOData;
using Orleans.Streams;

namespace StreamerGrains
{
    public class IngestionGrain : Grain, IIngestionGrain
    {
        public async Task FeedMe(Food food, string mouth)
        {

            base.GetGrain<IDigestionGrain>(mouth);




            var strmId = new Guid("7128ABF9-D945-4DDF-8E2D-DD27EABCD902");

            // Post data directly into device's stream.
            IStreamProvider streamProvider = base.GetStreamProvider("SMSProvider");

            IAsyncStream<Food> foodStream = streamProvider.GetStream<Food>(strmId, mouth);
            await foodStream.OnNextAsync(food);
        }

    }
}
