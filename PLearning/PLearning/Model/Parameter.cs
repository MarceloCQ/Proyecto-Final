using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase parámetro que sirve para guardar ciertas cosas necesarias de un parámetro
    /// </summary>
    public class Parameter
    {
        public int Type { get; set; }           //Tipo de dato del parámetro
        public int VirtualDir { get; set; }     //Dirección virtual del parámetro
        public bool Reference { get; set; }     //Indica si es por referencia o no
        public List<Dimension> Dimensions { get; set; } //Dimensiones del parametro en caso que haya

        /// <summary>
        /// Método constructor de la clase parámetro
        /// </summary>
        /// <param name="t">Tipo de dato del parámetro</param>
        /// <param name="vDir">Dirección virtual</param>
        /// <param name="refer">Si es por referencia o no</param>
        public Parameter(int t, int vDir, bool refer)
        {
            Type = t;
            VirtualDir = vDir;
            Reference = refer;
            Dimensions = new List<Dimension>();
                
        }
    }
}
