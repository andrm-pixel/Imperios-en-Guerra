using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Multiplicadores propios del prototipo para tiempos de entrenamiento.
    /// </summary>
    public sealed class ConfiguracionEntrenamiento
    {
        private readonly Dictionary<string, double> factores;

        /// <summary>
        /// Inicializa una nueva instancia de ConfiguracionEntrenamiento.
        /// </summary>
        public ConfiguracionEntrenamiento()
        {
            factores =
                new Dictionary<string, double>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    { nameof(Aldeano), 1.00d },
                    { nameof(Soldado), 1.20d },
                    
                    { nameof(Arquero), 1.30d }
                };
        }

        /// <summary>
        /// Intenta obtener factor.
        /// </summary>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <param name="factor">El valor de factor.</param>
        /// <returns>true si la operacion tuvo exito; false en caso contrario.</returns>
        public bool IntentarObtenerFactor(
            string tipoUnidad,
            out double factor)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad))
            {
                factor = 0d;
                return false;
            }

            return factores.TryGetValue(
                tipoUnidad,
                out factor);
        }
    }
}
