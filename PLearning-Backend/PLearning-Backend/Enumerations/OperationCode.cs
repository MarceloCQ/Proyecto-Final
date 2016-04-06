using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Enumerations
{
    /// <summary>
    /// Clase estática OperationCode que se encarga de darles número a todos los códigos de operación,
    /// así como covertirlos de string a numero y viceversa
    /// </summary>
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
        public static int Era = 15;
        public static int Param = 16;
        public static int Ret = 17;
        public static int Return = 18;

        /// <summary>
        /// Método que se encarga de convertir un string a un código de operación
        /// </summary>
        /// <param name="operStr">El string del código de operación</param>
        /// <returns>El código de operación correspondiente</returns>
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

        /// <summary>
        /// Método que se encarga de convertir el código de operación a un string
        /// </summary>
        /// <param name="opCode">El código de operación a convertir</param>
        /// <returns>El string correspondiente</returns>
        public static string toString(int opCode)
        {
            switch (opCode)
            {
                case 0: return "and";
                case 1: return "or";
                case 2: return ">";
                case 3: return "<";
                case 4: return "!=";
                case 5: return "==";
                case 6: return "+";
                case 7: return "-";
                case 8: return "*";
                case 9: return "/";
                case 10: return "=";
                case 11: return "Goto";
                case 12: return "GotoF";
                case 13: return "Print";
                case 14: return "Read";
                case 15: return "Era";
                case 16: return "Ret";
                case 17: return "Param";
                default: return "Error";

            }
        }

        
    }


}
