namespace PLearning
{
    /// <summary>
    /// Enumeración que tiene los tipos de lineas que tiene la interface
    /// </summary>
    public enum LineType
    {
        Main,
        If,
        While,
        For,
        Function,
        Assign,
        Read,
        Write,
        Call,
        Add,
        Vars,
        Program,
        Else,
        Return,
        Other,
        None
    }

    /// <summary>
    /// Clase que se encarga de manejar los metodos para los tipos de linea
    /// </summary>
    public static class LineTypeExtensions
    {
        /// <summary>
        /// Método que se encarga de convertir un LineType a string
        /// </summary>
        /// <param name="ltype">El LineType a convertir</param>
        /// <returns></returns>
        public static string ToString (this LineType ltype)
        {
            switch (ltype)
            {
                case LineType.Main: return "Main";
                case LineType.If: return "If";
                case LineType.While: return "While";
                case LineType.For: return "For";
                case LineType.Function: return "Function";
                case LineType.Assign: return "Assign";
                case LineType.Read: return "Read";
                case LineType.Write: return "Write";
                case LineType.Call: return "Call";
                case LineType.Add: return "Add";
                case LineType.Vars: return "Vars";
                case LineType.Program: return "Program";
                case LineType.Else: return "Else";
                case LineType.Return: return "Return";
                case LineType.Other: return "Other";
                case LineType.None: return "None";
                default: return null;

            }
        }

        /// <summary>
        /// Método que se encarga de convertir un string a LineType
        /// </summary>
        /// <param name="sType">El string a convertir</param>
        /// <returns>El LineType que representa el string</returns>
        public static LineType ToType(string sType)
        {
            switch (sType)
            {
                case "Main": return LineType.Main;
                case "If": return LineType.If;
                case "While": return LineType.While;
                case "For": return LineType.For;
                case "Function": return LineType.Function;
                case "Assign": return LineType.Assign;
                case "Read": return LineType.Read;
                case "Write": return LineType.Write;
                case "Call": return LineType.Call;
                case "Add": return LineType.Add;
                case "Vars": return LineType.Vars;
                case "Program": return LineType.Program;
                case "Else": return LineType.Else;
                case "Return": return  LineType.Return;
                case "Other": return LineType.Other;
                case "None": return LineType.None;
                default: return LineType.None;

            }
        }


    }

}
