using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase Memory dir que sirve para guardar el Tipo de variable, su tipo de datos y su dirección real
    /// </summary>
    public class MemoryDir
    {
        public int VariableType { get; set; }       //Tipo de la variable (Temporal, Local, etc)
        public int DataType { get; set; }           //Tipo de dato
        public int RealDir { get; set; }            //Dirección real, física

        /// <summary>
        /// Método constructor de la clase MemoryDir
        /// </summary>
        /// <param name="vtype">Tipo de la variable</param>
        /// <param name="dtype">Tipo de dato</param>
        /// <param name="rdir">Dirección real</param>
        public MemoryDir(int vtype, int dtype, int rdir)
        {
            DataType = dtype;
            VariableType = vtype;
            RealDir = rdir;        
        }
    }
}
