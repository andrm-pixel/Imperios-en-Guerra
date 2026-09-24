namespace ImperiosEnGuerra.Modelo.Map
{
    /// <summary>
    /// Representa una posicion logica mediante dos componentes enteros de solo lectura.
    /// </summary>
    public class Coordenada
    {
        /// <summary>
        /// Componente horizontal de la posicion logica.
        /// </summary>
        public int X { get; }
        /// <summary>
        /// Componente vertical de la posicion logica.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Conserva las componentes recibidas sin comprobar limites de un mapa.
        /// </summary>
        /// <param name="x">Componente horizontal.</param>
        /// <param name="y">Componente vertical.</param>
        public Coordenada(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
