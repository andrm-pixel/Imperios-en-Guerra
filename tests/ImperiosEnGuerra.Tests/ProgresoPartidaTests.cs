using System.IO;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Persistencia;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de ProgresoPartida: verifica guardado y carga en TXT.</summary>
public class ProgresoPartidaTests
{
    // Caso IdaVuelta: verifica serializar y deserializar conserva todo.
    [Test]
    public void IdaVuelta_ConservaMapaSaldosYEntidades()
    {
        var mapa = new Mapa(15, 15);
        var humano = new Jugador(
            "Griegos", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "CPU", TipoJugador.Maquina, mapa, new RecursosJugador());

        humano.Recursos.Agregar(TipoRecurso.Oro, 42);
        humano.Recursos.Agregar(TipoRecurso.Piedra, 7);
        humano.Recursos.Agregar(TipoRecurso.Hierro, 3);

        var aldeano = new Aldeano(new Coordenada(1, 1));
        humano.AgregarUnidad(aldeano);
        var guerrero = new Soldado(new Coordenada(2, 2));
        guerrero.RecibirDano(10);
        humano.AgregarUnidad(guerrero);
        humano.AgregarEdificio(new Castillo(new Coordenada(1, 7)));
        mapa.ObtenerCasilla(1, 7).Ocupar();

        var enemigo = new Arquero(new Coordenada(13, 7));
        enemigo.RecibirDano(5);
        maquina.AgregarUnidad(enemigo);

        Assert.That(
            mapa.ColocarRecurso(new Recurso(TipoRecurso.Oro, new Coordenada(5, 5), 33)),
            Is.True);

        var original = new Partida(humano, maquina);

        string texto = ProgresoPartida.Serializar(original);
        Assert.That(texto, Does.Contain("PROGRESO_V1"));
        Assert.That(texto, Does.Contain("Mapa=15x15"));

        Partida cargada = ProgresoPartida.Deserializar(texto);

        Assert.That(cargada.JugadorHumano.Nombre, Is.EqualTo("Griegos"));
        Assert.That(cargada.JugadorHumano.Mapa.Ancho, Is.EqualTo(15));
        Assert.That(
            cargada.JugadorHumano.Recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.EqualTo(42));
        Assert.That(
            cargada.JugadorHumano.Recursos.ObtenerCantidad(TipoRecurso.Piedra),
            Is.EqualTo(7));
        Assert.That(
            cargada.JugadorHumano.Recursos.ObtenerCantidad(TipoRecurso.Hierro),
            Is.EqualTo(3));
        Assert.That(cargada.JugadorHumano.Unidades.Count, Is.EqualTo(2));
        Assert.That(cargada.JugadorHumano.Edificios.Count, Is.EqualTo(1));

        var guerreroCargado = cargada.JugadorHumano.Unidades
            .OfType<Soldado>().Single();
        Assert.That(guerreroCargado.Vida, Is.EqualTo(110));
        Assert.That(guerreroCargado.Coordenada.X, Is.EqualTo(2));

        var enemigoCargado = cargada.JugadorMaquina.Unidades
            .OfType<Arquero>().Single();
        Assert.That(enemigoCargado.Vida, Is.EqualTo(85));

        Assert.That(
            cargada.JugadorHumano.Mapa.ObtenerRecursoEn(new Coordenada(5, 5)).CantidadRestante,
            Is.EqualTo(33));
    }

    // Caso ContenidoInvalido: verifica rechaza texto malformado.
    [Test]
    public void ContenidoInvalido_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => ProgresoPartida.Deserializar(null));
        Assert.Throws<FormatException>(() => ProgresoPartida.Deserializar("basura"));
        Assert.Throws<FormatException>(() => ProgresoPartida.Deserializar("PROGRESO_V1\nMapa=xx\n"));
    }

    // Caso Servicio: verifica guardar y cargar por archivo redondo.
    [Test]
    public void Servicio_GuardarYCargarPorArchivo()
    {
        string directorio = Path.Combine(
            Path.GetTempPath(),
            "Progreso_" + Guid.NewGuid().ToString("N"));

        var archivos = new ServicioArchivos(directorio);
        var servicio = new EstadoPartidaService(archivos);

        var mapa = new Mapa(15, 15);
        var humano = new Jugador(
            "Griegos", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "CPU", TipoJugador.Maquina, mapa, new RecursosJugador());
        humano.AgregarUnidad(new Aldeano(new Coordenada(1, 1)));
        maquina.AgregarUnidad(new Aldeano(new Coordenada(13, 13)));
        servicio.EstablecerPartida(new Partida(humano, maquina));

        Assert.That(servicio.GuardarProgreso().Exito, Is.True);
        Assert.That(File.Exists(Path.Combine(directorio, "progreso.txt")), Is.True);

        humano.Recursos.Agregar(TipoRecurso.Oro, 999);
        Assert.That(
            servicio.ObtenerEstado().JugadorHumano.Recursos.Oro,
            Is.EqualTo(999));

        Assert.That(servicio.CargarProgreso().Exito, Is.True);
        Assert.That(
            servicio.ObtenerEstado().JugadorHumano.Recursos.Oro,
            Is.Zero);

        Directory.Delete(directorio, true);
    }
}
