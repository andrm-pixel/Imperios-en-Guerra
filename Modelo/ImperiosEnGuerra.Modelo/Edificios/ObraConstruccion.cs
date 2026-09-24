using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Estado logico de una construccion todavia no terminada.
    /// </summary>
    public sealed class ObraConstruccion
    {
        /// <summary>
        /// Obtiene o establece id.
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Obtiene o establece aldeano id.
        /// </summary>
        public Guid AldeanoId { get; }
        /// <summary>
        /// Obtiene o establece tipo edificio.
        /// </summary>
        public string TipoEdificio { get; }
        /// <summary>
        /// Obtiene o establece coordenada.
        /// </summary>
        public Coordenada Coordenada { get; }
        /// <summary>
        /// Obtiene o establece progreso.
        /// </summary>
        public int Progreso { get; private set; }

        /// <summary>
        /// Obtiene terminada.
        /// </summary>
        public bool Terminada =>
            Progreso >= 100;

        /// <summary>
        /// Inicializa una nueva instancia de ObraConstruccion.
        /// </summary>
        /// <param name="aldeanoId">El valor de aldeano id.</param>
        /// <param name="tipoEdificio">El valor de tipo edificio.</param>
        /// <param name="coordenada">El valor de coordenada.</param>
        public ObraConstruccion(
            Guid aldeanoId,
            string tipoEdificio,
            Coordenada coordenada)
        {
            if (string.IsNullOrWhiteSpace(tipoEdificio))
            {
                throw new ArgumentException(
                    "El tipo de edificio es obligatorio.",
                    nameof(tipoEdificio));
            }

            if (coordenada == null)
            {
                throw new ArgumentNullException(
                    nameof(coordenada));
            }

            Id = Guid.NewGuid();
            AldeanoId = aldeanoId;
            TipoEdificio = tipoEdificio;
            Coordenada = coordenada;
            Progreso = 0;
        }

        /// <summary>
        /// Ejecuta la operacion avanzar.
        /// </summary>
        /// <param name="incremento">El valor de incremento.</param>
        /// <returns>Resultado de la operacion.</returns>
        public int Avanzar(
            int incremento)
        {
            if (incremento <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incremento));
            }

            Progreso =
                Math.Min(
                    100,
                    Progreso + incremento);

            return Progreso;
        }
    }
}
