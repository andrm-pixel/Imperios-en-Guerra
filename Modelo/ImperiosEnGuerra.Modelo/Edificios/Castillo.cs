using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Representa centro urbano dentro del modelo del juego.
    /// </summary>
    public class Castillo : Edificio
    {
        private readonly object sincronizacion =
            new object();

        private readonly List<EntrenamientoPendiente>
            colaEntrenamiento =
                new List<EntrenamientoPendiente>();

        /// <summary>
        /// Obtiene esta entrenando.
        /// </summary>
        public bool EstaEntrenando
        {
            get
            {
                lock (sincronizacion)
                {
                    return colaEntrenamiento.Count > 0;
                }
            }
        }

        /// <summary>
        /// Obtiene tipo unidad entrenando.
        /// </summary>
        public string TipoUnidadEntrenando
        {
            get
            {
                lock (sincronizacion)
                {
                    return colaEntrenamiento.Count == 0
                        ? null
                        : colaEntrenamiento[0].TipoUnidad;
                }
            }
        }

        /// <summary>
        /// Obtiene cola entrenamiento.
        /// </summary>
        public IReadOnlyList<EntrenamientoPendiente> ColaEntrenamiento
        {
            get
            {
                lock (sincronizacion)
                {
                    return colaEntrenamiento
                        .ToArray();
                }
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia de Castillo.
        /// </summary>
        /// <param name="coordenada">El valor de coordenada.</param>
        public Castillo(
            Coordenada coordenada)
            : base(coordenada)
        {
        }

        /// <summary>
        /// Ejecuta la operacion encolar entrenamiento.
        /// </summary>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <param name="puntoReunion">El valor de punto reunion.</param>
        /// <returns>Resultado de la operacion.</returns>
        public EntrenamientoPendiente EncolarEntrenamiento(
            string tipoUnidad,
            Coordenada puntoReunion = null)
        {
            var pendiente =
                new EntrenamientoPendiente(
                    tipoUnidad,
                    puntoReunion);

            lock (sincronizacion)
            {
                colaEntrenamiento.Add(
                    pendiente);
            }

            return pendiente;
        }

        /// <summary>
        /// Ejecuta la operacion es primero.
        /// </summary>
        /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
        /// <returns>true si la operacion tuvo exito; false en caso contrario.</returns>
        public bool EsPrimero(
            Guid entrenamientoId)
        {
            lock (sincronizacion)
            {
                return colaEntrenamiento.Count > 0 &&
                       colaEntrenamiento[0].Id ==
                       entrenamientoId;
            }
        }

        /// <summary>
        /// Ejecuta la operacion avanzar entrenamiento.
        /// </summary>
        /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
        /// <param name="incremento">El valor de incremento.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ResultadoProgresoEntrenamiento AvanzarEntrenamiento(
            Guid entrenamientoId,
            int incremento)
        {
            lock (sincronizacion)
            {
                if (colaEntrenamiento.Count == 0 ||
                    colaEntrenamiento[0].Id != entrenamientoId)
                {
                    return ResultadoProgresoEntrenamiento.Fallido(
                        "La orden no está al frente de la cola.");
                }

                int progreso =
                    colaEntrenamiento[0]
                        .Avanzar(
                            incremento);

                return ResultadoProgresoEntrenamiento.Exitoso(
                    progreso);
            }
        }

        /// <summary>
        /// Completa entrenamiento.
        /// </summary>
        /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
        /// <returns>true si la operacion tuvo exito; false en caso contrario.</returns>
        public bool CompletarEntrenamiento(
            Guid entrenamientoId)
        {
            lock (sincronizacion)
            {
                if (colaEntrenamiento.Count == 0 ||
                    colaEntrenamiento[0].Id != entrenamientoId ||
                    colaEntrenamiento[0].Progreso < 100)
                {
                    return false;
                }

                colaEntrenamiento.RemoveAt(0);
                return true;
            }
        }

        /// <summary>
        /// Cancela entrenamiento.
        /// </summary>
        /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
        /// <returns>true si la operacion tuvo exito; false en caso contrario.</returns>
        public bool CancelarEntrenamiento(
            Guid entrenamientoId)
        {
            lock (sincronizacion)
            {
                EntrenamientoPendiente pendiente =
                    colaEntrenamiento.FirstOrDefault(
                        p => p.Id == entrenamientoId);

                return pendiente != null &&
                       colaEntrenamiento.Remove(
                           pendiente);
            }
        }

        // Compatibilidad con llamadas historicas.
        /// <summary>
        /// Inicia entrenamiento.
        /// </summary>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <returns>true si la operacion tuvo exito; false en caso contrario.</returns>
        public bool IniciarEntrenamiento(
            string tipoUnidad)
        {
            lock (sincronizacion)
            {
                if (colaEntrenamiento.Count > 0 ||
                    string.IsNullOrWhiteSpace(tipoUnidad))
                {
                    return false;
                }

                colaEntrenamiento.Add(
                    new EntrenamientoPendiente(
                        tipoUnidad,
                        null));

                return true;
            }
        }

        /// <summary>
        /// Completa entrenamiento.
        /// </summary>
        public void CompletarEntrenamiento()
        {
            lock (sincronizacion)
            {
                if (colaEntrenamiento.Count > 0)
                {
                    colaEntrenamiento.RemoveAt(0);
                }
            }
        }
    }
}
