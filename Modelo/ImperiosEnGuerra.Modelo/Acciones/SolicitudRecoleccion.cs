using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Representa solicitud recoleccion dentro del modelo del juego.
    /// </summary>
    public sealed class SolicitudRecoleccion : SolicitudAccion
    {
        /// <summary>
        /// Obtiene aldeano id.
        /// </summary>
        public Guid AldeanoId { get; }
        /// <summary>
        /// Obtiene objetivo.
        /// </summary>
        public Coordenada Objetivo { get; }

        /// <summary>
        /// Inicializa una nueva instancia de SolicitudRecoleccion.
        /// </summary>
        /// <param name="aldeanoId">El valor de aldeano id.</param>
        /// <param name="objetivo">El valor de objetivo.</param>
        public SolicitudRecoleccion(Guid aldeanoId, Coordenada objetivo)
            : base(TipoAccionJuego.Recolectar)
        {
            AldeanoId = aldeanoId;
            Objetivo = objetivo;
        }
    }
}
