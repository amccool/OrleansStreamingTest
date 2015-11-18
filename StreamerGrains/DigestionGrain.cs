using DTOData;
using Orleans;
using Orleans.Streams;
using StreamingGrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerGrains
{
    public class DigestionGrain : Grain, IDigestionGrain
    {
        public async Task LinkToMouth(Guid streamId)
        {

            var exitonly = GrainFactory.GetGrain<IExpulsionGrain>(this.GetPrimaryKeyString());

            await exitonly.LinkToDigestion(streamId);



            // Post data directly into device's stream.
            IStreamProvider streamProvider = base.GetStreamProvider("SMSProvider");

            IAsyncStream<Food> foodStream = streamProvider.GetStream<Food>(streamId, this.GetPrimaryKeyString());

            //var consumerObserver = new FoodObserver<Food>(this);

            var consumerHandle = await foodStream.SubscribeAsync(
                (f, t) =>
                {
                    base.GetLogger().Info("{0}{1}", f, t);
                    return TaskDone.Done;
                });
        }

    }
}
