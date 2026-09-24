namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Intencion minima; no identifica aun ejecutor, objetivo ni destino.</summary>
    public class SolicitudAccion
    {
        /// <summary>
        /// Obtiene tipo.
        /// </summary>
        public TipoAccionJuego Tipo { get; }

        /// <summary>
        /// Inicializa una nueva instancia de SolicitudAccion.
        /// </summary>
        /// <param name="tipo">El valor de tipo.</param>
        public SolicitudAccion(TipoAccionJuego tipo)
        {
            Tipo = tipo;
        }
    }
}
