using System;
using System.Threading.Tasks;

namespace ImperiosEnGuerra.Modelo.Concurrencia
{
    /// <summary>
    /// Referencia estable a un proceso concurrente iniciado. Vive en el Modelo.
    /// </summary>
    public sealed class ProcesoConcurrente
    {
        public Guid Id { get; }
        public string Nombre { get; }
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
