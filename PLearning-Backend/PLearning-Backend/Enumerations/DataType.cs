using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Enumerations
{
    public static class DataType
    {
        //Se empieza en uno para dejarle al cubo semantico el cero como error
        public static int Int = 1;
        public static int Float = 2;
        public static int String = 3;
        public static int Bool = 4;
        public static int Char = 5;

        public static int toDataType(string dataType)
        {
            switch (dataType)
            {
                case "int":
                    return Int;
                case "float":
                    return Float;
                case "char":
                    return Char;
                case "bool":
                    return Bool;
                case "string":
                    return String;
                default:
                    return -1;
            }
        }
    }
}
