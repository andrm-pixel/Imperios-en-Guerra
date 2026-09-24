using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Representa entrenamiento pendiente dentro del modelo del juego.
    /// </summary>
    public sealed class EntrenamientoPendiente
    {
        /// <summary>
        /// Obtiene o establece id.
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Obtiene o establece tipo unidad.
        /// </summary>
        public string TipoUnidad { get; }
        /// <summary>
        /// Obtiene o establece punto reunion.
        /// </summary>
        public Coordenada PuntoReunion { get; }
        /// <summary>
        /// Obtiene o establece progreso.
        /// </summary>
        public int Progreso { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia de EntrenamientoPendiente.
        /// </summary>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <param name="puntoReunion">El valor de punto reunion.</param>
        public EntrenamientoPendiente(
            string tipoUnidad,
            Coordenada puntoReunion)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad))
            {
                throw new ArgumentException(
                    "El tipo de unidad es obligatorio.",
                    nameof(tipoUnidad));
            }

            Id = Guid.NewGuid();
            TipoUnidad = tipoUnidad;
            PuntoReunion = puntoReunion;
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
                throw new ArgumentOutOfRangeException(nameof(incremento));

            Progreso =
                Math.Min(
                    100,
                    Progreso + incremento);

            return Progreso;
        }
    }
}
