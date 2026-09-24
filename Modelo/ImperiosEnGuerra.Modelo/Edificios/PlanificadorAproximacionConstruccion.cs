using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Calcula una ruta hasta una casilla libre adyacente a una obra. Las
    /// preferencias por unidad reducen colisiones cuando varios workers
    /// circulan cerca de la misma zona.
    /// </summary>
    public sealed class PlanificadorAproximacionConstruccion
    {
        private static readonly (int X, int Y)[] Direcciones =
        {
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1)
        };

        private readonly PlanificadorMovimiento planificadorMovimiento;

        /// <summary>
        /// Inicializa una nueva instancia de PlanificadorAproximacionConstruccion.
        /// </summary>
        public PlanificadorAproximacionConstruccion()
            : this(new PlanificadorMovimiento())
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de PlanificadorAproximacionConstruccion.
        /// </summary>
        /// <param name="planificadorMovimiento">El valor de planificador movimiento.</param>
        public PlanificadorAproximacionConstruccion(
            PlanificadorMovimiento planificadorMovimiento)
        {
            this.planificadorMovimiento =
                planificadorMovimiento
                ?? throw new ArgumentNullException(
                    nameof(planificadorMovimiento));
        }

        /// <summary>
        /// Ejecuta la operación preparar.
        /// </summary>
        /// <param name="partida">El valor de partida.</param>
        /// <param name="aldeanoId">El valor de aldeano id.</param>
        /// <param name="obra">El valor de obra.</param>
        /// <param name="permitirOrdenMovimientoActiva">El valor de permitir orden movimiento activa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ResultadoAproximacionConstruccion Preparar(
            Partida partida,
            Guid aldeanoId,
            Coordenada obra,
            bool permitirOrdenMovimientoActiva = false)
        {
            if (partida == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No hay una partida activa.");
            }

            Aldeano aldeano =
                partida.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (aldeano == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No existe un Aldeano humano con ese ID.");
            }

            if (!aldeano.Disponible &&
                !(permitirOrdenMovimientoActiva &&
                  aldeano.OrdenActiva == TipoAccionJuego.Mover))
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "El Aldeano no está disponible.");
            }

            if (obra == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "La posición de obra es obligatoria.");
            }

            if (Distancia(
                    aldeano.Coordenada,
                    obra) == 1)
            {
                return ResultadoAproximacionConstruccion.Exitoso(
                    aldeano.Coordenada,
                    Array.Empty<Coordenada>());
            }

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            var candidatos =
                new List<(int Indice, Coordenada Punto, ResultadoPlanMovimiento Plan)>();

            for (int i = 0;
                 i < Direcciones.Length;
                 i++)
            {
                (int X, int Y) direccion =
                    Direcciones[i];

                Coordenada candidato =
                    new Coordenada(
                        obra.X + direccion.X,
                        obra.Y + direccion.Y);

                if (!mapa.EstaDentroDeLimites(
                        candidato))
                {
                    continue;
                }

                ResultadoPlanMovimiento plan =
                    planificadorMovimiento.Preparar(
                        partida,
                        new SolicitudMovimiento(
                            aldeano.Id,
                            candidato),
                        permitirOrdenMovimientoActiva);

                if (plan.Exito)
                {
                    candidatos.Add(
                        (i, candidato, plan));
                }
            }

            if (candidatos.Count == 0)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No existe una casilla accesible junto a la obra.",
                    true);
            }

            int inicio =
                PreferenciaCasillaInteraccion
                    .ObtenerIndiceInicial(
                        partida,
                        aldeano.Id,
                        Direcciones.Length);

            // Camino más corto primero; la preferencia rotativa solo
            // desempata para no alargar rutas hacia la obra.
            var elegido =
                candidatos
                    .OrderBy(
                        c => c.Plan.Pasos.Count)
                    .ThenBy(
                        c =>
                            (c.Indice - inicio +
                             Direcciones.Length) %
                            Direcciones.Length)
                    .First();

            return ResultadoAproximacionConstruccion.Exitoso(
                elegido.Punto,
                elegido.Plan.Pasos);
        }

        private static int Distancia(
            Coordenada primera,
            Coordenada segunda)
        {
            return Math.Abs(
                       primera.X -
                       segunda.X)
                   +
                   Math.Abs(
                       primera.Y -
                       segunda.Y);
        }
    }
}
