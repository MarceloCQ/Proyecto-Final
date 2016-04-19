using PLearning_Backend.Enumerations;
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

        public dynamic ReadValue(int vType, int dataType, int virtualDir)
        {

            int realIndex = VirtualStructure.getRealIndex(vType, dataType, virtualDir);

            if (vType == VirtualStructure.VariableType.Global || vType == VirtualStructure.VariableType.Local)
            {
                switch (dataType)
                {
                    case DataType.Int:
                        return VariableIntegers[realIndex];
                    case DataType.Float:
                        return VariableFloats[realIndex];
                    case DataType.String:
                        return VariableStrings[realIndex];
                    case DataType.Char:
                        return VariableChars[realIndex];
                    case DataType.Bool:
                        return VariableBools[realIndex];

                }
            }
            else if (vType == VirtualStructure.VariableType.Temporal)
            {

                switch (dataType)
                {
                    case DataType.Int:
                        return TemporalIntegers[realIndex];
                    case DataType.Float:
                        return TemporalFloats[realIndex];
                    case DataType.String:
                        return TemporalStrings[realIndex];
                    case DataType.Char:
                        return TemporalChars[realIndex];
                    case DataType.Bool:
                        return TemporalBools[realIndex];

                }
            }
            else
            {
                return null;
            }

            return null;


            
        }

        public void WriteValue(int vType, int dataType, int virtualDir, dynamic value)
        {
            int realIndex = VirtualStructure.getRealIndex(vType, dataType, virtualDir);

            if (vType == VirtualStructure.VariableType.Global || vType == VirtualStructure.VariableType.Local)
            {
                switch (dataType)
                {
                    case DataType.Int:
                        VariableIntegers[realIndex] = value;
                        break;
                    case DataType.Float:
                        VariableFloats[realIndex] = value;
                        break;
                    case DataType.String:
                        VariableStrings[realIndex] = value;
                        break;
                    case DataType.Char:
                        VariableChars[realIndex] = value;
                        break;
                    case DataType.Bool:
                        VariableBools[realIndex] = value;
                        break;

                }
            }
            else if (vType == VirtualStructure.VariableType.Temporal)
            {

                switch (dataType)
                {
                    case DataType.Int:
                        TemporalIntegers[realIndex] = value;
                        break;
                    case DataType.Float:
                        TemporalFloats[realIndex] = value;
                        break;
                    case DataType.String:
                        TemporalStrings[realIndex] = value;
                        break;
                    case DataType.Char:
                        TemporalChars[realIndex] = value;
                        break;
                    case DataType.Bool:
                        TemporalBools[realIndex] = value;
                        break;

                }
            }
            else
            {
                
            }

            
        }


    }
}
