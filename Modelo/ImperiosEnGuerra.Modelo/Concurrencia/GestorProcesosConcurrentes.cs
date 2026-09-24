using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Modelo.Acciones;

namespace ImperiosEnGuerra.Modelo.Concurrencia
{
    /// <summary>
    /// Núcleo de concurrencia del juego. Vive en el Modelo.
    /// Inicia trabajos reales en ThreadPool mediante Task.Run, permite cancelarlos
    /// y publica sus resultados en una cola thread-safe para consumo posterior.
    /// No depende de UnityEngine. Ninguna otra capa debe crear Thread/Task.
    /// </summary>
    public sealed class GestorProcesosConcurrentes : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> cancelaciones =
            new ConcurrentDictionary<Guid, CancellationTokenSource>();

        private readonly ConcurrentQueue<ResultadoProcesoConcurrente> resultados =
            new ConcurrentQueue<ResultadoProcesoConcurrente>();

        private readonly ConcurrentDictionary<Guid, ResultadoProcesoConcurrente> resultadosPorId =
            new ConcurrentDictionary<Guid, ResultadoProcesoConcurrente>();

        private int cerrado;

        /// <summary>
        /// Inicia el elemento solicitado.
        /// </summary>
        /// <param name="nombre">El valor de nombre.</param>
        /// <param name="trabajo">El valor de trabajo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ProcesoConcurrente Iniciar(
            string nombre,
            Func<CancellationToken, ResultadoAccion> trabajo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre del proceso es obligatorio.",
                    nameof(nombre));

            if (trabajo == null)
                throw new ArgumentNullException(nameof(trabajo));

            if (Volatile.Read(ref cerrado) != 0)
                throw new ObjectDisposedException(nameof(GestorProcesosConcurrentes));

            Guid procesoId = Guid.NewGuid();
            var cancelacion = new CancellationTokenSource();

            if (!cancelaciones.TryAdd(procesoId, cancelacion))
                throw new InvalidOperationException(
                    "No se pudo registrar el proceso concurrente.");

            Task finalizacion = Task.Run(() =>
            {
                int hiloTrabajo = Thread.CurrentThread.ManagedThreadId;

                try
                {
                    cancelacion.Token.ThrowIfCancellationRequested();

                    ResultadoAccion resultado =
                        trabajo(cancelacion.Token);

                    PublicarResultado(
                        ResultadoProcesoConcurrente.Completado(
                            procesoId,
                            nombre,
                            hiloTrabajo,
                            resultado));
                }
                catch (OperationCanceledException)
                {
                    PublicarResultado(
                        ResultadoProcesoConcurrente.Cancelado(
                            procesoId,
                            nombre,
                            hiloTrabajo));
                }
                catch (Exception ex)
                {
                    PublicarResultado(
                        ResultadoProcesoConcurrente.Fallido(
                            procesoId,
                            nombre,
                            hiloTrabajo,
                            ex.Message));
                }
                finally
                {
                    cancelaciones.TryRemove(
                        procesoId,
                        out CancellationTokenSource registrada);

                    registrada?.Dispose();
                }
            });

            return new ProcesoConcurrente(
                procesoId,
                nombre,
                finalizacion);
        }

        /// <summary>
        /// Cancela el elemento solicitado.
        /// </summary>
        /// <param name="procesoId">El valor de proceso id.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public bool Cancelar(Guid procesoId)
        {
            if (!cancelaciones.TryGetValue(
                procesoId,
                out CancellationTokenSource cancelacion))
            {
                return false;
            }

            cancelacion.Cancel();
            return true;
        }

        /// <summary>
        /// Cancela todos.
        /// </summary>
        public void CancelarTodos()
        {
            foreach (CancellationTokenSource cancelacion
                     in cancelaciones.Values)
            {
                cancelacion.Cancel();
            }
        }

        /// <summary>
        /// Intenta obtener resultado.
        /// </summary>
        /// <param name="procesoId">El valor de proceso id.</param>
        /// <param name="resultado">El valor de resultado.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public bool IntentarObtenerResultado(
            Guid procesoId,
            out ResultadoProcesoConcurrente resultado)
        {
            return resultadosPorId.TryRemove(
                procesoId,
                out resultado);
        }

        /// <summary>
        /// Intenta obtener resultado.
        /// </summary>
        /// <param name="resultado">El valor de resultado.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public bool IntentarObtenerResultado(
            out ResultadoProcesoConcurrente resultado)
        {
            while (resultados.TryDequeue(
                out ResultadoProcesoConcurrente candidato))
            {
                if (resultadosPorId.TryRemove(
                    candidato.ProcesoId,
                    out resultado))
                {
                    return true;
                }
            }

            resultado = null!;
            return false;
        }

        private void PublicarResultado(
            ResultadoProcesoConcurrente resultado)
        {
            resultadosPorId[resultado.ProcesoId] = resultado;
            resultados.Enqueue(resultado);
        }

        /// <summary>
        /// Obtiene procesos activos.
        /// </summary>
        public int ProcesosActivos
        {
            get { return cancelaciones.Count; }
        }

        /// <summary>
        /// Obtiene resultados pendientes.
        /// </summary>
        public int ResultadosPendientes
        {
            get { return resultadosPorId.Count; }
        }

        /// <summary>
        /// Ejecuta la operación dispose.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref cerrado, 1) != 0)
                return;

            CancelarTodos();
        }
    }
}
