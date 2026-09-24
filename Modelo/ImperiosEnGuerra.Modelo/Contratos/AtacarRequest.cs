namespace ImperiosEnGuerra.Modelo.Contratos;

/// <summary>
/// Representa atacar request dentro del modelo del juego.
/// </summary>
public sealed class AtacarRequest
{
    /// <summary>
    /// Obtiene o establece atacante id.
    /// </summary>
    public string? AtacanteId { get; set; }
    /// <summary>
    /// Obtiene o establece objetivo id.
    /// </summary>
    public string? ObjetivoId { get; set; }
}
