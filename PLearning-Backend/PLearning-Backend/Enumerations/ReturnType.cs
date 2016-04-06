using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Enumerations
{
    /// <summary>
    /// Clase que se encarga de manejar los tipos de retorno de una función mas el Program y el Main
    /// </summary>
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

        /// <summary>
        /// Convierte un string al tipo de retorno correspondiente
        /// </summary>
        /// <param name="returnType">String que representa el tipo se retorno correspondiente</param>
        /// <returns>El numero del tpo de retorno que corresponde al string</returns>
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
