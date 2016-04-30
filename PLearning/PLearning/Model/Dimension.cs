namespace PLearning_Backend.Model
{
    /// <summary>
    /// Clase dimensión la cual guarda el tamaño de la dimensión así como la "m" para indexamiento
    /// </summary>
    public class Dimension
    {
        public int Dim { get; set;}         //Tamaño de la dimensión
        public int M { get; set; }          //M que se calcula para la fórmula de indexamiento

        /// <summary>
        /// Método constructor de la clase dimensión
        /// </summary>
        /// <param name="d">Tamaño de la dimensión</param>
        /// <param name="m">M para indexamiento</param>
        public Dimension(int d, int m)
        {
            Dim = d;
            M = m;
        }

    }
}
