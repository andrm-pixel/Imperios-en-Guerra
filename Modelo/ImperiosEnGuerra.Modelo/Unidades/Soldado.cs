using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Soldado cuerpo a cuerpo del prototipo (antes Guerrero).
    /// Lo crean <see cref="Acciones.FabricaUnidades"/> y
    /// <see cref="Acciones.OperacionEntrenamiento"/> por su nombre.
    /// </summary>
    public class Soldado : UnidadMilitar
    {
        /// <summary>
        /// Inicializa el Soldado en la coordenada indicada.
        /// </summary>
        public Soldado(Coordenada coordenada)
            : base(coordenada, 1.00d, 120, 25, 1)
        {
        }
    }
}
