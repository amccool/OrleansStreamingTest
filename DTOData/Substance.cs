using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOData
{
    [Serializable]
    public class Substance
    {
        public IEnumerable<Vitamin> vitamins;
    }
}
