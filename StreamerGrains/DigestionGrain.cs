using Orleans;
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
        public Task LinkToStomach()
        {
            return TaskDone.Done;
        }
    }
}
