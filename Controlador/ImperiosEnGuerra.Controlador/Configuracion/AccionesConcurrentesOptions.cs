namespace ImperiosEnGuerra.Controlador.Configuracion;

/// <summary>Duraciones configurables de las acciones concurrentes (en segundos).</summary>
public class AccionesConcurrentesOptions
{
    /// <summary>Duración del movimiento de unidades.</summary>
    public int MovimientoSegundos { get; set; }
    /// <summary>Duración de la recolección de recursos.</summary>
    public int RecoleccionSegundos { get; set; }
    /// <summary>Duración de la construcción de edificios.</summary>
    public int ConstruccionSegundos { get; set; }
    /// <summary>Duración del entrenamiento de unidades.</summary>
    public int EntrenamientoSegundos { get; set; }
    /// <summary>Duración del ataque entre unidades.</summary>
    public int AtaqueSegundos { get; set; }
}