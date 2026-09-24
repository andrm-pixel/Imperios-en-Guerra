namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Estado logico autoritativo de una unidad durante una orden de gameplay.
    /// </summary>
    public enum EstadoUnidad
    {
        /// <summary>
        /// Representa el valor idle.
        /// </summary>
        Idle,
        /// <summary>
        /// Representa el valor moviendo.
        /// </summary>
        Moviendo,
        /// <summary>
        /// Representa el valor recolectando.
        /// </summary>
        Recolectando,
        /// <summary>
        /// Representa el valor construyendo.
        /// </summary>
        Construyendo,
        /// <summary>
        /// Representa el valor atacando.
        /// </summary>
        Atacando
    }
}
