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
    /// Busca casilla libre junto a la obra para el aldeano.
    /// </summary>
    public sealed class AproximacionConstruccion
    {
        private static readonly (int X, int Y)[] Direcciones =
        {
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1)
        };

        private readonly RutaMovimiento planificadorMovimiento;

        /// <summary>
        /// Crea con ruta base.
        /// </summary>
        public AproximacionConstruccion()
            : this(new RutaMovimiento())
        {
        }

        /// <summary>
        /// Crea con ruta dada.
        /// </summary>
        /// <param name="planificadorMovimiento">Ruta a usar.</param>
        public AproximacionConstruccion(
            RutaMovimiento planificadorMovimiento)
        {
            this.planificadorMovimiento =
                planificadorMovimiento
                ?? throw new ArgumentNullException(
                    nameof(planificadorMovimiento));
        }

        /// <summary>
        /// Prepara la aproximacion.
        /// </summary>
        /// <param name="partida">Partida actual.</param>
        /// <param name="aldeanoId">Id del aldeano.</param>
        /// <param name="obra">Punto de la obra.</param>
        /// <param name="permitirOrdenMovimientoActiva">True si admite orden mover activa.</param>
        /// <returns>Resultado.</returns>
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
                    "El Aldeano humano no existe o fue destruido.");
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

            // Ruta mas corta primero; el indice rompe empates.
            var elegido =
                candidatos
                    .OrderBy(
                        c => c.Plan.Pasos.Count)
                    .ThenBy(c => c.Indice)
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
