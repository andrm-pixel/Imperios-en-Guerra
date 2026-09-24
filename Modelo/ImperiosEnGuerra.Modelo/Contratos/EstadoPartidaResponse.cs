namespace ImperiosEnGuerra.Modelo.Contratos;

/// <summary>
/// Representa estado partida response dentro del modelo del juego.
/// </summary>
public sealed class EstadoPartidaResponse
{
    /// <summary>
    /// Obtiene o establece estado.
    /// </summary>
    public string Estado { get; set; }
    /// <summary>
    /// Obtiene o establece mapa.
    /// </summary>
    public MapaEstadoResponse Mapa { get; set; }
    /// <summary>
    /// Obtiene o establece jugador humano.
    /// </summary>
    public JugadorEstadoResponse JugadorHumano { get; set; }
    /// <summary>
    /// Obtiene o establece jugador maquina.
    /// </summary>
    public JugadorEstadoResponse JugadorMaquina { get; set; }
    /// <summary>
    /// Obtiene o establece economia.
    /// </summary>
    public EconomiaEstadoResponse Economia { get; set; }
}

/// <summary>
/// Representa economia estado response dentro del modelo del juego.
/// </summary>
public sealed class EconomiaEstadoResponse
{
    /// <summary>
    /// Obtiene o establece centro urbano.
    /// </summary>
    public CostoEstadoResponse CentroUrbano { get; set; }
    /// <summary>
    /// Obtiene o establece aldeano.
    /// </summary>
    public CostoEstadoResponse Aldeano { get; set; }
    /// <summary>
    /// Obtiene o establece soldado.
    /// </summary>
    public CostoEstadoResponse Soldado { get; set; }
    /// <summary>
    /// Obtiene o establece arquero.
    /// </summary>
    public CostoEstadoResponse Arquero { get; set; }
}

/// <summary>
/// Representa costo estado response dentro del modelo del juego.
/// </summary>
public sealed class CostoEstadoResponse
{
    /// <summary>
    /// Obtiene o establece oro.
    /// </summary>
    public int Oro { get; set; }
    /// <summary>
    /// Obtiene o establece madera.
    /// </summary>
    public int Madera { get; set; }
    /// <summary>
    /// Obtiene o establece comida.
    /// </summary>
    public int Comida { get; set; }
    /// <summary>
    /// Obtiene o establece piedra.
    /// </summary>
    public int Piedra { get; set; }
    /// <summary>
    /// Obtiene o establece hierro.
    /// </summary>
    public int Hierro { get; set; }
}

/// <summary>
/// Representa mapa estado response dentro del modelo del juego.
/// </summary>
public sealed class MapaEstadoResponse
{
    /// <summary>
    /// Obtiene o establece ancho.
    /// </summary>
    public int Ancho { get; set; }
    /// <summary>
    /// Obtiene o establece alto.
    /// </summary>
    public int Alto { get; set; }
    /// <summary>
    /// Obtiene o establece recursos.
    /// </summary>
    public IReadOnlyList<RecursoEstadoResponse> Recursos { get; set; }
}

/// <summary>
/// Representa jugador estado response dentro del modelo del juego.
/// </summary>
public sealed class JugadorEstadoResponse
{
    /// <summary>
    /// Obtiene o establece nombre.
    /// </summary>
    public string Nombre { get; set; }
    /// <summary>
    /// Obtiene o establece tipo.
    /// </summary>
    public string Tipo { get; set; }
    /// <summary>
    /// Obtiene o establece recursos.
    /// </summary>
    public RecursosJugadorEstadoResponse Recursos { get; set; }
    /// <summary>
    /// Obtiene o establece edificios.
    /// </summary>
    public IReadOnlyList<EdificioEstadoResponse> Edificios { get; set; }
    /// <summary>
    /// Obtiene o establece obras construccion.
    /// </summary>
    public IReadOnlyList<ObraConstruccionEstadoResponse> ObrasConstruccion { get; set; }
    /// <summary>
    /// Obtiene o establece unidades.
    /// </summary>
    public IReadOnlyList<UnidadEstadoResponse> Unidades { get; set; }
}

