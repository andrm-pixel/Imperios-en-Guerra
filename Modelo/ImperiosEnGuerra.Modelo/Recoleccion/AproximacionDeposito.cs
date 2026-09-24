using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Busca castillo libre y reparte aldeanos entre sus lados.
    /// </summary>
    public sealed class AproximacionDeposito
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
        public AproximacionDeposito()
            : this(new RutaMovimiento())
        {
        }

        /// <summary>
        /// Crea con ruta dada.
        /// </summary>
        /// <param name="planificadorMovimiento">Ruta a usar.</param>
        public AproximacionDeposito(
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
        /// <param name="permitirOrdenMovimientoActiva">True si admite orden mover activa.</param>
        /// <returns>Resultado.</returns>
        public ResultadoAproximacionDeposito Preparar(
            Partida partida,
            Guid aldeanoId,
            bool permitirOrdenMovimientoActiva = false)
        {
            if (partida == null)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No hay una partida activa.");
            }

            Aldeano aldeano =
                partida.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (aldeano == null)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "El Aldeano humano no existe o fue destruido.");
            }

            if (!aldeano.Disponible &&
                !(permitirOrdenMovimientoActiva &&
                  aldeano.OrdenActiva == TipoAccionJuego.Mover))
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "El Aldeano no está disponible.");
            }

            Castillo[] centros =
                partida.JugadorHumano.Edificios
                    .OfType<Castillo>()
                    .ToArray();

            if (centros.Length == 0)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No existe un Castillo humano para depositar.");
            }

            foreach (Castillo centro in centros)
            {
                if (Distancia(
                        aldeano.Coordenada,
                        centro.Coordenada) == 1)
                {
                    return ResultadoAproximacionDeposito.Exitoso(
                        centro.Coordenada,
                        aldeano.Coordenada,
                        Array.Empty<Coordenada>());
                }
            }

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            var candidatos =
                new List<(int Indice, Coordenada Centro, Coordenada Punto, ResultadoPlanMovimiento Plan)>();

            foreach (Castillo centro in centros)
            {
                for (int i = 0;
                     i < Direcciones.Length;
                     i++)
                {
                    (int X, int Y) direccion =
                        Direcciones[i];

                    Coordenada candidato =
                        new Coordenada(
                            centro.Coordenada.X + direccion.X,
                            centro.Coordenada.Y + direccion.Y);

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
                            (i, centro.Coordenada, candidato, plan));
                    }
                }
            }

            if (candidatos.Count == 0)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No existe una ruta accesible hasta un Castillo.",
                    true);
            }

            // Ruta mas corta primero; el indice rompe empates.
            var elegido =
                candidatos
                    .OrderBy(
                        c => c.Plan.Pasos.Count)
                    .ThenBy(c => c.Indice)
                    .First();

            return ResultadoAproximacionDeposito.Exitoso(
                elegido.Centro,
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
