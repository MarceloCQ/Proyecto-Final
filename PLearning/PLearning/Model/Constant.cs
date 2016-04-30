namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase constante la cual incluye un nombre y una direccion virtual
    /// </summary>
    public class Constant
    {
        public string Name { get; set; }        //Nombre de la constante
        public int VirtualDir { get; set; }     //Dirección virtual asignada

        /// <summary>
        /// Constructor de la constante
        /// </summary>
        /// <param name="name">Nombre de la constante</param>
        /// <param name="virtualDir">Dirección virtual de la constante</param>
        public Constant (string name, int virtualDir)
        {
            Name = name;
            VirtualDir = virtualDir;
        }
    }
}
