using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;


namespace StreamerGrains
{
    public class GutState : GrainState
    {
        public Guid StreamId { get; set; }
    }
}
