using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Modelo.Concurrencia;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de Ataque Concurrente: verifica ataque concurrente.</summary>
public class AtaqueConcurrenteTests
{
    // Caso Ataque Concurrente: verifica ejecuta en worker y publica resultado.
    [Test]
    public async Task AtaqueConcurrente_EjecutaEnWorkerYPublicaResultado()
    {
        Partida partida = CrearPartida(
            out Guerrero atacante,
            out Arquero objetivo);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.Zero);

        ProcesoConcurrente proceso =
            servicio.IniciarAtaque(
                new AtacarRequest
                {
                    AtacanteId = atacante.Id.ToString(),
                    ObjetivoId = objetivo.Id.ToString()
                });

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
        Assert.That(resultado.Resultado.Mensaje,
            Does.Contain("destruid"));
    }


    // Caso Cancelar Ataque: verifica antes de aplicar - no modifica modelo.
    [Test]
    public async Task CancelarAtaque_AntesDeAplicar_NoModificaModelo()
    {
        Partida partida = CrearPartida(
            out Guerrero atacante,
            out Arquero objetivo);

        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partida);

        using var gestor = new GestorProcesosConcurrentes();

        var servicio = new ServicioAccionesConcurrentes(
            estado,
            gestor,
            TimeSpan.FromSeconds(10));


        ProcesoConcurrente proceso =
            servicio.IniciarAtaque(
                new AtacarRequest
                {
                    AtacanteId = atacante.Id.ToString(),
                    ObjetivoId = objetivo.Id.ToString()
                });


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


        Assert.That(objetivo.Coordenada.X, Is.EqualTo(2));
        Assert.That(objetivo.Coordenada.Y, Is.EqualTo(1));
    }


    // Caso Objetivo Lejos: verifica camina hasta alcance y destruye.
    [Test]
    public async Task ObjetivoLejos_CaminaHastaAlcanceYDestruye()
    {
        var mapa = new Mapa(8, 8);
        var humano = new Jugador(
            "Humano", TipoJugador.Humano, mapa, new RecursosJugador());
        var maquina = new Jugador(
            "Máquina", TipoJugador.Maquina, mapa, new RecursosJugador());

        var atacanteLejos = new Guerrero(new Coordenada(1, 1));
        var objetivoLejos = new Arquero(new Coordenada(4, 4));
        humano.AgregarUnidad(atacanteLejos);
        maquina.AgregarUnidad(objetivoLejos);

        var partidaLejos = new Partida(humano, maquina);
        var estado = new EstadoPartidaService();
        estado.EstablecerPartida(partidaLejos);

        using var gestor = new GestorProcesosConcurrentes();
        var servicio = new ServicioAccionesConcurrentes(
            estado, gestor, TimeSpan.Zero);

        ProcesoConcurrente proceso = servicio.IniciarAtaque(
            new AtacarRequest
            {
                AtacanteId = atacanteLejos.Id.ToString(),
                ObjetivoId = objetivoLejos.Id.ToString()
            });

        Assert.That(
            proceso.Finalizacion.Wait(TimeSpan.FromSeconds(30)),
            Is.True);

        Assert.That(
            servicio.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado),
            Is.True);

        Assert.That(resultado.Resultado, Is.Not.Null);
        Assert.That(resultado.Resultado.Exito, Is.True);
        Assert.That(maquina.Unidades.Contains(objetivoLejos), Is.False);

        int distanciaFinal =
            Math.Abs(atacanteLejos.Coordenada.X - 4) +
            Math.Abs(atacanteLejos.Coordenada.Y - 4);
        Assert.That(distanciaFinal, Is.LessThanOrEqualTo(1));
    }

    private static Partida CrearPartida(
        out Guerrero atacante,
        out Arquero objetivo)
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


        atacante =
            new Guerrero(
                new Coordenada(1, 1));


        objetivo =
            new Arquero(
                new Coordenada(2, 1));


        humano.AgregarUnidad(atacante);
        maquina.AgregarUnidad(objetivo);


        return new Partida(
            humano,
            maquina);
    }
}
