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
        public const int And = 0;
        public const int Or = 1;
        public const int MoreThan = 2;
        public const int LessThan = 3;
        public const int Different = 4;
        public const int EqualComparison = 5;
        public const int Sum = 6;
        public const int Substraction = 7;
        public const int Multiplication = 8;
        public const int Division = 9;
        public const int MoreThanEq = 10;
        public const int LessThanEq = 11;
        public const int Assignment = 12;
        public const int Goto = 13;
        public const int GotoF = 14;
        public const int Print = 15;
        public const int Read = 16;
        public const int Era = 17;
        public const int Param = 18;
        public const int Ret = 19;
        public const int Return = 20;
        public const int GoSub = 21;
        public const int EndProg = 22;
        public const int Verify = 23;
        public const int EndFunc = 24;
        public const int Ref = 25;


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
                case ">=":
                    return MoreThanEq;
                case "<=":
                    return LessThanEq;
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
                case 10: return ">=";
                case 11: return "<=";
                case 12: return "=";
                case 13: return "Goto";
                case 14: return "GotoF";
                case 15: return "Print";
                case 16: return "Read";
                case 17: return "Era";
                case 18: return "Param";
                case 19: return "Ret";
                case 20: return "Return";
                case 21: return "GoSub";
                case 22: return "EndProg";
                case 23: return "Verify";
                case 24: return "EndFunc";
                case 25: return "Ref";
                default: return "Error";

            }
        }

        
    }


}
