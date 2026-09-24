using System;
using System.Collections.Generic;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Configuracion logica de cantidad recolectada por ciclo segun el tipo de recurso.
    /// </summary>
    public sealed class ConfiguracionRecoleccion
    {
        private readonly Dictionary<TipoRecurso, int> tasas;

        /// <summary>
        /// Inicializa una nueva instancia de ConfiguracionRecoleccion.
        /// </summary>
        /// <param name="tasaOro">El valor de tasa oro.</param>
        /// <param name="tasaMadera">El valor de tasa madera.</param>
        /// <param name="tasaComida">El valor de tasa comida.</param>
        /// <param name="tasaPiedra">El valor de tasa piedra.</param>
        /// <param name="tasaHierro">El valor de tasa hierro.</param>
        public ConfiguracionRecoleccion(
            int tasaOro = 5,
            int tasaMadera = 5,
            int tasaComida = 5,
            int tasaPiedra = 5,
            int tasaHierro = 5)
        {
            ValidarTasa(
                tasaOro,
                nameof(tasaOro));

            ValidarTasa(
                tasaMadera,
                nameof(tasaMadera));

            ValidarTasa(
                tasaComida,
                nameof(tasaComida));

            ValidarTasa(
                tasaPiedra,
                nameof(tasaPiedra));

            ValidarTasa(
                tasaHierro,
                nameof(tasaHierro));

            tasas =
                new Dictionary<TipoRecurso, int>
                {
                    { TipoRecurso.Oro, tasaOro },
                    { TipoRecurso.Madera, tasaMadera },
                    { TipoRecurso.Comida, tasaComida },
                    { TipoRecurso.Piedra, tasaPiedra },
                    { TipoRecurso.Hierro, tasaHierro }
                };
        }

        /// <summary>
        /// Obtiene tasa.
        /// </summary>
        /// <param name="tipo">El valor de tipo.</param>
        /// <returns>Resultado de la operacion.</returns>
        public int ObtenerTasa(
            TipoRecurso tipo)
        {
            if (!tasas.TryGetValue(
                    tipo,
                    out int tasa))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tipo),
                    "El tipo de recurso no tiene una tasa de recolección configurada.");
            }

            return tasa;
        }

        private static void ValidarTasa(
            int tasa,
            string nombreParametro)
        {
            if (tasa <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nombreParametro,
                    "La tasa de recolección debe ser positiva.");
            }
        }
    }
}
