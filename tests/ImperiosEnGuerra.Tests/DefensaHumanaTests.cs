using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de las reacciones defensivas humanas y la retirada de la IA.</summary>
public class DefensaHumanaTests
{
    [Test]
    public void Aldeano_HuyeDeMilitarEnemigoCercano()
    {
        Partida partida = CrearPartida(
            new Coordenada(2, 2),
            new Coordenada(3, 2));

        var aldeano = partida.JugadorHumano.Unidades.OfType<Aldeano>().Single();
        int distanciaAntes = Distancia(aldeano.Coordenada, new Coordenada(3, 2));

        ResultadoAccion resultado = DefensaHumana.Ejecutar(
            partida,
            out int acciones);

        Assert.That(resultado.Exito, Is.True);
        Assert.That(acciones, Is.EqualTo(1));
        Assert.That(Distancia(aldeano.Coordenada, new Coordenada(3, 2)),
            Is.GreaterThan(distanciaAntes));
        Assert.That(aldeano.Disponible, Is.True);
        Assert.That(aldeano.OrdenActiva, Is.Null);
    }

    [Test]
    public void Aldeano_NoHuyeSiLaAmenazaEstaLejos()
    {
        Partida partida = CrearPartida(
            new Coordenada(2, 2),
            new Coordenada(2, 6));

        var aldeano = partida.JugadorHumano.Unidades.OfType<Aldeano>().Single();
        Coordenada posicionOriginal = aldeano.Coordenada;

        ResultadoAccion resultado = DefensaHumana.Ejecutar(
            partida,
            out int acciones);

        Assert.That(resultado.Exito, Is.True);
        Assert.That(acciones, Is.EqualTo(0));
        Assert.That(aldeano.Coordenada, Is.EqualTo(posicionOriginal));
    }

    [Test]
    public void Aldeano_NoInterrumpeOrdenDelJugador()
    {
        Partida partida = CrearPartida(
            new Coordenada(2, 2),
            new Coordenada(3, 2));

        var aldeano = partida.JugadorHumano.Unidades.OfType<Aldeano>().Single();
        Assert.That(aldeano.IntentarIniciarOrden(TipoAccionJuego.Recolectar),
            Is.True);
        Coordenada posicionOriginal = aldeano.Coordenada;

        DefensaHumana.Ejecutar(partida, out int acciones);

        Assert.That(acciones, Is.EqualTo(0));
        Assert.That(aldeano.Coordenada, Is.EqualTo(posicionOriginal));
        Assert.That(aldeano.OrdenActiva, Is.EqualTo(TipoAccionJuego.Recolectar));
    }

    [Test]
    public void Ia_RetiraUnidadMilitarMuyHeridaHaciaLaGuardia()
    {
        var mapa = new Mapa(10, 10);
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

        humano.AgregarUnidad(new Aldeano(new Coordenada(9, 0)));
        var soldado = new Soldado(new Coordenada(1, 1));
        maquina.AgregarUnidad(soldado);
        maquina.AgregarEdificio(new Castillo(new Coordenada(8, 8)));
        mapa.ObtenerCasilla(8, 8).Ocupar();

        soldado.RecibirDano(90);
        int antes = Distancia(soldado.Coordenada, new Coordenada(8, 8));

        ResultadoAccion resultado = new InteligenciaMaquina()
            .EjecutarTurno(new Partida(humano, maquina));

        int despues = Distancia(soldado.Coordenada, new Coordenada(8, 8));

        Assert.That(resultado.Exito, Is.True);
        Assert.That(despues, Is.LessThan(antes));
    }

    private static Partida CrearPartida(
        Coordenada posicionAldeano,
        Coordenada posicionMilitar)
    {
        var mapa = new Mapa(8, 8);
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

        humano.AgregarUnidad(new Aldeano(posicionAldeano));
        maquina.AgregarUnidad(new Soldado(posicionMilitar));

        return new Partida(humano, maquina);
    }

    private static int Distancia(Coordenada a, Coordenada b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
