using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Enumerations
{
    public static class OperationCode
    {
        public static int And = 0;
        public static int Or = 1;
        public static int MoreThan = 2;
        public static int LessThan = 3;
        public static int Different = 4;
        public static int EqualComparison = 5;
        public static int Sum = 6;
        public static int Substraction = 7;
        public static int Multiplication = 8;
        public static int Division = 9;
        public static int Assignment = 10;
        public static int Goto = 11;
        public static int GotoF = 12;
        public static int Print = 13;
        public static int ReadLine = 14;

        public static int toOperationCode(string operStr)
        {
            
            switch (operStr)
            {
                case "and":
                    return And;
                case "or":
                    return Or;
                case ">":
                    return MoreThan;
                case "<":
                    return LessThan;
                case "!=":
                    return Different;
                case "==":
                    return EqualComparison;
                case "+":
                    return Sum;
                case "-":
                    return Substraction;
                case "*":
                    return Multiplication;
                case "/":
                    return Division;
                case "=":
                    return Assignment;
                default:
                    return -1;
            }
        }
    }
}
