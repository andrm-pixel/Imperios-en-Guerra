using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.IA
{
    /// <summary>
    /// Cerebro de la máquina. Vive en el Modelo y se ejecuta desde un worker
    /// del GestorProcesosConcurrentes cada pocos segundos.
    /// Cada turno: sus soldados cazan humanos en un radio de 7 casillas
    /// (caminan hasta el alcance y golpean); si no hay objetivo cerca,
    /// regresan junto a su Centro Urbano. Sin hilos propios.
    /// </summary>
    public sealed class InteligenciaMaquina
    {
        private const int RadioCaza = 7;
        private const int RadioGuardia = 2;

        /// <summary>
        /// Ejecuta turno.
        /// </summary>
        /// <param name="partida">El valor de partida.</param>
        /// <returns>Resultado de la operación.</returns>
        public ResultadoAccion EjecutarTurno(Partida partida)
        {
            if (partida == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");

            var bitacora = new StringBuilder();
            int acciones = 0;

            List<Unidad> soldados =
                partida.JugadorMaquina.Unidades
                    .Where(u => u is UnidadMilitar)
                    .ToList();

            foreach (Unidad soldado in soldados)
            {
                if (soldado == null ||
                    soldado.Coordenada == null ||
                    !soldado.EstaViva)
                {
                    continue;
                }

                if (OperacionAtaque.EsVictoriaMaquina(partida))
                    break;

                if (ActuarConUnidad(partida, soldado, bitacora))
                    acciones++;
            }

            if (OperacionAtaque.EsVictoriaMaquina(partida))
            {
                return ResultadoAccion.Exitoso(
                    $"IA: {bitacora}¡Victoria! La máquina destruyó el Centro Urbano o todas las unidades humanas.");
            }

            if (acciones == 0)
                return ResultadoAccion.Exitoso("IA sin acciones.");

            return ResultadoAccion.Exitoso($"IA: {bitacora}");
        }

        private static bool ActuarConUnidad(
            Partida partida,
            Unidad soldado,
            StringBuilder bitacora)
        {
            var objetivo = BuscarObjetivoCercano(partida, soldado);

            if (objetivo == null)
            {
                return RegresarAGuardia(partida, soldado, bitacora);
            }

            int distancia =
                Distancia(soldado.Coordenada, objetivo.Posicion);

            if (distancia <= soldado.AlcanceAtaque)
            {
                return Golpear(partida, soldado, objetivo, bitacora);
            }

            return Acercarse(partida, soldado, objetivo, bitacora);
        }

        private sealed class Objetivo
        {
            /// <summary>
            /// Representa el campo id.
            /// </summary>
            public Guid Id;
            /// <summary>
            /// Representa el campo posicion.
            /// </summary>
            public Coordenada Posicion;
            /// <summary>
            /// Representa el campo es unidad.
            /// </summary>
            public bool EsUnidad;
        }

        private static Objetivo BuscarObjetivoCercano(
            Partida partida,
            Unidad soldado)
        {
            Objetivo mejor = null;
            int mejorDistancia = RadioCaza + 1;

            foreach (Unidad unidad in partida.JugadorHumano.Unidades)
            {
                if (unidad == null ||
                    unidad.Coordenada == null ||
                    !unidad.EstaViva)
                {
                    continue;
                }

                int distancia =
                    Distancia(soldado.Coordenada, unidad.Coordenada);

                if (distancia < mejorDistancia)
                {
                    mejorDistancia = distancia;
                    mejor = new Objetivo
                    {
                        Id = unidad.Id,
                        Posicion = unidad.Coordenada,
                        EsUnidad = true
                    };
                }
            }

            foreach (Edificio edificio in partida.JugadorHumano.Edificios)
            {
                if (edificio == null ||
                    edificio.Coordenada == null ||
                    edificio.EstaDestruido)
                {
                    continue;
                }

                int distancia =
                    Distancia(soldado.Coordenada, edificio.Coordenada);

                if (distancia < mejorDistancia)
                {
                    mejorDistancia = distancia;
                    mejor = new Objetivo
                    {
                        Id = edificio.Id,
                        Posicion = edificio.Coordenada,
                        EsUnidad = false
                    };
                }
            }

            return mejorDistancia <= RadioCaza ? mejor : null;
        }

        private static bool Golpear(
            Partida partida,
            Unidad soldado,
            Objetivo objetivo,
            StringBuilder bitacora)
        {
            bool destruido;

            if (objetivo.EsUnidad)
            {
                Unidad victima =
                    partida.JugadorHumano.Unidades
                        .FirstOrDefault(u => u.Id == objetivo.Id);

                if (victima == null || !victima.EstaViva)
                    return false;

                destruido = victima.RecibirDano(soldado.PuntosAtaque);

                if (destruido)
                {
                    partida.JugadorHumano.EliminarUnidad(victima);

                    if (victima.Coordenada != null)
                    {
                        partida.JugadorHumano.Mapa.ObtenerCasilla(
                            victima.Coordenada.X,
                            victima.Coordenada.Y)?.Liberar();
                    }

                    bitacora.Append(
                        $"Destruye {victima.GetType().Name} humano. ");
                }
                else
                {
                    bitacora.Append(
                        $"Golpea {victima.GetType().Name} ({victima.Vida}). ");
                }

                return true;
            }

            Edificio edificio =
                partida.JugadorHumano.Edificios
                    .FirstOrDefault(e => e.Id == objetivo.Id);

            if (edificio == null || edificio.EstaDestruido)
                return false;

            destruido = edificio.RecibirDano(soldado.PuntosAtaque);

            if (destruido)
            {
                partida.JugadorHumano.EliminarEdificio(edificio);

                if (edificio.Coordenada != null)
                {
                    partida.JugadorHumano.Mapa.ObtenerCasilla(
                        edificio.Coordenada.X,
                        edificio.Coordenada.Y)?.Liberar();
                }

                bitacora.Append(
                    $"Destruye {edificio.GetType().Name} humano. ");
            }
            else
            {
                bitacora.Append(
                    $"Golpea {edificio.GetType().Name} ({edificio.Vida}). ");
            }

            return true;
        }

        private static bool Acercarse(
            Partida partida,
            Unidad soldado,
            Objetivo objetivo,
            StringBuilder bitacora)
        {
            Mapa mapa = partida.JugadorMaquina.Mapa;
            var buscador = new BuscadorRutaAStar();

            List<Coordenada> bloqueos = ObtenerBloqueos(partida, soldado);
            IReadOnlyList<Coordenada> mejoresPasos = null;

            for (int dx = -soldado.AlcanceAtaque; dx <= soldado.AlcanceAtaque; dx++)
            {
                for (int dy = -soldado.AlcanceAtaque; dy <= soldado.AlcanceAtaque; dy++)
                {
                    int dist = Math.Abs(dx) + Math.Abs(dy);

                    if (dist == 0 || dist > soldado.AlcanceAtaque)
                        continue;

                    var candidata = new Coordenada(
                        objetivo.Posicion.X + dx,
                        objetivo.Posicion.Y + dy);

                    if (!mapa.EstaDentroDeLimites(candidata))
                        continue;

                    var ruta = buscador.Buscar(
                        mapa,
                        soldado.Coordenada,
                        candidata,
                        bloqueos.Select(c => c).ToList());

                    if (!ruta.Encontrada || ruta.Pasos.Count == 0)
                        continue;

                    if (mejoresPasos == null ||
                        ruta.Pasos.Count < mejoresPasos.Count)
                    {
                        mejoresPasos = ruta.Pasos;
                    }
                }
            }

            if (mejoresPasos == null)
                return false;

            Coordenada paso = mejoresPasos[0];

            if (!PasoLibre(partida, soldado, paso))
                return false;

            soldado.EstablecerDestino(paso);
            bitacora.Append($"Avanza a ({paso.X},{paso.Y}). ");
            return true;
        }

        private static bool RegresarAGuardia(
            Partida partida,
            Unidad soldado,
            StringBuilder bitacora)
        {
            CentroUrbano centro =
                partida.JugadorMaquina.Edificios
                    .OfType<CentroUrbano>()
                    .FirstOrDefault();

            if (centro == null || centro.Coordenada == null)
                return false;

            if (Distancia(soldado.Coordenada, centro.Coordenada) <= RadioGuardia)
                return false;

            var señuelo = new Objetivo
            {
                Id = centro.Id,
                Posicion = centro.Coordenada,
                EsUnidad = false
            };

            return AcercarseHacia(partida, soldado, señuelo, 1, bitacora);
        }

        private static bool AcercarseHacia(
            Partida partida,
            Unidad soldado,
            Objetivo objetivo,
            int alcance,
            StringBuilder bitacora)
        {
            Mapa mapa = partida.JugadorMaquina.Mapa;
            var buscador = new BuscadorRutaAStar();

            List<Coordenada> bloqueos = ObtenerBloqueos(partida, soldado);
            IReadOnlyList<Coordenada> mejoresPasos = null;

            foreach (Coordenada candidata in CeldasAdyacentes(objetivo.Posicion))
            {
                if (!mapa.EstaDentroDeLimites(candidata))
                    continue;

                var ruta = buscador.Buscar(
                    mapa,
                    soldado.Coordenada,
                    candidata,
                    bloqueos.Select(c => c).ToList());

                if (!ruta.Encontrada || ruta.Pasos.Count == 0)
                    continue;

                if (mejoresPasos == null ||
                    ruta.Pasos.Count < mejoresPasos.Count)
                {
                    mejoresPasos = ruta.Pasos;
                }
            }

            if (mejoresPasos == null)
                return false;

            Coordenada paso = mejoresPasos[0];

            if (!PasoLibre(partida, soldado, paso))
                return false;

            soldado.EstablecerDestino(paso);
            bitacora.Append($"Patrulla a ({paso.X},{paso.Y}). ");
            return true;
        }

        private static IEnumerable<Coordenada> CeldasAdyacentes(Coordenada centro)
        {
            yield return new Coordenada(centro.X + 1, centro.Y);
            yield return new Coordenada(centro.X - 1, centro.Y);
            yield return new Coordenada(centro.X, centro.Y + 1);
            yield return new Coordenada(centro.X, centro.Y - 1);
        }

        private static List<Coordenada> ObtenerBloqueos(
            Partida partida,
            Unidad movil)
        {
            var bloqueos = new List<Coordenada>();

            foreach (Unidad unidad in partida.JugadorHumano.Unidades)
            {
                if (!ReferenceEquals(unidad, movil) &&
                    unidad.Coordenada != null)
                {
                    bloqueos.Add(unidad.Coordenada);
                }
            }

            foreach (Unidad unidad in partida.JugadorMaquina.Unidades)
            {
                if (!ReferenceEquals(unidad, movil) &&
                    unidad.Coordenada != null)
                {
                    bloqueos.Add(unidad.Coordenada);
                }
            }

            foreach (Edificio edificio in partida.JugadorHumano.Edificios)
            {
                if (edificio.Coordenada != null)
                    bloqueos.Add(edificio.Coordenada);
            }

            foreach (Edificio edificio in partida.JugadorMaquina.Edificios)
            {
                if (edificio.Coordenada != null)
                    bloqueos.Add(edificio.Coordenada);
            }

            return bloqueos;
        }

        private static bool PasoLibre(
            Partida partida,
            Unidad movil,
            Coordenada paso)
        {
            Mapa mapa = partida.JugadorMaquina.Mapa;

            if (!mapa.EstaDentroDeLimites(paso))
                return false;

            Casilla casilla = mapa.ObtenerCasilla(paso.X, paso.Y);

            if (casilla == null ||
                !casilla.EsTransitable ||
                casilla.EstaOcupada ||
                mapa.ObtenerRecursoEn(paso) != null)
            {
                return false;
            }

            foreach (Unidad unidad in partida.JugadorHumano.Unidades
                .Concat(partida.JugadorMaquina.Unidades))
            {
                if (!ReferenceEquals(unidad, movil) &&
                    unidad.Coordenada != null &&
                    unidad.Coordenada.X == paso.X &&
                    unidad.Coordenada.Y == paso.Y)
                {
                    return false;
                }
            }

            foreach (Edificio edificio in partida.JugadorHumano.Edificios
                .Concat(partida.JugadorMaquina.Edificios))
            {
                if (edificio.Coordenada != null &&
                    edificio.Coordenada.X == paso.X &&
                    edificio.Coordenada.Y == paso.Y)
                {
                    return false;
                }
            }

            return true;
        }

        private static int Distancia(Coordenada a, Coordenada b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
