using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Mantiene el movimiento inmediato legado, pero usa el mismo planificador de rutas del movimiento progresivo.
    /// </summary>
    public sealed class OperacionMovimiento
    {
        /// <summary>
        /// Ejecuta el elemento solicitado.
        /// </summary>
        /// <param name="partida">El valor de partida.</param>
        /// <param name="solicitud">El valor de solicitud.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudMovimiento solicitud)
        {
            ResultadoPlanMovimiento plan =
                new RutaMovimiento()
                    .Preparar(
                        partida,
                        solicitud);

            if (!plan.Exito)
            {
                return ResultadoAccion.Fallido(
                    plan.Mensaje);
            }

            Unidad unidad =
                partida.JugadorHumano.Unidades
                    .First(
                        u =>
                            u.Id ==
                            solicitud.UnidadId);

            if (plan.Pasos.Count > 0)
            {
                unidad.EstablecerDestino(
                    solicitud.Destino);
            }

            return ResultadoAccion.Exitoso(
                "Movimiento realizado.");
        }
    }
}
