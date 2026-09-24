using System;
using ImperiosEnGuerra.Modelo.Core;

namespace ImperiosEnGuerra.Modelo.Movimiento
{
    /// <summary>
    /// Distribuye de forma determinista las casillas de interacción entre
    /// unidades humanas para reducir colisiones y livelocks alrededor de
    /// recursos, depósitos y obras.
    /// </summary>
    internal static class PreferenciaCasillaInteraccion
    {
        /// <summary>
        /// Obtiene indice inicial.
        /// </summary>
        /// <param name="partida">El valor de partida.</param>
        /// <param name="unidadId">El valor de unidad id.</param>
        /// <param name="cantidadOpciones">El valor de cantidad opciones.</param>
        /// <returns>Resultado de la operación.</returns>
        public static int ObtenerIndiceInicial(
            Partida partida,
            Guid unidadId,
            int cantidadOpciones)
        {
            if (partida == null)
                throw new ArgumentNullException(nameof(partida));

            if (cantidadOpciones <= 0)
                throw new ArgumentOutOfRangeException(nameof(cantidadOpciones));

            int indice = 0;

            foreach (var unidad in partida.JugadorHumano.Unidades)
            {
                if (unidad.Id == unidadId)
                    return indice % cantidadOpciones;

                indice++;
            }

            byte[] bytes = unidadId.ToByteArray();

            return
                (bytes[0] ^
                 bytes[5] ^
                 bytes[10] ^
                 bytes[15]) %
                cantidadOpciones;
        }
    }
}
