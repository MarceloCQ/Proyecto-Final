using PLearning_Backend.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase variable que incluye su nombre, tipo y dirección virtual
    /// </summary>
    public class Variable
    {
        public string Name { get; set; }                //Nombre de la variable
        public int Type { get; set; }                   //Tipo de la variable
        public int VirtualDir { get; set; }             //Dirección virtual
        public List<Dimension> Dimensions { get; set;}  //Lista de dimensiones en caso que las tenga
        


        /// <summary>
        /// Constructor de la variable la cual incluye todos los atributos
        /// </summary>
        /// <param name="name">Nombre de la variable</param>
        /// <param name="type">Tipo de la variable</param>
        /// <param name="virtualDir">Dirección virtual asignada</param>
        public Variable (string name, int type, int virtualDir)
        {
            Name = name;
            Type = type;
            VirtualDir = virtualDir;
            Dimensions = new List<Dimension>();
            
        }

        



    }
}
