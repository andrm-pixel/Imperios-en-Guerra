namespace ImperiosEnGuerra.Modelo.Map
{
    /// <summary>
    /// Representa una celda del mapa con posicion, transitabilidad y ocupacion independientes.
    /// </summary>
    public class Casilla
    {
        /// <summary>
        /// Posicion logica recibida al crear la casilla.
        /// </summary>
        public Coordenada Posicion { get; }
        /// <summary>
        /// Indica la transitabilidad configurada para la casilla.
        /// </summary>
        public bool EsTransitable { get; private set; }
        /// <summary>
        /// Indica si la casilla fue marcada como ocupada.
        /// </summary>
        public bool EstaOcupada { get; private set; }

        /// <summary>
        /// Crea una casilla desocupada con la posicion y transitabilidad indicadas.
        /// </summary>
        /// <param name="posicion">Posicion logica que se conserva sin validacion.</param>
        /// <param name="esTransitable">Transitabilidad inicial.</param>
        public Casilla(Coordenada posicion, bool esTransitable)
        {
            Posicion = posicion;
            EsTransitable = esTransitable;
            EstaOcupada = false;
        }

        /// <summary>
        /// Actualiza la transitabilidad sin cambiar la ocupacion.
        /// </summary>
        /// <param name="esTransitable">Nuevo valor de transitabilidad.</param>
        public void CambiarTransitabilidad(bool esTransitable)
        {
            EsTransitable = esTransitable;
        }

        /// <summary>
        /// Marca la casilla como ocupada si estaba libre, independientemente de su transitabilidad.
        /// </summary>
        /// <returns>true si paso a estar ocupada; false si ya lo estaba.</returns>
        public bool Ocupar()
        {
            if (EstaOcupada)
            {
                return false;
            }

            EstaOcupada = true;
            return true;
        }

        /// <summary>
        /// Deja la casilla desocupada, incluso si ya estaba libre.
        /// </summary>
        public void Liberar()
        {
            EstaOcupada = false;
        }
    }
}
