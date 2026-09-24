using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Representa operacion entrenamiento dentro del modelo del juego.
    /// </summary>
    public sealed class OperacionEntrenamiento
    {
        /// <summary>
        /// Ejecuta el elemento solicitado.
        /// </summary>
        /// <param name="partida">El valor de partida.</param>
        /// <param name="solicitud">El valor de solicitud.</param>
        /// <returns>Resultado de la operación.</returns>
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudEntrenamiento solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            if (solicitud == null)
                return ResultadoAccion.Fallido(
                    "La solicitud de entrenamiento es obligatoria.");

            if (solicitud.EdificioOrigen == null)
                return ResultadoAccion.Fallido(
                    "El edificio de origen es obligatorio.");

            if (solicitud.Destino == null)
                return ResultadoAccion.Fallido(
                    "La posición de aparición es obligatoria.");

            bool edificioMaquina =
                partida.JugadorMaquina.Edificios.Any(
                    e => MismaCoordenada(
                        e.Coordenada,
                        solicitud.EdificioOrigen));

            if (edificioMaquina)
                return ResultadoAccion.Fallido(
                    "No se puede entrenar desde un edificio de la máquina.");

            Edificio edificio =
                partida.JugadorHumano.Edificios.FirstOrDefault(
                    e => MismaCoordenada(
                        e.Coordenada,
                        solicitud.EdificioOrigen));

            if (edificio == null)
                return ResultadoAccion.Fallido(
                    "No existe un edificio humano en la posición indicada.");

            if (!(edificio is Castillo))
                return ResultadoAccion.Fallido(
                    "El edificio seleccionado no permite entrenamiento.");

            Mapa mapa = partida.JugadorHumano.Mapa;

            if (!mapa.EstaDentroDeLimites(solicitud.Destino))
                return ResultadoAccion.Fallido(
                    "La posición de aparición está fuera del mapa.");

            if (!mapa.PuedeColocar(solicitud.Destino))
                return ResultadoAccion.Fallido(
                    "La posición de aparición no está disponible.");

            Unidad unidad =
                CrearUnidad(
                    solicitud.TipoUnidad,
                    solicitud.Destino);

            if (unidad == null)
                return ResultadoAccion.Fallido(
                    "El tipo de unidad indicado no está permitido.");

            Casilla casilla = mapa.ObtenerCasilla(
                solicitud.Destino.X,
                solicitud.Destino.Y);

            if (casilla == null || !casilla.Ocupar())
                return ResultadoAccion.Fallido(
                    "No se pudo ocupar la posición de aparición.");

            partida.JugadorHumano.AgregarUnidad(unidad);

            return ResultadoAccion.Exitoso(
                $"Unidad {unidad.GetType().Name} entrenada correctamente.");
        }

        private static Unidad CrearUnidad(
            string tipoUnidad,
            Coordenada coordenada)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad))
                return null;

            if (string.Equals(
                tipoUnidad,
                nameof(Aldeano),
                StringComparison.OrdinalIgnoreCase))
            {
                return new Aldeano(coordenada);
            }

            if (string.Equals(
                tipoUnidad,
                nameof(Soldado),
                StringComparison.OrdinalIgnoreCase))
            {
                return new Soldado(coordenada);
            }

            if (string.Equals(
                tipoUnidad,
                nameof(Arquero),
                StringComparison.OrdinalIgnoreCase))
            {
                return new Arquero(coordenada);
            }

            return null;
        }

        private static bool MismaCoordenada(
            Coordenada a,
            Coordenada b)
        {
            return a != null &&
                   b != null &&
                   a.X == b.X &&
                   a.Y == b.Y;
        }
    }
}