/// <summary>
/// Representa recursos jugador estado response dentro del modelo del juego.
/// </summary>
public sealed class RecursosJugadorEstadoResponse
{
    /// <summary>
    /// Obtiene o establece oro.
    /// </summary>
    public int Oro { get; set; }
    /// <summary>
    /// Obtiene o establece madera.
    /// </summary>
    public int Madera { get; set; }
    /// <summary>
    /// Obtiene o establece comida.
    /// </summary>
    public int Comida { get; set; }
    /// <summary>
    /// Obtiene o establece piedra.
    /// </summary>
    public int Piedra { get; set; }
    /// <summary>
    /// Obtiene o establece hierro.
    /// </summary>
    public int Hierro { get; set; }
}

/// <summary>
/// Representa recurso estado response dentro del modelo del juego.
/// </summary>
public sealed class RecursoEstadoResponse
{
    /// <summary>
    /// Obtiene o establece tipo.
    /// </summary>
    public string Tipo { get; set; }
    /// <summary>
    /// Obtiene o establece coordenada.
    /// </summary>
    public CoordenadaEstadoResponse Coordenada { get; set; }
    /// <summary>
    /// Obtiene o establece cantidad restante.
    /// </summary>
    public int CantidadRestante { get; set; }
}

/// <summary>
/// Representa edificio estado response dentro del modelo del juego.
/// </summary>
public sealed class EdificioEstadoResponse
{
    /// <summary>
    /// Obtiene o establece id.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Obtiene o establece tipo.
    /// </summary>
    public string Tipo { get; set; }
    /// <summary>
    /// Obtiene o establece coordenada.
    /// </summary>
    public CoordenadaEstadoResponse Coordenada { get; set; }
    /// <summary>
    /// Obtiene o establece cola entrenamiento.
    /// </summary>
    public IReadOnlyList<EntrenamientoEstadoResponse> ColaEntrenamiento { get; set; }
}

/// <summary>
/// Representa entrenamiento estado response dentro del modelo del juego.
/// </summary>
public sealed class EntrenamientoEstadoResponse
{
    /// <summary>
    /// Obtiene o establece id.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Obtiene o establece tipo unidad.
    /// </summary>
    public string TipoUnidad { get; set; }
    /// <summary>
    /// Obtiene o establece progreso.
    /// </summary>
    public int Progreso { get; set; }
}

/// <summary>
/// Representa obra construccion estado response dentro del modelo del juego.
/// </summary>
public sealed class ObraConstruccionEstadoResponse
{
    /// <summary>
    /// Obtiene o establece id.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Obtiene o establece tipo.
    /// </summary>
    public string Tipo { get; set; }
    /// <summary>
    /// Obtiene o establece coordenada.
    /// </summary>
    public CoordenadaEstadoResponse Coordenada { get; set; }
    /// <summary>
    /// Obtiene o establece progreso.
    /// </summary>
    public int Progreso { get; set; }
}

/// <summary>
/// Representa unidad estado response dentro del modelo del juego.
/// </summary>
public sealed class UnidadEstadoResponse
{
    /// <summary>
    /// Obtiene o establece id.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Obtiene o establece tipo.
    /// </summary>
    public string Tipo { get; set; }
    /// <summary>
    /// Obtiene o establece coordenada.
    /// </summary>
    public CoordenadaEstadoResponse? Coordenada { get; set; }
    /// <summary>
    /// Obtiene o establece disponible.
    /// </summary>
    public bool Disponible { get; set; }

    /// <summary>
    /// Obtiene o establece estado.
    /// </summary>
    public string Estado { get; set; }
    /// <summary>
    /// Obtiene o establece orden activa.
    /// </summary>
    public string? OrdenActiva { get; set; }

    /// <summary>
    /// Obtiene o establece capacidad carga.
    /// </summary>
    public int CapacidadCarga { get; set; }
    /// <summary>
    /// Obtiene o establece carga actual.
    /// </summary>
    public int CargaActual { get; set; }
    /// <summary>
    /// Obtiene o establece tipo carga.
    /// </summary>
    public string? TipoCarga { get; set; }
}

/// <summary>
/// Representa coordenada estado response dentro del modelo del juego.
/// </summary>
public sealed class CoordenadaEstadoResponse
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
