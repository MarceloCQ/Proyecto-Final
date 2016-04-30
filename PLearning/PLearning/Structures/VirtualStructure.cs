using PLearning_Backend.Enumerations;
using PLearning_Backend.Model;
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
        private static readonly int[,] initialMatrixStructure = new int[4, 6] {
                                                                         //NoUsado   //Int      //Float     //String    //Bool      //Char
                                                                        {-1,        10000,      12000,      14000,      16000,      18000},     //GLOBAL   
                                                                        {-1,        20000,      22000,      24000,      26000,      28000},     //LOCAL
                                                                        {-1,        30000,      31000,      32000,      33000,      34000},     //TEMPORAL
                                                                        {-1,        35000,      37000,      39000,      41000,      43000}      //CONSTANTE
                                                                    };


        //Matriz que almacena todas las direcciones de la memoria virtual
        private static int[,] matrixStructure = new int[4, 6];

        /// <summary>
        /// Método constructor de la clase estática virtual structure que copia lo de la matriz inicial a la matriz usada
        /// </summary>
        static VirtualStructure()
        {
            matrixStructure = (int[,])initialMatrixStructure.Clone();
        }

        /// <summary>
        /// Método que sirve para resetear la matriz utilizada con todos los valores iniciales, por si se quiere volver a correr el programa
        /// </summary>
        public static void Reset()
        {
            matrixStructure = (int[,])initialMatrixStructure.Clone();
        }

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

        /// <summary>
        /// Metodo que sirve para reservar n espacios de la estructura virtual
        /// </summary>
        /// <param name="variableType">Es el tipo de variable (Temporal, constante, local o global)</param>
        /// <param name="dataType">Tipo de dato</param>
        /// <param name="spaces">Cantidad a reservar</param>
        public static void reserveSpaces(int variableType, int dataType, int spaces)
        {
            matrixStructure[variableType, dataType] += spaces;
        }

        /// <summary>
        /// Método que sirve para, a raiz de una dirección virtual, obtener el tipo de variable y el tipo de dato
        /// </summary>
        /// <param name="virtualDir">Dirección virtual a obtener</param>
        /// <returns>Tipo de variable y tipo de dato en una tupla</returns>
        public static Tuple<int, int> getVTypeAndDType(int virtualDir)
        {
            //Se recorre la matriz inicial
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    //Si la dirección virtual cae antes del siguiente valor
                    if (virtualDir < initialMatrixStructure[i , j])
                    {
                        //Si no es el primero, entonces se regresa el anterior
                        if (j > 1)
                        {
                            return Tuple.Create(i, j - 1);
                        }
                        //Si es el primero, se regresa el ultimo de la fila anterior
                        else
                        {
                            return Tuple.Create(i - 1, 5);
                        }
                    }
                    
                }
            }

            //Si es mayor que el último valor de la mtriz, se regresa que es constante char
            if (virtualDir > initialMatrixStructure[3, 5])
            {
                return Tuple.Create(3, 5);
            }

            return null;
        }

        /// <summary>
        /// Método que sirve para reiniciar los constador locales y temporales ya que son locales a una función
        /// </summary>
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

        /// <summary>
        /// Método que sirve para, a raiz de una dirección virtual obtener la dirección real, el tipo de variable y el tipo de dato
        /// </summary>
        /// <param name="virtualDir">Dirección virtual a obtener</param>
        /// <returns>La dirección real, el tipo de variable y el tipo de dato</returns>
        public static MemoryDir getRealIndex(int virtualDir)
        {
            //Se obtiene el tipo de variable y el tipo de dato con la función previa
            Tuple<int, int> VTypeAndDType = getVTypeAndDType(virtualDir);
            int vType = VTypeAndDType.Item1;
            int dataType = VTypeAndDType.Item2;

            //Se obtiene la dirección real restando la virtual menos la inicial para ese tipo de variable y de dato
            int realDir = virtualDir - initialMatrixStructure[vType, dataType];

            return new MemoryDir(vType, dataType, realDir);
        }

    }
}
