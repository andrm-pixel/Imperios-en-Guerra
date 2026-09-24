using System;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Representa fabrica unidades dentro del modelo del juego.
    /// </summary>
    public static class FabricaUnidades
    {
        /// <summary>
        /// Crea el elemento solicitado.
        /// </summary>
        /// <param name="tipoUnidad">El valor de tipo unidad.</param>
        /// <param name="coordenada">El valor de coordenada.</param>
        /// <returns>Resultado de la operación.</returns>
        public static Unidad Crear(
            string tipoUnidad,
            Coordenada coordenada)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad) ||
                coordenada == null)
            {
                return null;
            }

            if (string.Equals(tipoUnidad, nameof(Aldeano), StringComparison.OrdinalIgnoreCase))
                return new Aldeano(coordenada);
            if (string.Equals(tipoUnidad, nameof(Soldado), StringComparison.OrdinalIgnoreCase))
                return new Soldado(coordenada);
            if (string.Equals(tipoUnidad, nameof(Arquero), StringComparison.OrdinalIgnoreCase))
                return new Arquero(coordenada);

            return null;
        }
    }
}
