using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de InicializadorPartida: verifica mapa 15x15 jugable.</summary>
public class InicializadorPartidaTests
{
    // Caso MapaQuince: verifica dimensiones de la guia.
    [Test]
    public void MapaQuincePorQuince()
    {
        Assert.That(InicializadorPartida.AnchoMapa, Is.EqualTo(15));
        Assert.That(InicializadorPartida.AltoMapa, Is.EqualTo(15));
    }

    // Caso SinTraslapes: verifica nodos y centros sin colisiones.
    [Test]
    public void NodosYCentrosSinTraslapes()
    {
        var ocupadas = new HashSet<(int, int)>
        {
            (InicializadorPartida.CentroHumano.X, InicializadorPartida.CentroHumano.Y),
            (InicializadorPartida.CentroMaquina.X, InicializadorPartida.CentroMaquina.Y)
        };

        foreach (Recurso recurso in InicializadorPartida.RecursosHumano()
            .Concat(InicializadorPartida.RecursosMaquina()))
        {
            var clave = (recurso.Coordenada.X, recurso.Coordenada.Y);

            Assert.That(
                InicializadorPartida.AnchoMapa > recurso.Coordenada.X &&
                recurso.Coordenada.X >= 0 &&
                InicializadorPartida.AltoMapa > recurso.Coordenada.Y &&
                recurso.Coordenada.Y >= 0,
                "Nodo fuera del mapa: " + recurso.Tipo + " " + clave);

            Assert.That(ocupadas.Add(clave), Is.True, "Traslape en " + clave);
        }
    }

    // Caso TodoAlcanzable: verifica que cada nodo tenga una casilla
    // adyacente con ruta A* real desde la base, sin muros de recursos.
    [Test]
    public void TodoNodoAlcanzableDesdeSuBase()
    {
        var mapa = new Mapa(
            InicializadorPartida.AnchoMapa,
            InicializadorPartida.AltoMapa);

        var partida = new InicializadorPartida().Crear(
            "Jugador",
            mapa,
            InicializadorPartida.CentroHumano,
            new List<Recurso>(InicializadorPartida.RecursosHumano()),
            "CPU",
            mapa,
            InicializadorPartida.CentroMaquina,
            new List<Recurso>(InicializadorPartida.RecursosMaquina()));

        var buscador = new RutaAStar();

        foreach (var lado in new[]
        {
            (partida.JugadorHumano, InicializadorPartida.RecursosHumano()),
            (partida.JugadorMaquina, InicializadorPartida.RecursosMaquina())
        })
        {
            foreach (Unidad aldeano in lado.Item1.Unidades.OfType<Aldeano>())
            {
                foreach (Recurso recurso in lado.Item2)
                {
                    bool alcanzable = false;

                    foreach (var d in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        var candidata = new Coordenada(
                            recurso.Coordenada.X + d.Item1,
                            recurso.Coordenada.Y + d.Item2);

                        if (!mapa.EstaDentroDeLimites(candidata) ||
                            !mapa.PuedeColocar(candidata))
                        {
                            continue;
                        }

                        var ruta = buscador.Buscar(
                            mapa,
                            aldeano.Coordenada,
                            candidata,
                            Bloqueos(partida, aldeano));

                        if (ruta.Encontrada)
                        {
                            alcanzable = true;
                            break;
                        }
                    }

                    Assert.That(
                        alcanzable,
                        Is.True,
                        "Nodo inalcanzable: " + recurso.Tipo +
                        " (" + recurso.Coordenada.X + "," + recurso.Coordenada.Y + ")");
                }
            }
        }
    }

    private static List<Coordenada> Bloqueos(Partida partida, Unidad movil)
    {
        var bloqueos = new List<Coordenada>();

        foreach (Unidad unidad in partida.JugadorHumano.Unidades
            .Concat(partida.JugadorMaquina.Unidades))
        {
            if (!ReferenceEquals(unidad, movil) && unidad.Coordenada != null)
                bloqueos.Add(unidad.Coordenada);
        }

        foreach (var edificio in partida.JugadorHumano.Edificios
            .Concat(partida.JugadorMaquina.Edificios))
        {
            if (edificio.Coordenada != null)
                bloqueos.Add(edificio.Coordenada);
        }

        return bloqueos;
    }
}
