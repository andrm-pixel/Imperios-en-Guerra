using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Base abstracta de las unidades militares (Soldado y Arquero).
    /// La usan <see cref="Acciones.OperacionAtaque"/>,
    /// <see cref="Servicios.NucleoBatalla"/> e
    /// <see cref="IA.InteligenciaMaquina"/> para distinguir combatientes.
    /// </summary>
    public abstract class UnidadMilitar : Unidad
    {
        /// <summary>
        /// Inicializa la base militar delegando posición y estadísticas.
        /// </summary>
        protected UnidadMilitar(
            Coordenada coordenada,
            double velocidadMovimiento = 1d)
            : base(coordenada, velocidadMovimiento)
        {
        }

        /// <summary>
        /// Inicializa la base militar con estadísticas de combate.
        /// </summary>
        protected UnidadMilitar(
            Coordenada coordenada,
            double velocidadMovimiento,
            int vidaMaxima,
            int puntosAtaque,
            int alcanceAtaque)
            : base(coordenada, velocidadMovimiento, vidaMaxima, puntosAtaque, alcanceAtaque)
        {
        }
    }
}
