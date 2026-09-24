using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Representa solicitud construccion dentro del modelo del juego.
    /// </summary>
    public sealed class SolicitudConstruccion : SolicitudAccion
    {
        /// <summary>
        /// Obtiene aldeano id.
        /// </summary>
        public Guid AldeanoId { get; }
        /// <summary>
        /// Obtiene tipo edificio.
        /// </summary>
        public string TipoEdificio { get; }
        /// <summary>
        /// Obtiene destino.
        /// </summary>
        public Coordenada Destino { get; }

        /// <summary>
        /// Inicializa una nueva instancia de SolicitudConstruccion.
        /// </summary>
        /// <param name="aldeanoId">El valor de aldeano id.</param>
        /// <param name="tipoEdificio">El valor de tipo edificio.</param>
        /// <param name="destino">El valor de destino.</param>
        public SolicitudConstruccion(
            Guid aldeanoId,
            string tipoEdificio,
            Coordenada destino)
            : base(TipoAccionJuego.Construir)
        {
            AldeanoId = aldeanoId;
            TipoEdificio = tipoEdificio;
            Destino = destino;
        }
    }
}
