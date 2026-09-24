using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Resultado de calcular una ruta hasta una casilla de interacción
    /// adyacente a un nodo de recurso.
    /// </summary>
    public sealed class ResultadoAproximacionRecurso
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
        /// Obtiene tipo recurso.
        /// </summary>
        public TipoRecurso? TipoRecurso { get; }
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
        public IReadOnlyList<Coordenada> Pasos
        {
            get { return pasos.AsReadOnly(); }
        }

        private ResultadoAproximacionRecurso(
            bool exito,
            string mensaje,
            TipoRecurso? tipoRecurso,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos,
            bool reintentable)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            TipoRecurso = tipoRecurso;
            PuntoInteraccion = puntoInteraccion;
            Reintentable = reintentable;
            this.pasos = pasos == null
                ? new List<Coordenada>()
                : new List<Coordenada>(pasos);
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="tipoRecurso">El valor de tipo recurso.</param>
        /// <param name="puntoInteraccion">El valor de punto interaccion.</param>
        /// <param name="pasos">El valor de pasos.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoAproximacionRecurso Exitoso(
            TipoRecurso tipoRecurso,
            Coordenada puntoInteraccion,
            IEnumerable<Coordenada> pasos)
        {
            if (puntoInteraccion == null)
            {
                throw new ArgumentNullException(
                    nameof(puntoInteraccion));
            }

            if (pasos == null)
            {
                throw new ArgumentNullException(
                    nameof(pasos));
            }

            return new ResultadoAproximacionRecurso(
                true,
                $"Ruta hacia recurso {tipoRecurso} preparada.",
                tipoRecurso,
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
        public static ResultadoAproximacionRecurso Fallido(
            string mensaje,
            bool reintentable = false)
        {
            return new ResultadoAproximacionRecurso(
                false,
                mensaje,
                null,
                null,
                Array.Empty<Coordenada>(),
                reintentable);
        }
    }
}
