using System;
using ImperiosEnGuerra.Modelo.Acciones;

namespace ImperiosEnGuerra.Modelo.Concurrencia
{
    /// <summary>
    /// Define los valores del tipo estado proceso concurrente.
    /// </summary>
    public enum EstadoProcesoConcurrente
    {
        /// <summary>
        /// Representa el valor completado.
        /// </summary>
        Completado,
        /// <summary>
        /// Representa el valor cancelado.
        /// </summary>
        Cancelado,
        /// <summary>
        /// Representa el valor fallido.
        /// </summary>
        Fallido
    }

    /// <summary>
    /// Mensaje inmutable producido por un worker del Modelo y listo para ser
    /// consumido desde otra capa, incluido el Main Thread de Unity.
    /// </summary>
    public sealed class ResultadoProcesoConcurrente
    {
        /// <summary>
        /// Obtiene proceso id.
        /// </summary>
        public Guid ProcesoId { get; }
        /// <summary>
        /// Obtiene nombre.
        /// </summary>
        public string Nombre { get; }
        /// <summary>
        /// Obtiene estado.
        /// </summary>
        public EstadoProcesoConcurrente Estado { get; }
        /// <summary>
        /// Obtiene hilo trabajo id.
        /// </summary>
        public int HiloTrabajoId { get; }
        /// <summary>
        /// Obtiene resultado.
        /// </summary>
        public ResultadoAccion? Resultado { get; }
        /// <summary>
        /// Obtiene error tecnico.
        /// </summary>
        public string? ErrorTecnico { get; }

        private ResultadoProcesoConcurrente(
            Guid procesoId,
            string nombre,
            EstadoProcesoConcurrente estado,
            int hiloTrabajoId,
            ResultadoAccion? resultado,
            string? errorTecnico)
        {
            ProcesoId = procesoId;
            Nombre = nombre;
            Estado = estado;
            HiloTrabajoId = hiloTrabajoId;
            Resultado = resultado;
            ErrorTecnico = errorTecnico;
        }

        internal static ResultadoProcesoConcurrente Completado(
            Guid procesoId,
            string nombre,
            int hiloTrabajoId,
            ResultadoAccion resultado)
        {
            return new ResultadoProcesoConcurrente(
                procesoId,
                nombre,
                EstadoProcesoConcurrente.Completado,
                hiloTrabajoId,
                resultado,
                null);
        }

        internal static ResultadoProcesoConcurrente Cancelado(
            Guid procesoId,
            string nombre,
            int hiloTrabajoId)
        {
            return new ResultadoProcesoConcurrente(
                procesoId,
                nombre,
                EstadoProcesoConcurrente.Cancelado,
                hiloTrabajoId,
                null,
                null);
        }

        internal static ResultadoProcesoConcurrente Fallido(
            Guid procesoId,
            string nombre,
            int hiloTrabajoId,
            string errorTecnico)
        {
            return new ResultadoProcesoConcurrente(
                procesoId,
                nombre,
                EstadoProcesoConcurrente.Fallido,
                hiloTrabajoId,
                null,
                errorTecnico);
        }
    }
}
