using PLearning_Backend.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Model
{
    class Variable
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public int VirtualDir { get; set; }

        public Variable (string name, int type, int virtualDir)
        {
            Name = name;
            Type = type;
            VirtualDir = virtualDir;
        }

        



    }
}
