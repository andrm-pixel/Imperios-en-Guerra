namespace ImperiosEnGuerra.Modelo.Contratos;

public sealed class EstadoPartidaResponse
{
    public string Estado { get; set; }
    public MapaEstadoResponse Mapa { get; set; }
    public JugadorEstadoResponse JugadorHumano { get; set; }
    public JugadorEstadoResponse JugadorMaquina { get; set; }
    public EconomiaEstadoResponse Economia { get; set; }
}

public sealed class EconomiaEstadoResponse
{
    public CostoEstadoResponse CentroUrbano { get; set; }
    public CostoEstadoResponse Aldeano { get; set; }
    public CostoEstadoResponse Guerrero { get; set; }
    public CostoEstadoResponse Lancero { get; set; }
    public CostoEstadoResponse Arquero { get; set; }
    public CostoEstadoResponse Monje { get; set; }
}

public sealed class CostoEstadoResponse
{
    public int Oro { get; set; }
    public int Madera { get; set; }
    public int Comida { get; set; }
    public int Piedra { get; set; }
    public int Hierro { get; set; }
}

public sealed class MapaEstadoResponse
{
    public int Ancho { get; set; }
    public int Alto { get; set; }
    public IReadOnlyList<RecursoEstadoResponse> Recursos { get; set; }
}

public sealed class JugadorEstadoResponse
{
    public string Nombre { get; set; }
    public string Tipo { get; set; }
    public RecursosJugadorEstadoResponse Recursos { get; set; }
    public IReadOnlyList<EdificioEstadoResponse> Edificios { get; set; }
    public IReadOnlyList<ObraConstruccionEstadoResponse> ObrasConstruccion { get; set; }
    public IReadOnlyList<UnidadEstadoResponse> Unidades { get; set; }
}

public sealed class RecursosJugadorEstadoResponse
{
    public int Oro { get; set; }
    public int Madera { get; set; }
    public int Comida { get; set; }
    public int Piedra { get; set; }
    public int Hierro { get; set; }
}

public sealed class RecursoEstadoResponse
{
    public string Tipo { get; set; }
    public CoordenadaEstadoResponse Coordenada { get; set; }
    public int CantidadRestante { get; set; }
}

public sealed class EdificioEstadoResponse
{
    public string Id { get; set; }
    public string Tipo { get; set; }
    public CoordenadaEstadoResponse Coordenada { get; set; }
    public IReadOnlyList<EntrenamientoEstadoResponse> ColaEntrenamiento { get; set; }
}

public sealed class EntrenamientoEstadoResponse
{
    public string Id { get; set; }
    public string TipoUnidad { get; set; }
    public int Progreso { get; set; }
}

public sealed class ObraConstruccionEstadoResponse
{
    public string Id { get; set; }
    public string Tipo { get; set; }
    public CoordenadaEstadoResponse Coordenada { get; set; }
    public int Progreso { get; set; }
}

public sealed class UnidadEstadoResponse
{
    public string Id { get; set; }
    public string Tipo { get; set; }
    public CoordenadaEstadoResponse? Coordenada { get; set; }
    public bool Disponible { get; set; }

    public string Estado { get; set; }
    public string? OrdenActiva { get; set; }

    public int CapacidadCarga { get; set; }
    public int CargaActual { get; set; }
    public string? TipoCarga { get; set; }
}

public sealed class CoordenadaEstadoResponse
{
    public int X { get; set; }
    public int Y { get; set; }
}
