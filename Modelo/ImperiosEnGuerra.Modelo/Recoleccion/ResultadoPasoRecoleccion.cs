using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Resultado de un ciclo logico de extraccion desde un nodo hacia la carga del Aldeano.
    /// </summary>
    public sealed class ResultadoPasoRecoleccion
    {
        /// <summary>
        /// Obtiene exito.
        /// </summary>
        public bool Exito { get; }
        /// <summary>
        /// Obtiene mensaje.
        /// </summary>
        public string Mensaje { get; }
        /// <summary>
        /// Obtiene cantidad extraida.
        /// </summary>
        public int CantidadExtraida { get; }
        /// <summary>
        /// Obtiene carga actual.
        /// </summary>
        public int CargaActual { get; }
        /// <summary>
        /// Obtiene capacidad carga.
        /// </summary>
        public int CapacidadCarga { get; }
        /// <summary>
        /// Obtiene capacidad completa.
        /// </summary>
        public bool CapacidadCompleta { get; }
        /// <summary>
        /// Obtiene recurso agotado.
        /// </summary>
        public bool RecursoAgotado { get; }
        /// <summary>
        /// Obtiene tipo recurso.
        /// </summary>
        public TipoRecurso? TipoRecurso { get; }

        private ResultadoPasoRecoleccion(
            bool exito,
            string mensaje,
            int cantidadExtraida,
            int cargaActual,
            int capacidadCarga,
            bool capacidadCompleta,
            bool recursoAgotado,
            TipoRecurso? tipoRecurso)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            CantidadExtraida = cantidadExtraida;
            CargaActual = cargaActual;
            CapacidadCarga = capacidadCarga;
            CapacidadCompleta = capacidadCompleta;
            RecursoAgotado = recursoAgotado;
            TipoRecurso = tipoRecurso;
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="cantidadExtraida">El valor de cantidad extraida.</param>
        /// <param name="cargaActual">El valor de carga actual.</param>
        /// <param name="capacidadCarga">El valor de capacidad carga.</param>
        /// <param name="recursoAgotado">El valor de recurso agotado.</param>
        /// <param name="tipoRecurso">El valor de tipo recurso.</param>
        /// <returns>Resultado de la operacion.</returns>
        public static ResultadoPasoRecoleccion Exitoso(
            int cantidadExtraida,
            int cargaActual,
            int capacidadCarga,
            bool recursoAgotado,
            TipoRecurso tipoRecurso)
        {
            return new ResultadoPasoRecoleccion(
                true,
                $"Se recolectaron {cantidadExtraida} de {tipoRecurso}.",
                cantidadExtraida,
                cargaActual,
                capacidadCarga,
                cargaActual >= capacidadCarga,
                recursoAgotado,
                tipoRecurso);
        }

        /// <summary>
        /// Crea un resultado fallido el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <returns>Resultado de la operacion.</returns>
        public static ResultadoPasoRecoleccion Fallido(
            string mensaje)
        {
            return new ResultadoPasoRecoleccion(
                false,
                mensaje,
                0,
                0,
                0,
                false,
                false,
                null);
        }
    }
}
