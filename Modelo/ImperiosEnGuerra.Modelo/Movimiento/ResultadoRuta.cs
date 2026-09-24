using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Movimiento
{
    /// <summary>
    /// Representa resultado ruta dentro del modelo del juego.
    /// </summary>
    public sealed class ResultadoRuta
    {
        private readonly List<Coordenada> pasos;

        /// <summary>
        /// Obtiene encontrada.
        /// </summary>
        public bool Encontrada { get; }

        /// <summary>
        /// Obtiene pasos.
        /// </summary>
        public IReadOnlyList<Coordenada> Pasos
        {
            get { return pasos.AsReadOnly(); }
        }

        private ResultadoRuta(
            bool encontrada,
            IEnumerable<Coordenada> pasos)
        {
            Encontrada = encontrada;

            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
        }

        /// <summary>
        /// Ejecuta la operación exitosa.
        /// </summary>
        /// <param name="pasos">El valor de pasos.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoRuta Exitosa(
            IEnumerable<Coordenada> pasos)
        {
            if (pasos == null)
            {
                throw new ArgumentNullException(nameof(pasos));
            }

            return new ResultadoRuta(
                true,
                pasos);
        }

        /// <summary>
        /// Ejecuta la operación imposible.
        /// </summary>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoRuta Imposible()
        {
            return new ResultadoRuta(
                false,
                Array.Empty<Coordenada>());
        }
    }
}
