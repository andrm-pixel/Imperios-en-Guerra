using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Movimiento
{
    /// <summary>
    /// Resultado inmutable de preparar una orden de movimiento sin modificar todavia la posicion.
    /// </summary>
    public sealed class ResultadoPlanMovimiento
    {
        private readonly List<Coordenada> pasos;

        /// <summary>
        /// Obtiene exito.
        /// </summary>
        public bool Exito { get; }
        /// <summary>
        /// Obtiene mensaje.
        /// </summary>
        public string Mensaje { get; }

        /// <summary>
        /// Obtiene pasos.
        /// </summary>
        public IReadOnlyList<Coordenada> Pasos
        {
            get { return pasos.AsReadOnly(); }
        }

        private ResultadoPlanMovimiento(
            bool exito,
            string mensaje,
            IEnumerable<Coordenada> pasos)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="pasos">El valor de pasos.</param>
        /// <returns>Resultado de la operacion.</returns>
        public static ResultadoPlanMovimiento Exitoso(
            IEnumerable<Coordenada> pasos)
        {
            if (pasos == null)
                throw new ArgumentNullException(nameof(pasos));

            return new ResultadoPlanMovimiento(
                true,
                "Ruta de movimiento preparada.",
                pasos);
        }

        /// <summary>
        /// Crea un resultado fallido el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <returns>Resultado de la operacion.</returns>
        public static ResultadoPlanMovimiento Fallido(
            string mensaje)
        {
            return new ResultadoPlanMovimiento(
                false,
                mensaje,
                Array.Empty<Coordenada>());
        }
    }
}
