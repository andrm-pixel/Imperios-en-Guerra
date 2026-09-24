namespace ImperiosEnGuerra.Modelo.Contratos;

/// <summary>
/// Representa iniciar partida request dentro del modelo del juego.
/// </summary>
public sealed class IniciarPartidaRequest
{
    /// <summary>
    /// Obtiene o establece nombre humano.
    /// </summary>
    public string? NombreHumano { get; set; }
    /// <summary>
    /// Obtiene o establece nombre maquina.
    /// </summary>
    public string? NombreMaquina { get; set; }

    /// <summary>
    /// Obtiene o establece ancho mapa.
    /// </summary>
    public int AnchoMapa { get; set; }
    /// <summary>
    /// Obtiene o establece alto mapa.
    /// </summary>
    public int AltoMapa { get; set; }

    /// <summary>
    /// Obtiene o establece centro humano.
    /// </summary>
    public CoordenadaRequest? CentroHumano { get; set; }
    /// <summary>
    /// Obtiene o establece centro maquina.
    /// </summary>
    public CoordenadaRequest? CentroMaquina { get; set; }

    /// <summary>
    /// Obtiene o establece recursos humano.
    /// </summary>
    public List<RecursoInicialRequest>? RecursosHumano { get; set; }
    /// <summary>
    /// Obtiene o establece recursos maquina.
    /// </summary>
    public List<RecursoInicialRequest>? RecursosMaquina { get; set; }
}

/// <summary>
/// Representa coordenada request dentro del modelo del juego.
/// </summary>
public sealed class CoordenadaRequest
{
    /// <summary>
    /// Obtiene o establece x.
    /// </summary>
    public int X { get; set; }
    /// <summary>
    /// Obtiene o establece y.
    /// </summary>
    public int Y { get; set; }
}

/// <summary>
/// Representa recurso inicial request dentro del modelo del juego.
/// </summary>
public sealed class RecursoInicialRequest
{
    /// <summary>
    /// Obtiene o establece tipo.
    /// </summary>
    public string? Tipo { get; set; }
    /// <summary>
    /// Obtiene o establece x.
    /// </summary>
    public int X { get; set; }
    /// <summary>
    /// Obtiene o establece y.
    /// </summary>
    public int Y { get; set; }
}
