using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Enumerations
{
    /// <summary>
    /// Clase que se encarga de manejar los tipos de datos 
    /// </summary>
    public static class DataType
    {
        //Se empieza en uno para dejarle al cubo semantico el cero como error
        public const int Int = 1;
        public const int Float = 2;
        public const int String = 3;
        public const int Bool = 4;
        public const int Char = 5;

        /// <summary>
        /// Convierte un string a el numero de tipo de dato correspondiente
        /// </summary>
        /// <param name="dataType">El tipo de dato como string</param>
        /// <returns>Regresa el numero de tipo de dato correspondiente</returns>
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
