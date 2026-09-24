using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Balance economico propio del prototipo academico.
    /// Comida sostiene crecimiento y ejercito; Madera se reserva a construccion;
    /// Oro financia tropas y expansion; Piedra refuerza construccion;
    /// Hierro arma a las tropas avanzadas.
    /// </summary>
    public sealed class ConfiguracionEconomia
    {
        private readonly Dictionary<string, CostoRecursos> costosEdificios;
        private readonly Dictionary<string, CostoRecursos> costosUnidades;

        /// <summary>
        /// Inicializa una nueva instancia de ConfiguracionEconomia.
        /// </summary>
        public ConfiguracionEconomia()
        {
            costosEdificios =
                new Dictionary<string, CostoRecursos>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    {
                        nameof(Castillo),
                        new CostoRecursos(
                            oro: 20,
                            madera: 50,
                            comida: 0,
                            piedra: 20)
                    }
                };

            costosUnidades =
                new Dictionary<string, CostoRecursos>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    { nameof(Aldeano), new CostoRecursos(0, 0, 10) },
                    { nameof(Soldado), new CostoRecursos(5, 0, 15, 0, 5) },
                    
                    { nameof(Arquero), new CostoRecursos(10, 0, 10, 0, 8) }
                };
        }

        /// <summary>
        /// Intenta obtener costo edificio.
        /// </summary>
        /// <param name="tipoEdificio">El valor de tipo edificio.</param>
        /// <param name="costo">El valor de costo.</param>
        /// <returns>true si la operacion tuvo exito; false en caso contrario.</returns>
        public bool IntentarObtenerCostoEdificio(
            string tipoEdificio,
            out CostoRecursos costo)
        {
            if (string.IsNullOrWhiteSpace(tipoEdificio))
            {
                costo = null;
                return false;
            }

            return costosEdificios.TryGetValue(
                tipoEdificio,
                out costo);
        }

        /// <summary>
        /// Intenta obtener costo unidad.
        /// </summary>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <param name="costo">El valor de costo.</param>
        /// <returns>true si la operacion tuvo exito; false en caso contrario.</returns>
        public bool IntentarObtenerCostoUnidad(
            string tipoUnidad,
            out CostoRecursos costo)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad))
            {
                costo = null;
                return false;
            }

            return costosUnidades.TryGetValue(
                tipoUnidad,
                out costo);
        }
    }
}
