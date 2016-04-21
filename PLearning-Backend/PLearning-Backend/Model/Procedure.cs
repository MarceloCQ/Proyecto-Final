using System.Collections.Generic;

namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase procedimiento, la cual incluye todos los datos correspondientes a un procedimiento del programa
    /// </summary>
    public class Procedure
    {
        public string Name { get; set; }                            //Nombre del procedimiento
        public int Type { get; set; }                               //Tipo de retorno o void del procedimiento
        public int InitialDir { get; set; }                         //Direccion de inicio del procedimiento
        public int[,] Size { get; set;}                             //Cantidad de variables que tiene el procedimiento
        public List<int> ParametersType { get; set; }               //Lista de tipos parametros de procedimiento        
        public List<int> ParametersDirs { get; set; }               //Lista de direcciones de parametros de un procedimiento
        public Dictionary<string, Variable> VariableTable { get; set; }     //Tabla de variables 

        /// <summary>
        /// Constructor del procedimiento 
        /// </summary>
        /// <param name="name">Nombre del procedimiento</param>
        /// <param name="type">Tipo de retorno del procedimiento o Main o Program</param>
        public Procedure (string name, int type)
        {
            Name = name;
            Type = type;
            VariableTable = new Dictionary<string, Variable>();
            ParametersType = new List<int>();
            ParametersDirs = new List<int>();
            Size = new int[3, 6];
        }

        /// <summary>
        /// Incrementa el contador de tamaño para un cierto tipo de variable y tipo de dato
        /// </summary>
        /// <param name="variableType">Tipo de variable, puede ser temporal o variable</param>
        /// <param name="dataType">Tipo de dato del 0 al 5</param>
        public void increaseCounter (int variableType, int dataType)
        {
            Size[variableType, dataType]++;
        }

        public void increaseCounterByX (int variableType, int dataType, int x)
        {
            Size[variableType, dataType]+= x;
        }

        

    }
}
