using PLearning_Backend.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Structures
{
    /// <summary>
    /// Clase que se encarga de manejar la estructura virtual del compilador, la cual
    /// asigna la "memoria virtual" a las variables, constantes y temporales.
    /// </summary>
    static class VirtualStructure
    {
        /// <summary>
        /// Clase estatica para represental el tipo de variable como entero.
        /// </summary>
        public static class VariableType
        {
            public const int Global = 0;
            public const int Local = 1;
            public const int Temporal = 2;
            public const int Constant = 3;

        }

        //Matriz que almacena todas las direcciones iniciales de la memoria virtual
        private static int[,] initialMatrixStructure = new int[4, 6] {
                                                                         //NoUsado   //Int      //Float     //String    //Bool      //Char
                                                                        {-1,        10000,      12000,      14000,      16000,      18000},     //GLOBAL   
                                                                        {-1,        20000,      22000,      24000,      26000,      28000},     //LOCAL
                                                                        {-1,        30000,      31000,      32000,      33000,      34000},     //TEMPORAL
                                                                        {-1,        35000,      37000,      39000,      41000,      43000}      //CONSTANTE
                                                                    };


        //Matriz que almacena todas las direcciones de la memoria virtual
        private static int[,] matrixStructure = initialMatrixStructure;

        /// <summary>
        /// Método que sirve para obtener la siguiente dirección virtual 
        /// </summary>
        /// <param name="variableType">El tipo de variable a obtener</param>
        /// <param name="dataType">El tipo de dato que se quiere obtener</param>
        /// <returns></returns>
        public static int getNext(int variableType, int dataType)
        {
            return matrixStructure[variableType, dataType]++;
        }

        public static Tuple<int, int> getVTypeAndDType(int virtualDir)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    if (virtualDir < initialMatrixStructure[i , j])
                    {
                        if (j > 1)
                        {
                            return Tuple.Create(i, j - 1);
                        }
                        else
                        {
                            return Tuple.Create(i - 1, 5);
                        }
                    }
                    
                }
            }

            if (virtualDir > initialMatrixStructure[3, 5])
            {
                return Tuple.Create(3, 5);
            }

            return null;
        }

        public static void resetCounters()
        {
            matrixStructure[VariableType.Local, DataType.Int] = initialMatrixStructure[VariableType.Local, DataType.Int];
            matrixStructure[VariableType.Local, DataType.Float] = initialMatrixStructure[VariableType.Local, DataType.Float];
            matrixStructure[VariableType.Local, DataType.String] = initialMatrixStructure[VariableType.Local, DataType.String];
            matrixStructure[VariableType.Local, DataType.Bool] = initialMatrixStructure[VariableType.Local, DataType.Bool];
            matrixStructure[VariableType.Local, DataType.Char] = initialMatrixStructure[VariableType.Local, DataType.Char];

            matrixStructure[VariableType.Temporal, DataType.Int] = initialMatrixStructure[VariableType.Temporal, DataType.Int];
            matrixStructure[VariableType.Temporal, DataType.Float] = initialMatrixStructure[VariableType.Temporal, DataType.Float];
            matrixStructure[VariableType.Temporal, DataType.String] = initialMatrixStructure[VariableType.Temporal, DataType.String];
            matrixStructure[VariableType.Temporal, DataType.Bool] = initialMatrixStructure[VariableType.Temporal, DataType.Bool];
            matrixStructure[VariableType.Temporal, DataType.Char] = initialMatrixStructure[VariableType.Temporal, DataType.Char];
        }

        public static int getRealIndex(int vType, int dataType, int virtualDir)
        {
            return virtualDir - initialMatrixStructure[vType, dataType];
        }

    }
}
