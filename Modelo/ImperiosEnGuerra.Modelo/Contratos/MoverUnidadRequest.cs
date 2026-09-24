namespace ImperiosEnGuerra.Modelo.Contratos;

/// <summary>
/// Representa mover unidad request dentro del modelo del juego.
/// </summary>
public sealed class MoverUnidadRequest
{
    /// <summary>
    /// Obtiene o establece unidad id.
    /// </summary>
    public string? UnidadId { get; set; }
    /// <summary>
    /// Obtiene o establece destino.
    /// </summary>
    public CoordenadaRequest? Destino { get; set; }
}
