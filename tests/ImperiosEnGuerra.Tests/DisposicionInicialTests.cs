using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de DisposicionInicial: verifica mapa 15x15 jugable.</summary>
public class DisposicionInicialTests
{
    // Caso MapaQuince: verifica dimensiones de la guía.
    [Test]
    public void MapaQuincePorQuince()
    {
        Assert.That(DisposicionInicial.AnchoMapa, Is.EqualTo(15));
        Assert.That(DisposicionInicial.AltoMapa, Is.EqualTo(15));
    }

    // Caso SinTraslapes: verifica nodos y centros sin colisiones.
    [Test]
    public void NodosYCentrosSinTraslapes()
    {
        var ocupadas = new HashSet<(int, int)>
        {
            (DisposicionInicial.CentroHumano.X, DisposicionInicial.CentroHumano.Y),
            (DisposicionInicial.CentroMaquina.X, DisposicionInicial.CentroMaquina.Y)
        };

        foreach (Recurso recurso in DisposicionInicial.RecursosHumano()
            .Concat(DisposicionInicial.RecursosMaquina()))
        {
            var clave = (recurso.Coordenada.X, recurso.Coordenada.Y);

            Assert.That(
                DisposicionInicial.AnchoMapa > recurso.Coordenada.X &&
                recurso.Coordenada.X >= 0 &&
                DisposicionInicial.AltoMapa > recurso.Coordenada.Y &&
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
            DisposicionInicial.AnchoMapa,
            DisposicionInicial.AltoMapa);

        var partida = new InicializadorPartida().Crear(
            "Jugador",
            mapa,
            DisposicionInicial.CentroHumano,
            new List<Recurso>(DisposicionInicial.RecursosHumano()),
            "CPU",
            mapa,
            DisposicionInicial.CentroMaquina,
            new List<Recurso>(DisposicionInicial.RecursosMaquina()));

        var buscador = new BuscadorRutaAStar();

        foreach (var lado in new[]
        {
            (partida.JugadorHumano, DisposicionInicial.RecursosHumano()),
            (partida.JugadorMaquina, DisposicionInicial.RecursosMaquina())
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
