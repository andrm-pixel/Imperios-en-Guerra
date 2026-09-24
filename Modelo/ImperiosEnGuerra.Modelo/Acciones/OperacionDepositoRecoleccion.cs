using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Transfiere la carga transportada al saldo del jugador unicamente cuando
    /// el Aldeano se encuentra junto a un Castillo humano.
    /// </summary>
    public sealed class OperacionDepositoRecoleccion
    {
        /// <summary>
        /// Ejecuta el elemento solicitado.
        /// </summary>
        /// <param name="partida">El valor de partida.</param>
        /// <param name="aldeanoId">El valor de aldeano id.</param>
        /// <param name="castillo">El valor de centro urbano.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ResultadoDepositoRecoleccion Ejecutar(
            Partida partida,
            Guid aldeanoId,
            Coordenada castillo)
        {
            if (partida == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No hay una partida activa.");
            }

            Aldeano aldeano =
                partida.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (aldeano == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "El Aldeano humano no existe o fue destruido.");
            }

            if (castillo == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "El Castillo de depósito es obligatorio.");
            }

            Castillo centro =
                partida.JugadorHumano.Edificios
                    .OfType<Castillo>()
                    .FirstOrDefault(
                        e =>
                            e.Coordenada.X == castillo.X &&
                            e.Coordenada.Y == castillo.Y);

            if (centro == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No existe un Castillo humano en la posición indicada.");
            }

            int distancia =
                Math.Abs(
                    aldeano.Coordenada.X -
                    centro.Coordenada.X)
                +
                Math.Abs(
                    aldeano.Coordenada.Y -
                    centro.Coordenada.Y);

            if (distancia != 1)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "El Aldeano debe estar junto al Castillo para depositar.");
            }

            if (aldeano.CargaActual <= 0 ||
                !aldeano.TipoCarga.HasValue)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "El Aldeano no transporta recursos para depositar.");
            }

            int cantidad =
                aldeano.VaciarCarga(
                    out var tipo);

            if (cantidad <= 0 ||
                !tipo.HasValue)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No fue posible obtener una carga válida para depositar.");
            }

            partida.JugadorHumano.Recursos.Agregar(
                tipo.Value,
                cantidad);

            return ResultadoDepositoRecoleccion.Exitoso(
                cantidad,
                tipo.Value);
        }
    }
}
