using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Representa resultado aproximacion construccion dentro del modelo del juego.
    /// </summary>
    public sealed class ResultadoAproximacionConstruccion
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
        /// Obtiene punto interaccion.
        /// </summary>
        public Coordenada PuntoInteraccion { get; }
        /// <summary>
        /// Obtiene reintentable.
        /// </summary>
        public bool Reintentable { get; }

        /// <summary>
        /// Obtiene pasos.
        /// </summary>
        public IReadOnlyList<Coordenada> Pasos =>
            pasos.AsReadOnly();

        private ResultadoAproximacionConstruccion(
            bool exito,
            string mensaje,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos,
            bool reintentable)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            PuntoInteraccion = puntoInteraccion;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
            Reintentable = reintentable;
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="puntoInteraccion">El valor de punto interaccion.</param>
        /// <param name="pasos">El valor de pasos.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoAproximacionConstruccion Exitoso(
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos)
        {
            if (puntoInteraccion == null)
                throw new ArgumentNullException(nameof(puntoInteraccion));
            if (pasos == null)
                throw new ArgumentNullException(nameof(pasos));

            return new ResultadoAproximacionConstruccion(
                true,
                "Ruta hacia la obra preparada.",
                puntoInteraccion,
                pasos,
                false);
        }

        /// <summary>
        /// Crea un resultado fallido el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <param name="reintentable">El valor de reintentable.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoAproximacionConstruccion Fallido(
            string mensaje,
            bool reintentable = false)
        {
            return new ResultadoAproximacionConstruccion(
                false,
                mensaje,
                null,
                Array.Empty<Coordenada>(),
                reintentable);
        }
    }
}
