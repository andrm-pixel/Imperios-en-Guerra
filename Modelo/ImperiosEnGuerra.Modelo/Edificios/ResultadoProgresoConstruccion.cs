namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Representa resultado progreso construccion dentro del modelo del juego.
    /// </summary>
    public sealed class ResultadoProgresoConstruccion
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
        /// Obtiene progreso.
        /// </summary>
        public int Progreso { get; }
        /// <summary>
        /// Obtiene terminada.
        /// </summary>
        public bool Terminada { get; }

        private ResultadoProgresoConstruccion(
            bool exito,
            string mensaje,
            int progreso,
            bool terminada)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            Progreso = progreso;
            Terminada = terminada;
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="progreso">El valor de progreso.</param>
        /// <param name="terminada">El valor de terminada.</param>
        /// <returns>Resultado de la operacion.</returns>
        public static ResultadoProgresoConstruccion Exitoso(
            int progreso,
            bool terminada)
        {
            return new ResultadoProgresoConstruccion(
                true,
                terminada
                    ? "Construcción terminada."
                    : $"Construcción al {progreso}%.",
                progreso,
                terminada);
        }

        /// <summary>
        /// Crea un resultado fallido el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <returns>Resultado de la operacion.</returns>
        public static ResultadoProgresoConstruccion Fallido(
            string mensaje)
        {
            return new ResultadoProgresoConstruccion(
                false,
                mensaje,
                0,
                false);
        }
    }
}
