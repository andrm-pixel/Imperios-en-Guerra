using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Representa resultado spawn entrenamiento dentro del modelo del juego.
    /// </summary>
    public sealed class ResultadoSpawnEntrenamiento
    {
        /// <summary>
        /// Obtiene exito.
        /// </summary>
        public bool Exito { get; }
        /// <summary>
        /// Obtiene mensaje.
        /// </summary>
        public string Mensaje { get; }
        /// <summary>
        /// Obtiene unidad id.
        /// </summary>
        public Guid UnidadId { get; }
        /// <summary>
        /// Obtiene coordenada.
        /// </summary>
        public Coordenada Coordenada { get; }

        private ResultadoSpawnEntrenamiento(
            bool exito,
            string mensaje,
            Guid unidadId,
            Coordenada coordenada)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            UnidadId = unidadId;
            Coordenada = coordenada;
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="unidadId">El valor de unidad id.</param>
        /// <param name="coordenada">El valor de coordenada.</param>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoSpawnEntrenamiento Exitoso(
            Guid unidadId,
            Coordenada coordenada,
            string tipoUnidad)
        {
            return new ResultadoSpawnEntrenamiento(
                true,
                $"Unidad {tipoUnidad} entrenada correctamente.",
                unidadId,
                coordenada);
        }

        /// <summary>
        /// Crea un resultado fallido el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoSpawnEntrenamiento Fallido(
            string mensaje)
        {
            return new ResultadoSpawnEntrenamiento(
                false,
                mensaje,
                Guid.Empty,
                null);
        }
    }
}
