using PLearning_Backend.Enumerations;
using PLearning_Backend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Structures
{
    /// <summary>
    /// Clase que se encarga de manejar la memoria de la máquina virtual
    /// </summary>
    class Memory
    {
        //Areglos en donde se van a guardar las variables
        public int[] VariableIntegers { get; set; }         
        public float[] VariableFloats { get; set; }
        public string[] VariableStrings { get; set; }
        public bool[] VariableBools { get; set; }
        public char[] VariableChars { get; set; }

        //Arreglos en donde se van a guardar los temporales
        public int[] TemporalIntegers { get; set; }
        public float[] TemporalFloats { get; set; }
        public string[] TemporalStrings { get; set; }
        public bool[] TemporalBools { get; set; }
        public char[] TemporalChars { get; set; }

        public int ReturnDir { get; set; }

        /// <summary>
        /// Método constructor de la clase memoria
        /// </summary>
        /// <param name="size">Tamaño a separar de la función</param>
        public Memory(int[,] size)
        {
            //Se asigna el espacio de memoria dependiendo del tamaño que tenga la función
            VariableIntegers = new int[size[VirtualStructure.VariableType.Local, DataType.Int]];
            VariableFloats = new float[size[VirtualStructure.VariableType.Local, DataType.Float]];
            VariableStrings = new string[size[VirtualStructure.VariableType.Local, DataType.String]];
            VariableBools = new bool[size[VirtualStructure.VariableType.Local, DataType.Bool]];
            VariableChars = new char[size[VirtualStructure.VariableType.Local, DataType.Char]];

            //Se asigna el espacio de los temporales dependiendo del tamaño de la función
            TemporalIntegers = new int[size[VirtualStructure.VariableType.Temporal, DataType.Int]];
            TemporalFloats = new float[size[VirtualStructure.VariableType.Temporal, DataType.Float]];
            TemporalStrings = new string[size[VirtualStructure.VariableType.Temporal, DataType.String]];
            TemporalBools = new bool[size[VirtualStructure.VariableType.Temporal, DataType.Bool]];
            TemporalChars = new char[size[VirtualStructure.VariableType.Temporal, DataType.Char]];
        }

        /// <summary>
        /// Método que sirve para leer un valor de la memoria
        /// </summary>
        /// <param name="memDir">Contiene el tipo de dato, el tipo de variable y la dirección real</param>
        /// <returns>El valor accesado</returns>
        public dynamic ReadValue(MemoryDir memDir)
        {
            //Se revisa para ver si el tipo de variable es global o local
            if (memDir.VariableType == VirtualStructure.VariableType.Global || memDir.VariableType == VirtualStructure.VariableType.Local)
            {
                //Si si es, entonces se accesa a los arreglos de Variable dependiendo del tipo
                switch (memDir.DataType)
                {
                    case DataType.Int:
                        return VariableIntegers[memDir.RealDir];
                    case DataType.Float:
                        return VariableFloats[memDir.RealDir];
                    case DataType.String:
                        return VariableStrings[memDir.RealDir];
                    case DataType.Char:
                        return VariableChars[memDir.RealDir];
                    case DataType.Bool:
                        return VariableBools[memDir.RealDir];

                }
            }
            //Si es temporal, entonces se accesa a los arreglos de temporales dependiendo del tipo
            else if (memDir.VariableType == VirtualStructure.VariableType.Temporal)
            {

                switch (memDir.DataType)
                {
                    case DataType.Int:
                        return TemporalIntegers[memDir.RealDir];
                    case DataType.Float:
                        return TemporalFloats[memDir.RealDir];
                    case DataType.String:
                        return TemporalStrings[memDir.RealDir];
                    case DataType.Char:
                        return TemporalChars[memDir.RealDir];
                    case DataType.Bool:
                        return TemporalBools[memDir.RealDir];

                }
            }
            else
            {
                return null;
            }

            return null;


            
        }

        /// <summary>
        /// Método que sirve para escribir un valor a la memoria
        /// </summary>
        /// <param name="memDir">Contiene el tipo de dato, el tipo de variable y la dirección real</param>
        /// <param name="value">Valor a escribir en la memoria</param>
        public void WriteValue(MemoryDir memDir, dynamic value)
        {
            //Se revisa para ver si el tipo de variable es global o local
            if (memDir.VariableType == VirtualStructure.VariableType.Global || memDir.VariableType == VirtualStructure.VariableType.Local)
            {
                //Si si es, entonces se escribe en los arreglos de variable, dependiendo del tipo
                switch (memDir.DataType)
                {
                    case DataType.Int:
                        VariableIntegers[memDir.RealDir] = value;
                        break;
                    case DataType.Float:
                        VariableFloats[memDir.RealDir] = value;
                        break;
                    case DataType.String:
                        VariableStrings[memDir.RealDir] = value;
                        break;
                    case DataType.Char:
                        VariableChars[memDir.RealDir] = value;
                        break;
                    case DataType.Bool:
                        VariableBools[memDir.RealDir] = value;
                        break;

                }
            }
            //Si es temporal
            else if (memDir.VariableType == VirtualStructure.VariableType.Temporal)
            {
                //Se escribe en los arreglos de temporal, dependiendo del tipo
                switch (memDir.DataType)
                {
                    case DataType.Int:
                        TemporalIntegers[memDir.RealDir] = value;
                        break;
                    case DataType.Float:
                        TemporalFloats[memDir.RealDir] = value;
                        break;
                    case DataType.String:
                        TemporalStrings[memDir.RealDir] = value;
                        break;
                    case DataType.Char:
                        TemporalChars[memDir.RealDir] = value;
                        break;
                    case DataType.Bool:
                        TemporalBools[memDir.RealDir] = value;
                        break;

                }
            }
         }


    }
}
