namespace ImperiosEnGuerra.Modelo.Reglas
{
    /// <summary>
    /// Reglas de selección y objetivos. Vive en el Modelo.
    /// El Controlador Unity solo actúa como puente y debe consultar aquí
    /// (vía API interna) en lugar de duplicar strings de dominio.
    /// </summary>
    public static class ReglasAcciones
    {
        /// <summary>
        /// Representa el campo tipo centro urbano.
        /// </summary>
        public const string TipoCastillo = "Castillo";
        /// <summary>
        /// Representa el campo tipo aldeano.
        /// </summary>
        public const string TipoAldeano = "Aldeano";

        /// <summary>
        /// Ejecuta la operación es unidad militar.
        /// </summary>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public static bool EsUnidadMilitar(string? tipoUnidad)
        {
            return tipoUnidad == "Soldado"
                
                || tipoUnidad == "Arquero";
        }

        /// <summary>
        /// Las órdenes son interrumpibles: una unidad ocupada puede recibir
        /// una nueva orden (el controlador cancela la anterior primero).
        /// Por eso la orden activa no bloquea las opciones.
        /// </summary>
        public static bool PermiteMover(string? propietario, string? categoria, string? ordenActiva)
        {
            if (propietario != "Humano")
                return false;
            if (categoria != "Unidad")
                return false;
            return true;
        }

        /// <summary>
        /// Ejecuta la operación permite recolectar.
        /// </summary>
        /// <param name="propietario">El valor de propietario.</param>
        /// <param name="categoria">El valor de categoria.</param>
        /// <param name="tipoLogico">El valor de tipo logico.</param>
        /// <param name="ordenActiva">El valor de orden activa.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public static bool PermiteRecolectar(string? propietario, string? categoria, string? tipoLogico, string? ordenActiva)
        {
            if (!PermiteMover(propietario, categoria, ordenActiva))
                return false;
            return tipoLogico == TipoAldeano;
        }

        /// <summary>
        /// Ejecuta la operación permite construir.
        /// </summary>
        /// <param name="propietario">El valor de propietario.</param>
        /// <param name="categoria">El valor de categoria.</param>
        /// <param name="tipoLogico">El valor de tipo logico.</param>
        /// <param name="ordenActiva">El valor de orden activa.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public static bool PermiteConstruir(string? propietario, string? categoria, string? tipoLogico, string? ordenActiva)
        {
            return PermiteRecolectar(propietario, categoria, tipoLogico, ordenActiva);
        }

        /// <summary>
        /// Ejecuta la operación permite entrenar.
        /// </summary>
        /// <param name="propietario">El valor de propietario.</param>
        /// <param name="categoria">El valor de categoria.</param>
        /// <param name="tipoLogico">El valor de tipo logico.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public static bool PermiteEntrenar(string? propietario, string? categoria, string? tipoLogico)
        {
            if (propietario != "Humano")
                return false;
            return categoria == "Edificio" && tipoLogico == TipoCastillo;
        }

        /// <summary>
        /// Ejecuta la operación permite atacar.
        /// </summary>
        /// <param name="propietario">El valor de propietario.</param>
        /// <param name="categoria">El valor de categoria.</param>
        /// <param name="tipoLogico">El valor de tipo logico.</param>
        /// <param name="ordenActiva">El valor de orden activa.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public static bool PermiteAtacar(string? propietario, string? categoria, string? tipoLogico, string? ordenActiva)
        {
            if (!PermiteMover(propietario, categoria, ordenActiva))
                return false;
            return EsUnidadMilitar(tipoLogico);
        }

        /// <summary>
        /// Ejecuta la operación es objetivo ataque valido.
        /// </summary>
        /// <param name="categoria">El valor de categoria.</param>
        /// <param name="propietario">El valor de propietario.</param>
        /// <param name="idLogico">El valor de id logico.</param>
        /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
        public static bool EsObjetivoAtaqueValido(string? categoria, string? propietario, string? idLogico)
        {
            if (string.IsNullOrWhiteSpace(idLogico) ||
                propietario != "Maquina")
            {
                return false;
            }

            return categoria == "Unidad" || categoria == "Edificio";
        }
    }
}
