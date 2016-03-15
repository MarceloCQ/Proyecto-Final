using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Enumerations
{
    public static class ReturnType
    {
        public static int Int = 0;
        public static int Float = 1;
        public static int String = 2;
        public static int Bool = 3;
        public static int Char = 4;
        public static int Void = 5;
        public static int Program = 6;
        public static int Main = 7;

        public static int toReturnType(string returnType)
        {
            switch (returnType)
            {
                case "int":
                    return ReturnType.Int;
                case "float":
                    return ReturnType.Float;
                case "char":
                    return ReturnType.Char;
                case "bool":
                    return ReturnType.Bool;
                case "string":
                    return ReturnType.String;
                case "void":
                    return ReturnType.Void;
                default:
                    return -1;
            }
        }
    }
}
