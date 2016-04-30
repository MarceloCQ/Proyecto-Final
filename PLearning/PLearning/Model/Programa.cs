using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase programa que sirve para contener todo lo referente al programa ya compilado
    /// </summary>
    public class Programa
    {
        public string Name { get; set; }                                    //Nombre del programa
        public Dictionary<string, Procedure> ProcedureTable { get; set; }   //Tabla de procedimientos
        public List<string> ProcedureList { get; set; }                     //Lista de procedimientos para indexar la tabla
        public List<Constant> ConstantTable { get; set; }                   //Tabla de constantes
        public List<Quadruple> Quadruples { get; set; }                     //Cuadruplos generados

        /// <summary>
        /// Método constructor para la clase programa
        /// </summary>
        /// <param name="name">Nombre del programa</param>
        /// <param name="procTable">Tabla de procedimientos</param>
        /// <param name="procList">Lista de procedimientos</param>
        /// <param name="constTable">Tabla de constantes</param>
        /// <param name="quadruples">Cuádruplos</param>
        public Programa(string name, Dictionary<string, Procedure> procTable, List<string> procList, List<Constant> constTable, List<Quadruple> quadruples)
        {
            Name = name;
            ProcedureTable = procTable;
            ProcedureList = procList;
            ConstantTable = constTable;
            Quadruples = quadruples;
        }

    }
}
