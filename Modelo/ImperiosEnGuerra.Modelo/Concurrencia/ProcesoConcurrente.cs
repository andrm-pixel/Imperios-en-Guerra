using System;
using System.Threading.Tasks;

namespace ImperiosEnGuerra.Modelo.Concurrencia
{
    /// <summary>
    /// Referencia estable a un proceso concurrente iniciado. Vive en el Modelo.
    /// </summary>
    public sealed class ProcesoConcurrente
    {
        /// <summary>
        /// Obtiene id.
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Obtiene nombre.
        /// </summary>
        public string Nombre { get; }
        /// <summary>
        /// Obtiene finalizacion.
        /// </summary>
        public Task Finalizacion { get; }

        internal ProcesoConcurrente(
            Guid id,
            string nombre,
            Task finalizacion)
        {
            Id = id;
            Nombre = nombre;
            Finalizacion = finalizacion
                ?? throw new ArgumentNullException(nameof(finalizacion));
        }
    }
}
