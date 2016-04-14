using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingGrainInterfaces
{
    interface IJunk : IGrainWithIntegerKey
    {
        Task OOOOhhhhNooo();
    }
}
