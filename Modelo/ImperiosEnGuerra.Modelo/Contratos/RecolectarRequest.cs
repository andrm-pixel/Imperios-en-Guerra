namespace ImperiosEnGuerra.Modelo.Contratos;

/// <summary>
/// Representa recolectar request dentro del modelo del juego.
/// </summary>
public sealed class RecolectarRequest
{
    /// <summary>
    /// Obtiene o establece aldeano id.
    /// </summary>
    public string? AldeanoId { get; set; }
    /// <summary>
    /// Obtiene o establece objetivo.
    /// </summary>
    public CoordenadaRequest? Objetivo { get; set; }
}
