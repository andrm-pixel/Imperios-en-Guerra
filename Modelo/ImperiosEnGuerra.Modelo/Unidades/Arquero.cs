using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa la especializacion Arquero con la posicion y disponibilidad heredadas.
    /// </summary>
    public class Arquero : UnidadMilitar
    {
       /// <summary>
       /// Inicializa la unidad delegando la posicion y la disponibilidad inicial en su clase base.
       /// </summary>
       /// <param name="coordenada">Posicion logica inicial, conservada sin validacion.</param>
       public Arquero(Coordenada coordenada)
       : base(coordenada, 1.10d, 90, 15, 4)
        {
            
        } 
    }
}
