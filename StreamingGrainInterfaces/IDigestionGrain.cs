using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingGrainInterfacesXXXXX
{

    public interface IDigestionGrain : IGrainWithStringKey
    {
        Task LinkToMouth(Guid streamId);

        Task ShitInMyMouth();
    }
}
