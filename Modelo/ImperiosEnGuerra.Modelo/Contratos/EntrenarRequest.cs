namespace ImperiosEnGuerra.Modelo.Contratos;

/// <summary>
/// Representa entrenar request dentro del modelo del juego.
/// </summary>
public sealed class EntrenarRequest
{
    /// <summary>
    /// Obtiene o establece edificio origen.
    /// </summary>
    public CoordenadaRequest? EdificioOrigen { get; set; }
    /// <summary>
    /// Obtiene o establece tipo unidad.
    /// </summary>
    public string? TipoUnidad { get; set; }
    /// <summary>
    /// Obtiene o establece destino.
    /// </summary>
    public CoordenadaRequest? Destino { get; set; }
}
