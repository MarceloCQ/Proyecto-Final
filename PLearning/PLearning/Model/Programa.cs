using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Model
{
    public class Programa
    {
        public string Name { get; set; }
        public Dictionary<string, Procedure> ProcedureTable { get; set; }
        public List<string> ProcedureList { get; set; }
        public List<Constant> ConstantTable { get; set; }
        public List<Quadruple> Quadruples { get; set; }

        public Programa(string name, Dictionary<string, Procedure> procTable, List<string> procList, List<Constant> constTable, List<Quadruple> quadruples)
        {
            Name = name;
            ProcedureTable = procTable;
            ProcedureList = procList;
            ConstantTable = constTable;
            Quadruples = quadruples;
        }

    }
}
