using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Representa resultado deposito recoleccion dentro del modelo del juego.
    /// </summary>
    public sealed class ResultadoDepositoRecoleccion
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
        /// Obtiene cantidad depositada.
        /// </summary>
        public int CantidadDepositada { get; }
        /// <summary>
        /// Obtiene tipo recurso.
        /// </summary>
        public TipoRecurso? TipoRecurso { get; }

        private ResultadoDepositoRecoleccion(
            bool exito,
            string mensaje,
            int cantidadDepositada,
            TipoRecurso? tipoRecurso)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            CantidadDepositada = cantidadDepositada;
            TipoRecurso = tipoRecurso;
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="cantidad">El valor de cantidad.</param>
        /// <param name="tipo">El valor de tipo.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoDepositoRecoleccion Exitoso(
            int cantidad,
            TipoRecurso tipo)
        {
            return new ResultadoDepositoRecoleccion(
                true,
                $"Se depositaron {cantidad} de {tipo}.",
                cantidad,
                tipo);
        }

        /// <summary>
        /// Crea un resultado fallido el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoDepositoRecoleccion Fallido(
            string mensaje)
        {
            return new ResultadoDepositoRecoleccion(
                false,
                mensaje,
                0,
                null);
        }
    }
}
