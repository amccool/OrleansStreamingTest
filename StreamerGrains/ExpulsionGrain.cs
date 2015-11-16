using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamingGrainInterfaces;
using DTOData;

namespace StreamerGrains
{
    public class ExpulsionGrain : Grain, IExpulsionGrain
    {
        public Task LinkToDigestion()
        {
            return TaskDone.Done;
        }


        public Task<IEnumerable<Waste>> Dump()
        {

            return Task.FromResult(Enumerable.Repeat(new Waste()
            {
                length = 10,
                numPoops = 1,
                width = 2
            }, 500));

        }


    }
}
