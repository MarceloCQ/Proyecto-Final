using PLearning_Backend.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Model
{
    class Procedure
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public Dictionary<string, Variable> VariableTable { get; set; }

        public Procedure (string name, int type)
        {
            Name = name;
            Type = type;
            VariableTable = new Dictionary<string, Variable>();
        }

        

    }
}
