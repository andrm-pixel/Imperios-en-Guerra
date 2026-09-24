using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa una unidad de combate de tipo Lancero.
    /// Se conecta con <see cref="Soldado"/> (hereda vida y ataque),
    /// <see cref="Acciones.FabricaUnidades"/> y
    /// <see cref="Acciones.OperacionEntrenamiento"/> (la crean por nombre),
    /// <see cref="Reglas.ReglasAcciones"/> (la autoriza a atacar) y
    /// <see cref="Acciones.OperacionAtaque"/> (usa su alcance 2).
    /// </summary>
    public class Lancero : Soldado
    {
        /// <summary>
        /// Inicializa el Lancero en la coordenada indicada.
        /// </summary>
        public Lancero(Coordenada coordenada)
            : base(coordenada, 1.20d, 100, 20, 2)
        {
        }
    }
}
