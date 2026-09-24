using System.Linq;
using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Modelo.Concurrencia;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de Entrenamiento Concurrente: verifica entrenamiento concurrente.</summary>
public class EntrenamientoConcurrenteTests
{
    // Caso Entrenamiento Concurrente: verifica crea unidad desde worker.
    [Test]
    public async Task EntrenamientoConcurrente_CreaUnidadDesdeWorker()
    {
        Partida partida = CrearPartida();

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        ProcesoConcurrente proceso =
            servicio.IniciarEntrenamiento(
                CrearRequest("Soldado", 2, 2));

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(EstadoProcesoConcurrente.Completado));

        Assert.That(resultado.Resultado, Is.Not.Null);
        Assert.That(resultado.Resultado.Exito, Is.True);

        Assert.That(
            partida.JugadorHumano.Unidades.Count,
            Is.EqualTo(1));

        Assert.That(
            partida.JugadorHumano.Unidades[0],
            Is.TypeOf<Soldado>());

        Coordenada spawn =
            partida.JugadorHumano.Unidades[0]
                .Coordenada;

        Assert.That(
            Math.Abs(spawn.X - 1) +
            Math.Abs(spawn.Y - 1),
            Is.EqualTo(1));

        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerCasilla(spawn.X, spawn.Y)
                .EstaOcupada,
            Is.True);
    }

    // Caso Cancelar Entrenamiento: verifica antes de aplicar - no crea unidad.
    [Test]
    public async Task CancelarEntrenamiento_AntesDeAplicar_NoCreaUnidad()
    {
        Partida partida = CrearPartida();

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromSeconds(10));

        ProcesoConcurrente proceso =
            servicio.IniciarEntrenamiento(
                CrearRequest("Arquero", 2, 2));

        Assert.That(
            servicio.Cancelar(proceso.Id),
            Is.True);

        await proceso.Finalizacion;

        Assert.That(
            servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(
            resultado.Estado,
            Is.EqualTo(EstadoProcesoConcurrente.Cancelado));

        Assert.That(
            partida.JugadorHumano.Unidades,
            Is.Empty);

        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerCasilla(2, 2)
                .EstaOcupada,
            Is.False);
    }

    // Caso Dos Entrenamientos: verifica se encolan y ambos usan spawn seguro.
    [Test]
    public async Task DosEntrenamientos_SeEncolanYAmbosUsanSpawnSeguro()
    {
        Partida partida = CrearPartida();

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        ProcesoConcurrente primero =
            servicio.IniciarEntrenamiento(
                CrearRequest("Arquero", 3, 3));

        ProcesoConcurrente segundo =
            servicio.IniciarEntrenamiento(
                CrearRequest("Arquero", 3, 3));

        await Task.WhenAll(
            primero.Finalizacion,
            segundo.Finalizacion);

        Assert.That(
            partida.JugadorHumano.Unidades.Count,
            Is.EqualTo(2));

        Assert.That(
            partida.JugadorHumano.Unidades[0].Coordenada.X ==
            partida.JugadorHumano.Unidades[1].Coordenada.X &&
            partida.JugadorHumano.Unidades[0].Coordenada.Y ==
            partida.JugadorHumano.Unidades[1].Coordenada.Y,
            Is.False);
    }

    private static Partida CrearPartida()
    {
        var mapa = new Mapa(6, 6);

        var humano = new Jugador(
            "Humano",
            TipoJugador.Humano,
            mapa,
            new RecursosJugador());

        var maquina = new Jugador(
            "Máquina",
            TipoJugador.Maquina,
            mapa,
            new RecursosJugador());

        humano.Recursos.Agregar(TipoRecurso.Oro, 500);
        humano.Recursos.Agregar(TipoRecurso.Madera, 500);
        humano.Recursos.Agregar(TipoRecurso.Comida, 500);
        humano.Recursos.Agregar(TipoRecurso.Piedra, 500);
        humano.Recursos.Agregar(TipoRecurso.Hierro, 500);

        humano.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(1, 1)));

        maquina.AgregarEdificio(
            new CentroUrbano(
                new Coordenada(5, 5)));

        mapa.ObtenerCasilla(1, 1).Ocupar();
        mapa.ObtenerCasilla(5, 5).Ocupar();

        return new Partida(humano, maquina);
    }

    private static EntrenarRequest CrearRequest(
        string tipoUnidad,
        int x,
        int y)
    {
        return new EntrenarRequest
        {
            EdificioOrigen = new CoordenadaRequest
            {
                X = 1,
                Y = 1
            },
            TipoUnidad = tipoUnidad,
            Destino = new CoordenadaRequest
            {
                X = x,
                Y = y
            }
        };
    }
}
