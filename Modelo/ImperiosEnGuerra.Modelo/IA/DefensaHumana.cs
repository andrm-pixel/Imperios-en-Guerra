using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.IA
{
    /// <summary>
    /// Reacciones defensivas automaticas del bando humano.
    /// Solo interviene sobre unidades disponibles: no interrumpe una orden
    /// que el jugador haya iniciado. La decision vive en el Modelo y no
    /// depende de Unity.
    /// </summary>
    public static class DefensaHumana
    {
        /// <summary>
        /// Distancia Manhattan a partir de la cual un Aldeano considera
        /// que una unidad militar enemiga es una amenaza inmediata.
        /// </summary>
        public const int RadioPanicoAldeano = 3;

        /// <summary>
        /// Ejecuta la reaccion de huida de los Aldeanos humanos.
        /// </summary>
        /// <param name="partida">Partida activa.</param>
        /// <param name="acciones">Numero de Aldeanos que cambiaron de posicion.</param>
        /// <returns>Resultado de la reaccion defensiva.</returns>
        public static ResultadoAccion Ejecutar(
            Partida partida,
            out int acciones)
        {
            acciones = 0;

            if (partida == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            var bitacora = new StringBuilder();

            foreach (Unidad unidad in partida.JugadorHumano.Unidades.ToList())
            {
                if (!(unidad is Aldeano aldeano) ||
                    !unidad.EstaViva ||
                    !unidad.Disponible ||
                    unidad.Coordenada == null)
                {
                    continue;
                }

                if (IntentarHuir(
                        partida,
                        aldeano,
                        out string detalle))
                {
                    acciones++;
                    bitacora.Append(detalle).Append(' ');
                }
            }

            return ResultadoAccion.Exitoso(
                acciones == 0
                    ? "Defensa humana sin acciones."
                    : "Defensa humana: " + bitacora.ToString().Trim());
        }

        /// <summary>
        /// Busca una casilla vecina que aumente la distancia minima a las
        /// amenazas militares y ejecuta un unico paso validado.
        /// </summary>
        private static bool IntentarHuir(
            Partida partida,
            Aldeano aldeano,
            out string detalle)
        {
            detalle = string.Empty;

            List<Unidad> amenazas = partida.JugadorMaquina.Unidades
                .Where(unidad =>
                    unidad is UnidadMilitar &&
                    unidad.EstaViva &&
                    unidad.Coordenada != null)
                .ToList();

            if (amenazas.Count == 0)
            {
                return false;
            }

            int distanciaActual = DistanciaMinima(
                aldeano.Coordenada,
                amenazas);

            if (distanciaActual > RadioPanicoAldeano)
            {
                return false;
            }

            Coordenada mejorCasilla = null;
            int mejorDistancia = distanciaActual;
            Unidad mejorAmenaza = AmenazaMasCercana(
                aldeano.Coordenada,
                amenazas);

            foreach (Coordenada candidata in Vecinos(aldeano.Coordenada))
            {
                if (!EsCasillaLibre(partida, aldeano, candidata))
                {
                    continue;
                }

                int distancia = DistanciaMinima(candidata, amenazas);

                if (distancia <= mejorDistancia)
                {
                    continue;
                }

                mejorCasilla = candidata;
                mejorDistancia = distancia;
            }

            if (mejorCasilla == null ||
                !aldeano.IntentarIniciarOrden(TipoAccionJuego.Mover))
            {
                return false;
            }

            ResultadoAccion paso = new OperacionPasoMovimiento().Ejecutar(
                partida,
                aldeano.Id,
                mejorCasilla);

            if (!paso.Exito)
            {
                aldeano.CancelarOrden();
                return false;
            }

            // La huida es una reaccion de un paso; el siguiente turno
            // vuelve a evaluar la amenaza sin dejar una orden colgada.
            aldeano.CompletarOrden();
            detalle =
                $"Aldeano huye de {mejorAmenaza?.GetType().Name ?? "enemigo"} " +
                $"hacia ({mejorCasilla.X},{mejorCasilla.Y}).";
            return true;
        }

        private static IEnumerable<Coordenada> Vecinos(Coordenada origen)
        {
            yield return new Coordenada(origen.X + 1, origen.Y);
            yield return new Coordenada(origen.X - 1, origen.Y);
            yield return new Coordenada(origen.X, origen.Y + 1);
            yield return new Coordenada(origen.X, origen.Y - 1);
        }

        private static int DistanciaMinima(
            Coordenada posicion,
            IEnumerable<Unidad> amenazas)
        {
            return amenazas.Min(amenaza =>
                Math.Abs(posicion.X - amenaza.Coordenada.X) +
                Math.Abs(posicion.Y - amenaza.Coordenada.Y));
        }

        private static Unidad AmenazaMasCercana(
            Coordenada posicion,
            IEnumerable<Unidad> amenazas)
        {
            return amenazas
                .OrderBy(amenaza =>
                    Math.Abs(posicion.X - amenaza.Coordenada.X) +
                    Math.Abs(posicion.Y - amenaza.Coordenada.Y))
                .FirstOrDefault();
        }

        private static bool EsCasillaLibre(
            Partida partida,
            Unidad unidad,
            Coordenada candidata)
        {
            Mapa mapa = partida.JugadorHumano.Mapa;

            if (!mapa.EstaDentroDeLimites(candidata))
            {
                return false;
            }

            Casilla casilla = mapa.ObtenerCasilla(
                candidata.X,
                candidata.Y);

            if (casilla == null ||
                !casilla.EsTransitable ||
                casilla.EstaOcupada ||
                mapa.ObtenerRecursoEn(candidata) != null)
            {
                return false;
            }

            foreach (Unidad otra in partida.JugadorHumano.Unidades
                .Concat(partida.JugadorMaquina.Unidades))
            {
                if (!ReferenceEquals(otra, unidad) &&
                    Coincide(otra.Coordenada, candidata))
                {
                    return false;
                }
            }

            foreach (Edificio edificio in partida.JugadorHumano.Edificios
                .Concat(partida.JugadorMaquina.Edificios))
            {
                if (Coincide(edificio.Coordenada, candidata))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Coincide(
            Coordenada primera,
            Coordenada segunda)
        {
            return primera != null &&
                   segunda != null &&
                   primera.X == segunda.X &&
                   primera.Y == segunda.Y;
        }
    }
}
