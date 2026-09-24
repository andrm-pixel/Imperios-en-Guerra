using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Concurrencia;
using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Servicios
{
    /// <summary>
    /// Ordena a todo el ejército humano atacar a su objetivo enemigo más
    /// cercano. Cada unidad pelea en su propio worker concurrente, por lo
    /// que entrenar, recolectar y construir siguen funcionando en simultáneo.
    /// Sin hilos propios: delega en ServicioAccionesConcurrentes.
    /// </summary>
    public static class NucleoBatalla
    {
        /// <summary>
        /// Identifica qué unidad ataca en cada proceso iniciado.
        /// </summary>
        public sealed class ProcesoBatalla
        {
            public Guid UnidadId { get; }
            public Guid ProcesoId { get; }

            public ProcesoBatalla(Guid unidadId, Guid procesoId)
            {
                UnidadId = unidadId;
                ProcesoId = procesoId;
            }
        }

        /// <summary>
        /// Inicia un worker de ataque por cada unidad militar humana con un
        /// objetivo enemigo en el mapa. Devuelve los procesos iniciados.
        /// </summary>
        public static IReadOnlyList<ProcesoBatalla> IniciarBatalla(
            EstadoPartidaService estado,
            ServicioAccionesConcurrentes acciones)
        {
            if (estado == null)
                throw new ArgumentNullException(nameof(estado));

            if (acciones == null)
                throw new ArgumentNullException(nameof(acciones));

            Partida partida = estado.ObtenerPartida();

            if (partida == null)
                return Array.Empty<ProcesoBatalla>();

            var procesos = new List<ProcesoBatalla>();

            foreach (Unidad unidad in partida.JugadorHumano.Unidades.ToList())
            {
                if (!(unidad is UnidadMilitar) ||
                    unidad.Coordenada == null)
                {
                    continue;
                }

                Guid? objetivo =
                    BuscarEnemigoMasCercano(partida, unidad.Coordenada);

                if (!objetivo.HasValue)
                    continue;

                ProcesoConcurrente proceso = acciones.IniciarAtaque(
                    new AtacarRequest
                    {
                        AtacanteId = unidad.Id.ToString("D"),
                        ObjetivoId = objetivo.Value.ToString("D")
                    });

                procesos.Add(new ProcesoBatalla(unidad.Id, proceso.Id));
            }

            return procesos;
        }

        private static Guid? BuscarEnemigoMasCercano(
            Partida partida,
            Coordenada desde)
        {
            Guid? mejor = null;
            int mejorDistancia = int.MaxValue;

            foreach (Unidad unidad in partida.JugadorMaquina.Unidades)
            {
                if (unidad == null || unidad.Coordenada == null)
                    continue;

                int distancia = Distancia(desde, unidad.Coordenada);

                if (distancia < mejorDistancia)
                {
                    mejorDistancia = distancia;
                    mejor = unidad.Id;
                }
            }

            foreach (var edificio in partida.JugadorMaquina.Edificios)
            {
                if (edificio == null || edificio.Coordenada == null)
                    continue;

                int distancia = Distancia(desde, edificio.Coordenada);

                if (distancia < mejorDistancia)
                {
                    mejorDistancia = distancia;
                    mejor = edificio.Id;
                }
            }

            return mejor;
        }

        private static int Distancia(Coordenada a, Coordenada b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
