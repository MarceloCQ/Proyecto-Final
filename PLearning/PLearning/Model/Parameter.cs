using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLearning_Backend.Model
{
    public class Parameter
    {
        public int Type { get; set; }
        public int VirtualDir { get; set; }
        public bool Reference { get; set; }
        public List<Dimension> Dimensions { get; set; }

        public Parameter(int t, int vDir, bool refer)
        {
            Type = t;
            VirtualDir = vDir;
            Reference = refer;
            Dimensions = new List<Dimension>();
                
        }
    }
}
