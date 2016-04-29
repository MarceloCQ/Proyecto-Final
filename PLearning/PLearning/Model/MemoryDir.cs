using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLearning_Backend.Model
{
    public class MemoryDir
    {
        public int VariableType { get; set; }
        public int DataType { get; set; }
        public int RealDir { get; set; }

        public MemoryDir(int vtype, int dtype, int rdir)
        {
            DataType = dtype;
            VariableType = vtype;
            RealDir = rdir;        
        }
    }
}
