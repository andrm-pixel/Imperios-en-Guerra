using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de Entrenamiento: verifica entrenamiento.</summary>
public class EntrenamientoTests
{
    private Partida partida;
    private Mapa mapa;
    private CentroUrbano centroHumano;
    private CentroUrbano centroMaquina;
    private OperacionEntrenamiento operacion;

    [SetUp]
    public void Preparar()
    {
        mapa = new Mapa(6, 6);

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

        centroHumano =
            new CentroUrbano(new Coordenada(1, 1));

        centroMaquina =
            new CentroUrbano(new Coordenada(4, 4));

        partida.JugadorHumano
            .AgregarEdificio(centroHumano);

        partida.JugadorMaquina
            .AgregarEdificio(centroMaquina);

        mapa.ObtenerCasilla(1, 1).Ocupar();
        mapa.ObtenerCasilla(4, 4).Ocupar();

        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Oro, 100);
        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Madera, 100);
        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Comida, 100);
        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Piedra, 100);
        partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Hierro, 100);

        operacion = new OperacionEntrenamiento();
    }

    // Caso Entrenamiento Valido: verifica crea unidad y ocupa casilla.
    [Test]
    public void EntrenamientoValido_CreaUnidadYOcupaCasilla()
    {
        int unidadesIniciales =
            partida.JugadorHumano.Unidades.Count;

        var solicitud =
            new SolicitudEntrenamiento(
                new Coordenada(1, 1),
                "Guerrero",
                new Coordenada(2, 2));

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                solicitud);

        Assert.That(resultado.Exito, Is.True);

        Assert.That(
            partida.JugadorHumano.Unidades.Count,
            Is.EqualTo(unidadesIniciales + 1));

        Assert.That(
            partida.JugadorHumano.Unidades[^1],
            Is.TypeOf<Guerrero>());

        Assert.That(
            mapa.ObtenerCasilla(2, 2).EstaOcupada,
            Is.True);
    }

    // Caso Edificio Maquina: verifica falla.
    [Test]
    public void EdificioMaquina_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(4, 4),
                    "Guerrero",
                    new Coordenada(2, 2)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("máquina"));
    }

    // Caso Edificio Humano Inexistente: verifica falla.
    [Test]
    public void EdificioHumanoInexistente_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(3, 3),
                    "Guerrero",
                    new Coordenada(2, 2)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("edificio humano"));
    }

    // Caso Tipo Unidad No Permitido: verifica falla.
    [Test]
    public void TipoUnidadNoPermitido_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(1, 1),
                    "Dragon",
                    new Coordenada(2, 2)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("permitido"));
    }

    // Caso Aparicion Fuera Del Mapa: verifica falla.
    [Test]
    public void AparicionFueraDelMapa_Falla()
    {
        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(1, 1),
                    "Arquero",
                    new Coordenada(8, 8)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("fuera"));
    }

    // Caso Aparicion Ocupada: verifica falla.
    [Test]
    public void AparicionOcupada_Falla()
    {
        mapa.ObtenerCasilla(2, 2).Ocupar();

        ResultadoAccion resultado =
            operacion.Ejecutar(
                partida,
                new SolicitudEntrenamiento(
                    new Coordenada(1, 1),
                    "Lancero",
                    new Coordenada(2, 2)));

        Assert.That(resultado.Exito, Is.False);
        Assert.That(
            resultado.Mensaje,
            Does.Contain("disponible"));
    }

    // Caso Servicio: verifica entrenamiento valido.
    [Test]
    public void Servicio_EntrenamientoValido()
    {
        var servicio =
            new EstadoPartidaService();

        servicio.EstablecerPartida(partida);

        var request = new EntrenarRequest
        {
            EdificioOrigen = new CoordenadaRequest
            {
                X = 1,
                Y = 1
            },

            TipoUnidad = "Arquero",

            Destino = new CoordenadaRequest
            {
                X = 2,
                Y = 2
            }
        };

        ResultadoAccion resultado =
            servicio.Entrenar(request);

        Assert.That(resultado.Exito, Is.True);

        Assert.That(
            partida.JugadorHumano.Unidades[^1],
            Is.TypeOf<Arquero>());
    }

    // Caso Servicio: verifica solicitud nula - falla.
    [Test]
    public void Servicio_SolicitudNula_Falla()
    {
        var servicio =
            new EstadoPartidaService();

        servicio.EstablecerPartida(partida);

        ResultadoAccion resultado =
            servicio.Entrenar(null);

        Assert.That(resultado.Exito, Is.False);
    }
}
