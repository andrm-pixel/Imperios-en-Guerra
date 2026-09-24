using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de Obra Construccion: verifica obra construccion.</summary>
public class ObraConstruccionTests
{
    // Caso Iniciar Obra: verifica reserva casilla sin crear edificio terminado.
    [Test]
    public void IniciarObra_ReservaCasillaSinCrearEdificioTerminado()
    {
        Partida partida =
            CrearPartida(
                out Aldeano aldeano);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(partida);

        ResultadoAccion resultado =
            estado.IniciarObra(
                Request(aldeano, 3, 3),
                out Guid obraId);

        Assert.That(resultado.Exito, Is.True);
        Assert.That(obraId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(
            partida.JugadorHumano.Edificios,
            Is.Empty);
        Assert.That(
            partida.JugadorHumano.ObrasConstruccion.Count,
            Is.EqualTo(1));
        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerCasilla(3, 3)
                .EstaOcupada,
            Is.True);
    }

    // Caso Cancelar Obra: verifica libera casilla.
    [Test]
    public void CancelarObra_LiberaCasilla()
    {
        Partida partida =
            CrearPartida(
                out Aldeano aldeano);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(partida);

        estado.IniciarObra(
            Request(aldeano, 3, 3),
            out Guid obraId);

        Assert.That(
            estado.CancelarObra(obraId),
            Is.True);

        Assert.That(
            partida.JugadorHumano.ObrasConstruccion,
            Is.Empty);

        Assert.That(
            partida.JugadorHumano.Mapa
                .ObtenerCasilla(3, 3)
                .EstaOcupada,
            Is.False);
    }

    // Caso Progreso100: verifica convierte obra en centro urbano una sola vez.
    [Test]
    public void Progreso100_ConvierteObraEnCastilloUnaSolaVez()
    {
        Partida partida =
            CrearPartida(
                out Aldeano aldeano);

        var estado =
            new EstadoPartidaService();

        estado.EstablecerPartida(partida);

        estado.IniciarObra(
            Request(aldeano, 3, 3),
            out Guid obraId);

        for (int i = 0; i < 10; i++)
        {
            var avance =
                estado.AvanzarObra(
                    obraId,
                    10);

            Assert.That(avance.Exito, Is.True);
        }

        Assert.That(
            partida.JugadorHumano.ObrasConstruccion,
            Is.Empty);

        Assert.That(
            partida.JugadorHumano.Edificios.Count,
            Is.EqualTo(1));

        Assert.That(
            estado.AvanzarObra(
                obraId,
                10).Exito,
            Is.False);

        Assert.That(
            partida.JugadorHumano.Edificios.Count,
            Is.EqualTo(1));
    }

    private static Partida CrearPartida(
        out Aldeano aldeano)
    {
        var mapa = new Mapa(6, 6);
        var humano = new Jugador(
            "Humano",
            TipoJugador.Humano,
            mapa,
            new RecursosJugador());
        var maquina = new Jugador(
            "Maquina",
            TipoJugador.Maquina,
            mapa,
            new RecursosJugador());

        aldeano =
            new Aldeano(
                new Coordenada(1, 1));

        humano.AgregarUnidad(
            aldeano);

        return new Partida(
            humano,
            maquina);
    }

    private static ConstruirRequest Request(
        Aldeano aldeano,
        int x,
        int y)
    {
        return new ConstruirRequest
        {
            AldeanoId =
                aldeano.Id.ToString("D"),
            TipoEdificio =
                "Castillo",
            Destino =
                new CoordenadaRequest
                {
                    X = x,
                    Y = y
                }
        };
    }
}
