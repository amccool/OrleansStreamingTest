using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamingGrainInterfaces;
using DTOData;
using Orleans.Streams;
using StreamingGrainInterfacesYYYYY;

namespace StreamerGrainsYYY
{
    public class ExpulsionGrain : Grain, IExpulsionGrain
    {
        private readonly IList<Waste> outputs = new List<Waste>();

        public Task<IEnumerable<Waste>> Dump()
        {
            //    var ww = Enumerable.Repeat(new Waste()
            //    {
            //        length = 10,
            //        numPoops = 1,
            //        width = 2
            //    }, 500);


            //    var wwl = ww.ToList();


            //    return Task.FromResult(wwl);


            return Task.FromResult(outputs.AsEnumerable());
        }

        public async Task LinkToDigestion(Guid streamId)
        {
            IStreamProvider streamProvider = base.GetStreamProvider("SMSProvider");

            IAsyncStream<Food> foodStream = streamProvider.GetStream<Food>(streamId, this.GetPrimaryKeyString());

            await foodStream.SubscribeAsync(
                (e, t) =>
                {
                    //var tok = t.ToString();
                    outputs.Add(new Waste()
                    {
                        length = e.order,
                        numPoops = e.order,
                        width = e.order
                    });
                    return TaskDone.Done;
                });

        }
    }
}
