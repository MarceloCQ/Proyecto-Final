using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLearning_Backend.Model
{
    public class Dimension
    {
        public int Dim { get; set;}
        public int M { get; set; }

        public Dimension(int d, int m)
        {
            Dim = d;
            M = m;
        }

    }
}
