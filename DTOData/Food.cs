using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOData
{
    [Serializable]
    public class Food
    {
        public FoodPyramid type;
        public string name;
        public int order;
    }

    public enum FoodPyramid
    {
        Meat,
        Dairy,
        Vegatable,
        Grain
    }
}
