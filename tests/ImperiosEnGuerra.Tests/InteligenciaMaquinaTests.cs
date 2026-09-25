using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de Inteligencia Maquina: verifica inteligencia maquina.</summary>
public class InteligenciaMaquinaTests
{
    // Caso Turno: verifica golpea militar adyacente e ignora aldeanos.
    [Test]
    public void Turno_GolpeaMilitarAdyacente()
    {
        Partida partida = CrearPartida(
            new Coordenada(1, 1),
            new Coordenada(2, 1),
            withCentro: false);

        var aldeano = partida.JugadorHumano.Unidades.OfType<Aldeano>().First();
        var soldadoHumano = new Soldado(new Coordenada(2, 2));
        partida.JugadorHumano.AgregarUnidad(soldadoHumano);

        var ia = new InteligenciaMaquina();

        ResultadoAccion turno = ia.EjecutarTurno(partida);

        Assert.That(turno.Exito, Is.True);
        Assert.That(soldadoHumano.Vida, Is.EqualTo(95));
        Assert.That(aldeano.Vida, Is.EqualTo(50));
    }

    // Caso Turno: verifica sin objetivo cercano - regresa a guardia.
    [Test]
    public void Turno_SinObjetivoCercano_RegresaAGuardia()
    {
        Partida partida = CrearPartida(
            new Coordenada(1, 1),
            new Coordenada(5, 5),
            withCentro: true);

        var guerrero = partida.JugadorMaquina.Unidades.OfType<Soldado>().First();
        int antes =
            Math.Abs(guerrero.Coordenada.X - 8) +
            Math.Abs(guerrero.Coordenada.Y - 8);

        var ia = new InteligenciaMaquina();
        ResultadoAccion turno = ia.EjecutarTurno(partida);

        Assert.That(turno.Exito, Is.True);

        int despues =
            Math.Abs(guerrero.Coordenada.X - 8) +
            Math.Abs(guerrero.Coordenada.Y - 8);

        Assert.That(despues, Is.LessThan(antes));
    }

    // Caso Turno: verifica elimina ultimo humano - declara victoria maquina.
    [Test]
    public void Turno_EliminaUltimoHumano_DeclaraVictoriaMaquina()
    {
        var mapa = new Mapa(10, 10);
        var humano = new Jugador(
            "Humano", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "Maquina", TipoJugador.Maquina, mapa, new RecursosJugador());

        humano.AgregarUnidad(new Soldado(new Coordenada(1, 1)));
        maquina.AgregarUnidad(new Soldado(new Coordenada(2, 1)));

        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(new Partida(humano, maquina));

        ResultadoAccion ultimo = null;

        for (int i = 0; i < 5; i++)
        {
            ultimo = servicio.EjecutarTurnoMaquina();
            Assert.That(ultimo.Exito, Is.True);
        }

        Assert.That(ultimo.Mensaje, Does.Contain("¡Victoria!"));
        Assert.That(humano.Unidades.Count, Is.EqualTo(0));
    }

    private static Partida CrearPartida(
        Coordenada aldeanoHumano,
        Coordenada guerreroMaquina,
        bool withCentro)
    {
        var mapa = new Mapa(10, 10);

        var humano = new Jugador(
            "Humano", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "Máquina", TipoJugador.Maquina, mapa, new RecursosJugador());

        humano.AgregarUnidad(new Aldeano(aldeanoHumano));
        maquina.AgregarUnidad(new Soldado(guerreroMaquina));

        if (withCentro)
        {
            var centro = new Castillo(new Coordenada(8, 8));
            maquina.AgregarEdificio(centro);
            mapa.ObtenerCasilla(8, 8).Ocupar();
        }

        return new Partida(humano, maquina);
    }
}
