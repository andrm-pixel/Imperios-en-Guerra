using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Movimiento
{
    /// <summary>
    /// Valida la solicitud y calcula la ruta sin mover la unidad.
    /// </summary>
    public sealed class RutaMovimiento
    {
        private readonly RutaAStar buscador;

        /// <summary>
        /// Crea ruta con buscador base.
        /// </summary>
        public RutaMovimiento()
            : this(new RutaAStar())
        {
        }

        /// <summary>
        /// Crea ruta con buscador dado.
        /// </summary>
        /// <param name="buscador">Buscador de ruta.</param>
        public RutaMovimiento(
            RutaAStar buscador)
        {
            this.buscador =
                buscador
                ?? throw new System.ArgumentNullException(
                    nameof(buscador));
        }

        /// <summary>
        /// Prepara la ruta.
        /// </summary>
        /// <param name="partida">Partida actual.</param>
        /// <param name="solicitud">Solicitud de mover.</param>
        /// <param name="permitirOrdenMovimientoActiva">True si admite orden mover activa.</param>
        /// <returns>Resultado.</returns>
        public ResultadoPlanMovimiento Preparar(
            Partida partida,
            SolicitudMovimiento solicitud,
            bool permitirOrdenMovimientoActiva = false)
        {
            if (partida == null)
                return ResultadoPlanMovimiento.Fallido(
                    "No hay una partida activa.");

            if (solicitud == null)
                return ResultadoPlanMovimiento.Fallido(
                    "La solicitud de movimiento es obligatoria.");

            if (partida.JugadorMaquina.Unidades.Any(
                u => u.Id == solicitud.UnidadId))
            {
                return ResultadoPlanMovimiento.Fallido(
                    "No se puede mover una unidad de la máquina.");
            }

            Unidad unidad =
                partida.JugadorHumano.Unidades
                    .FirstOrDefault(
                        u => u.Id == solicitud.UnidadId);

            if (unidad == null)
                return ResultadoPlanMovimiento.Fallido(
                    "La unidad humana no existe o fue destruida.");

            if (!unidad.Disponible &&
                !(permitirOrdenMovimientoActiva &&
                  unidad.OrdenActiva == TipoAccionJuego.Mover))
            {
                return ResultadoPlanMovimiento.Fallido(
                    "La unidad no está disponible.");
            }

            Coordenada destino = solicitud.Destino;
            Mapa mapa = partida.JugadorHumano.Mapa;

            if (destino == null)
                return ResultadoPlanMovimiento.Fallido(
                    "El destino es obligatorio.");

            if (!mapa.EstaDentroDeLimites(destino))
                return ResultadoPlanMovimiento.Fallido(
                    "El destino está fuera del mapa.");

            Casilla casilla =
                mapa.ObtenerCasilla(
                    destino.X,
                    destino.Y);

            if (!casilla.EsTransitable)
                return ResultadoPlanMovimiento.Fallido(
                    "La casilla destino no es transitable.");

            if (casilla.EstaOcupada)
                return ResultadoPlanMovimiento.Fallido(
                    "La casilla destino está ocupada.");

            if (mapa.ObtenerRecursoEn(destino) != null)
                return ResultadoPlanMovimiento.Fallido(
                    "La casilla destino contiene un recurso físico.");

            if (TieneEntidadEn(
                    partida.JugadorHumano,
                    unidad,
                    destino) ||
                (ReferenceEquals(
                     mapa,
                     partida.JugadorMaquina.Mapa) &&
                 TieneEntidadEn(
                     partida.JugadorMaquina,
                     unidad,
                     destino)))
            {
                return ResultadoPlanMovimiento.Fallido(
                    "La posición destino contiene una unidad o un edificio.");
            }

            List<Coordenada> bloqueos =
                ObtenerBloqueos(
                    partida,
                    mapa,
                    unidad);

            ResultadoRuta ruta =
                buscador.Buscar(
                    mapa,
                    unidad.Coordenada,
                    destino,
                    bloqueos);

            if (!ruta.Encontrada)
            {
                return ResultadoPlanMovimiento.Fallido(
                    "No existe una ruta transitable hasta el destino.");
            }

            return ResultadoPlanMovimiento.Exitoso(
                ruta.Pasos);
        }

        private static List<Coordenada> ObtenerBloqueos(
            Partida partida,
            Mapa mapa,
            Unidad unidadMovil)
        {
            var bloqueos =
                new List<Coordenada>();

            AgregarBloqueos(
                partida.JugadorHumano,
                mapa,
                unidadMovil,
                bloqueos);

            AgregarBloqueos(
                partida.JugadorMaquina,
                mapa,
                unidadMovil,
                bloqueos);

            return bloqueos;
        }

        private static void AgregarBloqueos(
            Jugador jugador,
            Mapa mapa,
            Unidad unidadMovil,
            List<Coordenada> bloqueos)
        {
            if (!ReferenceEquals(
                jugador.Mapa,
                mapa))
            {
                return;
            }

            foreach (Unidad unidad in jugador.Unidades)
            {
                if (!ReferenceEquals(
                    unidad,
                    unidadMovil))
                {
                    bloqueos.Add(
                        unidad.Coordenada);
                }
            }

            foreach (var edificio in jugador.Edificios)
            {
                bloqueos.Add(
                    edificio.Coordenada);
            }
        }

        private static bool TieneEntidadEn(
            Jugador jugador,
            Unidad unidadMovil,
            Coordenada destino)
        {
            return jugador.Unidades.Any(
                       u =>
                           !ReferenceEquals(
                               u,
                               unidadMovil) &&
                           Coincide(
                               u.Coordenada,
                               destino))
                ||
                   jugador.Edificios.Any(
                       e => Coincide(
                           e.Coordenada,
                           destino));
        }

        private static bool Coincide(
            Coordenada posicion,
            Coordenada destino)
        {
            return posicion != null &&
                   posicion.X == destino.X &&
                   posicion.Y == destino.Y;
        }
    }
}
