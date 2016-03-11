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

        public Variable (string name, int type)
        {
            Name = name;
            Type = type;
        }

        public static int toDataType(string dataType)
        {
            switch (dataType)
            {
                case "int":
                    return DataType.Int;
                case "float":
                    return DataType.Float;
                case "char":
                    return DataType.Char;
                case "bool":
                    return DataType.Bool;
                case "string":
                    return DataType.String;
                default:
                    return -1;
            }
        }



    }
}
