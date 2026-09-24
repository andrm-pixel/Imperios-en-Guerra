namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Resultado de una acción, independiente del transporte y de Unity.</summary>
    public class ResultadoAccion
    {
        /// <summary>
        /// Obtiene exito.
        /// </summary>
        public bool Exito { get; }
        /// <summary>
        /// Obtiene mensaje.
        /// </summary>
        public string Mensaje { get; }

        private ResultadoAccion(bool exito, string mensaje)
        {
            Exito = exito;
            Mensaje = mensaje;
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoAccion Exitoso(string mensaje)
        {
            return new ResultadoAccion(true, mensaje);
        }

        /// <summary>
        /// Crea un resultado fallido el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <returns>Resultado de la operación.</returns>
        public static ResultadoAccion Fallido(string mensaje)
        {
            return new ResultadoAccion(false, mensaje);
        }
    }
}
