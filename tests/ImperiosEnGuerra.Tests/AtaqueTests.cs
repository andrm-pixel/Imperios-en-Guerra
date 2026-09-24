using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de Ataque: verifica ataque.</summary>
public class AtaqueTests
{
    private Partida partida;
    private Soldado atacante;
    private Arquero objetivo;
    private OperacionAtaque operacion;

    [SetUp]
    public void Preparar()
    {
        var mapa = new Mapa(6, 6);

        partida = new Partida(
            new Jugador(
                "Humano",
                TipoJugador.Humano,
                mapa,
                new RecursosJugador()),
            new Jugador(
                "Máquina",
                TipoJugador.Maquina,
                mapa,
                new RecursosJugador()));

        atacante = new Soldado(new Coordenada(1, 1));
        objetivo = new Arquero(new Coordenada(2, 1));

        partida.JugadorHumano.AgregarUnidad(atacante);
        partida.JugadorMaquina.AgregarUnidad(objetivo);

        operacion = new OperacionAtaque();
    }

    // Caso Ataque Valido: verifica aplica dano real.
    [Test]
    public void AtaqueValido_AplicaDanoReal()
    {
        int unidadesHumano = partida.JugadorHumano.Unidades.Count;
        int unidadesMaquina = partida.JugadorMaquina.Unidades.Count;

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                objetivo.Id));

        Assert.That(resultado.Exito, Is.True);
        Assert.That(resultado.Mensaje, Does.Contain("Impacto"));
        Assert.That(partida.JugadorHumano.Unidades.Count, Is.EqualTo(unidadesHumano));
        Assert.That(partida.JugadorMaquina.Unidades.Count, Is.EqualTo(unidadesMaquina));
        Assert.That(objetivo.Vida, Is.EqualTo(65));
    }

    // Caso Objetivo Fuera De Alcance: verifica falla.
    [Test]
    public void ObjetivoFueraDeAlcance_Falla()
    {
        var lejano = new Arquero(new Coordenada(4, 4));
        partida.JugadorMaquina.AgregarUnidad(lejano);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                lejano.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("alcance"));
        Assert.That(lejano.Vida, Is.EqualTo(90));
    }

    // Caso Cuatro Impactos: verifica destruyen arquero y declaran victoria.
    [Test]
    public void CuatroImpactos_DestruyenArqueroYDeclaranVictoria()
    {
        ResultadoAccion ultimo = null;

        for (int i = 0; i < 4; i++)
        {
            ultimo = operacion.Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    objetivo.Id));

            Assert.That(ultimo.Exito, Is.True);
        }

        Assert.That(partida.JugadorMaquina.Unidades.Count, Is.EqualTo(0));
        Assert.That(ultimo.Mensaje, Does.Contain("¡Victoria!"));
    }

    // Caso Atacante Maquina: verifica no es controlable.
    [Test]
    public void AtacanteMaquina_NoEsControlable()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                objetivo.Id,
                atacante.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("máquina"));
    }

    // Caso Aldeano: verifica no puede atacar.
    [Test]
    public void Aldeano_NoPuedeAtacar()
    {
        var aldeano = new Aldeano(new Coordenada(2, 2));
        partida.JugadorHumano.AgregarUnidad(aldeano);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                aldeano.Id,
                objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("militar"));
    }

    // Caso Atacante No Disponible: verifica falla.
    [Test]
    public void AtacanteNoDisponible_Falla()
    {
        atacante.MarcarNoDisponible();

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("disponible"));
    }

    // Caso Ataque A Edificio: verifica aplica dano.
    [Test]
    public void AtaqueAEdificio_AplicaDano()
    {
        var centroEnemigo = new Castillo(new Coordenada(1, 2));
        partida.JugadorMaquina.AgregarEdificio(centroEnemigo);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                centroEnemigo.Id));

        Assert.That(resultado.Exito, Is.True);
        Assert.That(resultado.Mensaje, Does.Contain("Impacto"));
        Assert.That(centroEnemigo.Vida, Is.EqualTo(475));
        Assert.That(
            partida.JugadorMaquina.Edificios.Contains(centroEnemigo),
            Is.True);
    }

    // Caso Destruir Centro: verifica declara victoria.
    [Test]
    public void DestruirCentro_SoloNoDaVictoriaFaltaEjercito()
    {
        var centroEnemigo = new Castillo(new Coordenada(1, 2));
        partida.JugadorMaquina.AgregarEdificio(centroEnemigo);

        ResultadoAccion ultimo = null;

        for (int i = 0; i < 20; i++)
        {
            ultimo = operacion.Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    centroEnemigo.Id));

            Assert.That(ultimo.Exito, Is.True);
        }

        Assert.That(
            partida.JugadorMaquina.Edificios.Contains(centroEnemigo),
            Is.False);

        // Sin centro pero con ejercito vivo: aun no hay victoria total.
        Assert.That(ultimo.Mensaje, Does.Not.Contain("¡Victoria!"));

        // Al eliminar tambien la ultima unidad si hay victoria total.
        for (int i = 0; i < 4; i++)
        {
            ultimo = operacion.Ejecutar(
                partida,
                new SolicitudAtaque(
                    atacante.Id,
                    objetivo.Id));

            Assert.That(ultimo.Exito, Is.True);
        }

        Assert.That(ultimo.Mensaje, Does.Contain("¡Victoria!"));
    }

    // Caso Edificio Propio: verifica falla.
    [Test]
    public void EdificioPropio_Falla()
    {
        var centroPropio = new Castillo(new Coordenada(0, 0));
        partida.JugadorHumano.AgregarEdificio(centroPropio);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                centroPropio.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("humano"));
    }

    // Caso Objetivo Propio: verifica falla y conserva mensaje.
    [Test]
    public void ObjetivoPropio_FallaYConservaMensaje()
    {
        var aliado = new Arquero(new Coordenada(2, 2));
        partida.JugadorHumano.AgregarUnidad(aliado);

        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                aliado.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Is.EqualTo("El objetivo pertenece al jugador humano."));
    }

    // Caso Atacante Inexistente: verifica falla.
    [Test]
    public void AtacanteInexistente_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                Guid.NewGuid(),
                objetivo.Id));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("atacante"));
    }

    // Caso Objetivo Inexistente: verifica falla.
    [Test]
    public void ObjetivoInexistente_Falla()
    {
        ResultadoAccion resultado = operacion.Ejecutar(
            partida,
            new SolicitudAtaque(
                atacante.Id,
                Guid.NewGuid()));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("objetivo"));
    }

    // Caso Servicio: verifica ataque valido.
    [Test]
    public void Servicio_AtaqueValido()
    {
        var servicio = CrearServicio();

        ResultadoAccion resultado = servicio.Atacar(
            new AtacarRequest
            {
                AtacanteId = atacante.Id.ToString("D"),
                ObjetivoId = objetivo.Id.ToString("D")
            });

        Assert.That(resultado.Exito, Is.True);
        Assert.That(servicio.ObtenerEstado(), Is.Not.Null);
    }

    // Caso Servicio: verifica ids invalidos - devuelven mensaje claro.
    [Test]
    public void Servicio_IdsInvalidos_DevuelvenMensajeClaro()
    {
        var servicio = CrearServicio();

        ResultadoAccion atacanteInvalido = servicio.Atacar(
            new AtacarRequest
            {
                AtacanteId = "no-es-guid",
                ObjetivoId = objetivo.Id.ToString("D")
            });

        ResultadoAccion objetivoInvalido = servicio.Atacar(
            new AtacarRequest
            {
                AtacanteId = atacante.Id.ToString("D"),
                ObjetivoId = "no-es-guid"
            });

        Assert.That(atacanteInvalido.Exito, Is.False);
        Assert.That(atacanteInvalido.Mensaje, Does.Contain("atacante"));
        Assert.That(objetivoInvalido.Exito, Is.False);
        Assert.That(objetivoInvalido.Mensaje, Does.Contain("objetivo"));
    }

    // Caso Sin Partida O Solicitud: verifica devuelve fallo.
    [Test]
    public void SinPartidaOSolicitud_DevuelveFallo()
    {
        Assert.That(
            operacion.Ejecutar(
                null,
                new SolicitudAtaque(atacante.Id, objetivo.Id)).Exito,
            Is.False);

        Assert.That(
            operacion.Ejecutar(partida, null).Exito,
            Is.False);

        Assert.That(
            new EstadoPartidaService().Atacar(
                new AtacarRequest
                {
                    AtacanteId = atacante.Id.ToString("D"),
                    ObjetivoId = objetivo.Id.ToString("D")
                }).Mensaje,
            Is.EqualTo("No hay una partida activa."));
    }

    private EstadoPartidaService CrearServicio()
    {
        var servicio = new EstadoPartidaService();
        servicio.EstablecerPartida(partida);
        return servicio;
    }
}
