using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Modelo.Acciones;

namespace ImperiosEnGuerra.Modelo.Concurrencia
{
    /// <summary>
    /// Nucleo de concurrencia del juego en el Modelo.
    /// Lanza trabajos en ThreadPool, permite cancelarlos
    /// y publica resultados en cola segura para consumo posterior.
    /// </summary>
    public sealed class TareasJuego : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> cancelaciones =
            new ConcurrentDictionary<Guid, CancellationTokenSource>();

        private readonly ConcurrentQueue<ResultadoProcesoConcurrente> resultados =
            new ConcurrentQueue<ResultadoProcesoConcurrente>();

        private readonly ConcurrentDictionary<Guid, ResultadoProcesoConcurrente> resultadosPorId =
            new ConcurrentDictionary<Guid, ResultadoProcesoConcurrente>();

        private int cerrado;

        /// <summary>
        /// Inicia un trabajo.
        /// </summary>
        /// <param name="nombre">Nombre del proceso.</param>
        /// <param name="trabajo">Trabajo a ejecutar.</param>
        /// <returns>Resultado.</returns>
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
                throw new ObjectDisposedException(nameof(TareasJuego));

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
        /// Cancela un proceso.
        /// </summary>
        /// <param name="procesoId">Id del proceso.</param>
        /// <returns>True si ok, false si no.</returns>
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
        /// Cancela todo.
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
        /// Lee un resultado por id.
        /// </summary>
        /// <param name="procesoId">Id del proceso.</param>
        /// <param name="resultado">Resultado final.</param>
        /// <returns>True si ok, false si no.</returns>
        public bool IntentarObtenerResultado(
            Guid procesoId,
            out ResultadoProcesoConcurrente resultado)
        {
            return resultadosPorId.TryRemove(
                procesoId,
                out resultado);
        }

        /// <summary>
        /// Lee un resultado.
        /// </summary>
        /// <param name="resultado">Resultado final.</param>
        /// <returns>True si ok, false si no.</returns>
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
        /// Procesos activos.
        /// </summary>
        public int ProcesosActivos
        {
            get { return cancelaciones.Count; }
        }

        /// <summary>
        /// Resultados pendientes.
        /// </summary>
        public int ResultadosPendientes
        {
            get { return resultadosPorId.Count; }
        }

        /// <summary>
        /// Libera recursos.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref cerrado, 1) != 0)
                return;

            CancelarTodos();
        }
    }
}
