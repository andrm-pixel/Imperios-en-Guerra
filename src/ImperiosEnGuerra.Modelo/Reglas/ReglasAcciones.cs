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

        public static bool PermiteMover(string? propietario, string? categoria, string? ordenActiva)
        {
            if (propietario != "Humano")
                return false;
            if (categoria != "Unidad")
                return false;
            return string.IsNullOrWhiteSpace(ordenActiva);
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
            return categoria == "Unidad"
                && propietario == "Maquina"
                && !string.IsNullOrWhiteSpace(idLogico);
        }
    }
}
