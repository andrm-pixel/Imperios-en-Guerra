using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Identifica la unidad y el destino logico; la operacion valida la solicitud.</summary>
    public sealed class SolicitudMovimiento : SolicitudAccion
    {
        /// <summary>
        /// Obtiene unidad id.
        /// </summary>
        public Guid UnidadId { get; }
        /// <summary>
        /// Obtiene destino.
        /// </summary>
        public Coordenada Destino { get; }

        /// <summary>
        /// Inicializa una nueva instancia de SolicitudMovimiento.
        /// </summary>
        /// <param name="unidadId">El valor de unidad id.</param>
        /// <param name="destino">El valor de destino.</param>
        public SolicitudMovimiento(Guid unidadId, Coordenada destino)
            : base(TipoAccionJuego.Mover)
        {
            UnidadId = unidadId;
            Destino = destino;
        }
    }
}
