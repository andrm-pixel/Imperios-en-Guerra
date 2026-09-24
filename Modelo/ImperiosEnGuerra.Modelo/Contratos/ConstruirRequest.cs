namespace ImperiosEnGuerra.Modelo.Contratos;

/// <summary>
/// Representa construir request dentro del modelo del juego.
/// </summary>
public sealed class ConstruirRequest
{
    /// <summary>
    /// Obtiene o establece aldeano id.
    /// </summary>
    public string? AldeanoId { get; set; }
    /// <summary>
    /// Obtiene o establece tipo edificio.
    /// </summary>
    public string? TipoEdificio { get; set; }
    /// <summary>
    /// Obtiene o establece destino.
    /// </summary>
    public CoordenadaRequest? Destino { get; set; }
}
