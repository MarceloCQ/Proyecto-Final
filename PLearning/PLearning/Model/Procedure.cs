using System.Collections.Generic;

namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase procedimiento, la cual incluye todos los datos correspondientes a un procedimiento del programa
    /// </summary>
    public class Procedure
    {
        public string Name { get; set; }                                    //Nombre del procedimiento
        public int Type { get; set; }                                       //Tipo de retorno o void del procedimiento
        public int InitialDir { get; set; }                                 //Dirección de inicio del procedimiento
        public int[,] Size { get; set;}                                     //Cantidad de variables que tiene el procedimiento
        public List<Parameter> Parameters { get; set; }                     //Lista parámetros de procedimiento        
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
            Parameters = new List<Parameter>();
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

        /// <summary>
        /// Incrementa el contador de tamaño por cierta cantidad para un cierto tipo de variable y tipo de dato (asignar espacio a variables dimensionadas)
        /// </summary>
        /// <param name="variableType">Tipo de variable, puede ser temporal o variable</param>
        /// <param name="dataType">Tipo de dato del 0 al 5</param>
        /// <param name="x">Cantidad a incrementar</param>
        public void increaseCounterByX (int variableType, int dataType, int x)
        {
            Size[variableType, dataType]+= x;
        }

        

    }
}
