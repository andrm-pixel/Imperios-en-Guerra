namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Representa resultado progreso entrenamiento dentro del modelo del juego.
    /// </summary>
    public sealed class ResultadoProgresoEntrenamiento
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
        /// Obtiene terminado.
        /// </summary>
        public bool Terminado { get; }

        private ResultadoProgresoEntrenamiento(
            bool exito,
            string mensaje,
            int progreso,
            bool terminado)
        {
            Exito = exito;
            Mensaje = mensaje ?? string.Empty;
            Progreso = progreso;
            Terminado = terminado;
        }

        /// <summary>
        /// Crea un resultado exitoso el elemento solicitado.
        /// </summary>
        /// <param name="progreso">El valor de progreso.</param>
        /// <returns>Resultado de la operacion.</returns>
        public static ResultadoProgresoEntrenamiento Exitoso(
            int progreso)
        {
            return new ResultadoProgresoEntrenamiento(
                true,
                progreso >= 100
                    ? "Entrenamiento listo para aparición."
                    : $"Entrenamiento al {progreso}%.",
                progreso,
                progreso >= 100);
        }

        /// <summary>
        /// Crea un resultado fallido el elemento solicitado.
        /// </summary>
        /// <param name="mensaje">El valor de mensaje.</param>
        /// <returns>Resultado de la operacion.</returns>
        public static ResultadoProgresoEntrenamiento Fallido(
            string mensaje)
        {
            return new ResultadoProgresoEntrenamiento(
                false,
                mensaje,
                0,
                false);
        }
    }
}
