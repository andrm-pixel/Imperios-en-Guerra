namespace ImperiosEnGuerra.Modelo.Reglas
{
    /// <summary>
    /// Reglas de selección y objetivos. Vive en el Modelo.
    /// El Controlador Unity solo actúa como puente y debe consultar aquí
    /// (vía API interna) en lugar de duplicar strings de dominio.
    /// </summary>
    public static class ReglasAcciones
    {
        public const string TipoCentroUrbano = "CentroUrbano";
        public const string TipoAldeano = "Aldeano";

        public static bool EsUnidadMilitar(string? tipoUnidad)
        {
            return tipoUnidad == "Guerrero"
                || tipoUnidad == "Lancero"
                || tipoUnidad == "Arquero"
                || tipoUnidad == "Monje";
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

        public static bool PermiteRecolectar(string? propietario, string? categoria, string? tipoLogico, string? ordenActiva)
        {
            if (!PermiteMover(propietario, categoria, ordenActiva))
                return false;
            return tipoLogico == TipoAldeano;
        }

        public static bool PermiteConstruir(string? propietario, string? categoria, string? tipoLogico, string? ordenActiva)
        {
            return PermiteRecolectar(propietario, categoria, tipoLogico, ordenActiva);
        }

        public static bool PermiteEntrenar(string? propietario, string? categoria, string? tipoLogico)
        {
            if (propietario != "Humano")
                return false;
            return categoria == "Edificio" && tipoLogico == TipoCentroUrbano;
        }

        public static bool PermiteAtacar(string? propietario, string? categoria, string? tipoLogico, string? ordenActiva)
        {
            if (!PermiteMover(propietario, categoria, ordenActiva))
                return false;
            return EsUnidadMilitar(tipoLogico);
        }

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
