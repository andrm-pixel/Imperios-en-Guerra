using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Representa resultado aproximacion deposito dentro del modelo del juego.
    /// </summary>
    public sealed class ResultadoAproximacionDeposito
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
        /// Obtiene centro urbano.
        /// </summary>
        public Coordenada CentroUrbano { get; }
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

        private ResultadoAproximacionDeposito(
            bool exito,
            string mensaje,
            Coordenada centroUrbano,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos,
            bool reintentable)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            CentroUrbano = centroUrbano;
            PuntoInteraccion = puntoInteraccion;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
            Reintentable = reintentable;
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="centroUrbano">El valor de centro urbano.</param>
        /// <param name="puntoInteraccion">El valor de punto interaccion.</param>
        /// <param name="pasos">El valor de pasos.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoAproximacionDeposito Exitoso(
            Coordenada centroUrbano,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos)
        {
            if (centroUrbano == null)
                throw new ArgumentNullException(nameof(centroUrbano));

            if (puntoInteraccion == null)
                throw new ArgumentNullException(nameof(puntoInteraccion));

            if (pasos == null)
                throw new ArgumentNullException(nameof(pasos));

            return new ResultadoAproximacionDeposito(
                true,
                "Ruta hacia el punto de depósito preparada.",
                centroUrbano,
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
        public static ResultadoAproximacionDeposito Fallido(
            string mensaje,
            bool reintentable = false)
        {
            return new ResultadoAproximacionDeposito(
                false,
                mensaje,
                null,
                null,
                Array.Empty<Coordenada>(),
                reintentable);
        }
    }
}
