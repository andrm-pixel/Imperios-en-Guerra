using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

public class InteligenciaMaquinaTests
{
    [Test]
    public void Turno_GolpeaHumanoAdyacente()
    {
        Partida partida = CrearPartida(
            new Coordenada(1, 1),
            new Coordenada(2, 1),
            withCentro: false);

        var aldeano = partida.JugadorHumano.Unidades.OfType<Aldeano>().First();
        var ia = new InteligenciaMaquina();

        ResultadoAccion turno = ia.EjecutarTurno(partida);

        Assert.That(turno.Exito, Is.True);
        Assert.That(aldeano.Vida, Is.EqualTo(25));
    }

    [Test]
    public void Turno_SinObjetivoCercano_RegresaAGuardia()
    {
        Partida partida = CrearPartida(
            new Coordenada(1, 1),
            new Coordenada(5, 5),
            withCentro: true);

        var guerrero = partida.JugadorMaquina.Unidades.OfType<Guerrero>().First();
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

    [Test]
    public void Turno_EliminaUltimoHumano_DeclaraVictoriaMaquina()
    {
        Partida partida = CrearPartida(
            new Coordenada(1, 1),
            new Coordenada(2, 1),
            withCentro: false);

        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(partida);

        ResultadoAccion primero = servicio.EjecutarTurnoMaquina();
        Assert.That(primero.Exito, Is.True);

        ResultadoAccion segundo = servicio.EjecutarTurnoMaquina();
        Assert.That(segundo.Exito, Is.True);
        Assert.That(segundo.Mensaje, Does.Contain("¡Victoria!"));
        Assert.That(partida.JugadorHumano.Unidades.Count, Is.EqualTo(0));
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
        maquina.AgregarUnidad(new Guerrero(guerreroMaquina));

        if (withCentro)
        {
            var centro = new CentroUrbano(new Coordenada(8, 8));
            maquina.AgregarEdificio(centro);
            mapa.ObtenerCasilla(8, 8).Ocupar();
        }

        return new Partida(humano, maquina);
    }
}
