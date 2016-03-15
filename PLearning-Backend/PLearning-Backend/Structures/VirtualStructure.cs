using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Structures
{
    static class VirtualStructure
    {
        public static class VariableType
        {
            public static readonly int Global = 0;
            public static readonly int Local = 1;
            public static readonly int Temporal = 2;
            public static readonly int Constant = 3;

        }

        private static int[,] matrixStructure = new int [4,6] {
                                                            //NoUsado   //Int       //Float     //String    //Bool      //Char
                                                               {-1,     10000,      12000,      14000,      16000,      18000},   //GLOBAL   
                                                               {-1,     20000,      22000,      24000,      26000,      28000},   //LOCAL
                                                               {-1,     30000,      31000,      32000,      33000,      34000},   //TEMPORAL
                                                               {-1,     35000,      37000,      39000,      41000,      43000}    //CONSTANTE
                                                               };

        public static int getNext(int variableType, int dataType)
        {
            return matrixStructure[variableType, dataType]++;
        }

    }
}
