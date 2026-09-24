using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Representa solicitud entrenamiento dentro del modelo del juego.
    /// </summary>
    public sealed class SolicitudEntrenamiento : SolicitudAccion
    {
        /// <summary>
        /// Obtiene edificio origen.
        /// </summary>
        public Coordenada EdificioOrigen { get; }
        /// <summary>
        /// Obtiene tipo unidad.
        /// </summary>
        public string TipoUnidad { get; }
        /// <summary>
        /// Obtiene destino.
        /// </summary>
        public Coordenada Destino { get; }

        /// <summary>
        /// Inicializa una nueva instancia de SolicitudEntrenamiento.
        /// </summary>
        /// <param name="edificioOrigen">El valor de edificio origen.</param>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <param name="destino">El valor de destino.</param>
        public SolicitudEntrenamiento(
            Coordenada edificioOrigen,
            string tipoUnidad,
            Coordenada destino)
            : base(TipoAccionJuego.Entrenar)
        {
            EdificioOrigen = edificioOrigen;
            TipoUnidad = tipoUnidad;
            Destino = destino;
        }
    }
}
