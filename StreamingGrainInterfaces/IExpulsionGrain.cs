using DTOData;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingGrainInterfacesYYYYY
{
    public interface IExpulsionGrain : IGrainWithStringKey
    {
        Task LinkToDigestion(Guid streamId);

        Task<IEnumerable<Waste>> Dump();
    }



}
