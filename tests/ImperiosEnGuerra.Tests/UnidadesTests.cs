using System;
using NUnit.Framework;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Tests.Editor
{
    /// <summary>Pruebas de Unidades: verifica unidades.</summary>
    public class UnidadesTests
    {
        // Caso Guerrero: verifica es soldado y conserva coordenada.
        [Test]
        public void Guerrero_EsSoldadoYConservaCoordenada()
        {
            Coordenada coordenada = new Coordenada(1, 2);

            Guerrero guerrero = new Guerrero(coordenada);

            Assert.That(guerrero, Is.InstanceOf<Soldado>());
            Assert.That(guerrero.Coordenada, Is.SameAs(coordenada));
            Assert.That(guerrero.Disponible, Is.True);
        }

        // Caso Arquero: verifica es soldado y conserva coordenada.
        [Test]
        public void Arquero_EsSoldadoYConservaCoordenada()
        {
            Coordenada coordenada = new Coordenada(2, 3);

            Arquero arquero = new Arquero(coordenada);

            Assert.That(arquero, Is.InstanceOf<Soldado>());
            Assert.That(arquero.Coordenada, Is.SameAs(coordenada));
            Assert.That(arquero.Disponible, Is.True);
        }

        // Caso Arquero: verifica continua siendo soldado.
        [Test]
        public void Arquero_ContinuaSiendoSoldado()
        {
            Coordenada coordenada = new Coordenada(3, 4);

            Arquero arquero = new Arquero(coordenada);

            Assert.That(arquero, Is.InstanceOf<Soldado>());
            Assert.That(arquero.Coordenada, Is.SameAs(coordenada));
            Assert.That(arquero.Disponible, Is.True);
        }

        // Caso Guerrero: verifica tiene vida y ataque de combate.
        [Test]
        public void Guerrero_TieneVidaYAtaqueDeCombate()
        {
            Coordenada coordenada = new Coordenada(4, 5);

            Guerrero guerrero = new Guerrero(coordenada);

            Assert.That(guerrero, Is.InstanceOf<Unidad>());
            Assert.That(guerrero, Is.InstanceOf<Soldado>());
            Assert.That(guerrero.Vida, Is.GreaterThan(0));
            Assert.That(guerrero.PuntosAtaque, Is.GreaterThan(0));
            Assert.That(guerrero.AlcanceAtaque, Is.GreaterThanOrEqualTo(1));
        }

        // Caso Velocidades Movimiento: verifica son positivas.
        [Test]
        public void VelocidadesMovimiento_SonPositivas()
        {
            Unidad[] unidades =
            {
                new Aldeano(new Coordenada(0, 0)),
                new Guerrero(new Coordenada(0, 0)),
                new Arquero(new Coordenada(0, 0))
            };

            foreach (Unidad unidad in unidades)
            {
                Assert.That(
                    unidad.VelocidadMovimiento,
                    Is.GreaterThan(0d));
            }
        }

        // Caso Velocidad Movimiento: verifica puede variar segun tipo.
        [Test]
        public void VelocidadMovimiento_PuedeVariarSegunTipo()
        {
            var aldeano =
                new Aldeano(new Coordenada(0, 0));

            var arquero =
                new Arquero(new Coordenada(0, 0));

            var guerrero =
                new Guerrero(new Coordenada(0, 0));

            Assert.That(
                arquero.VelocidadMovimiento,
                Is.GreaterThan(
                    aldeano.VelocidadMovimiento));

            Assert.That(
                aldeano.VelocidadMovimiento,
                Is.EqualTo(
                    guerrero.VelocidadMovimiento));
        }

        // Caso Aldeano: verifica inicia sin carga y con capacidad.
        [Test]
        public void Aldeano_IniciaSinCargaYConCapacidad()
        {
            var aldeano =
                new Aldeano(
                    new Coordenada(1, 1));

            Assert.That(
                aldeano.CapacidadCarga,
                Is.EqualTo(
                    Aldeano.CapacidadCargaPredeterminada));

            Assert.That(
                aldeano.CargaActual,
                Is.Zero);

            Assert.That(
                aldeano.TipoCarga,
                Is.Null);
        }

        // Caso Aldeano: verifica recolectar desde - respeta capacidad.
        [Test]
        public void Aldeano_RecolectarDesde_RespetaCapacidad()
        {
            var aldeano =
                new Aldeano(
                    new Coordenada(1, 1),
                    4);

            var recurso =
                new Recurso(
                    TipoRecurso.Oro,
                    new Coordenada(2, 1),
                    10);

            Assert.That(
                aldeano.RecolectarDesde(
                    recurso,
                    10),
                Is.EqualTo(4));

            Assert.That(
                aldeano.CargaActual,
                Is.EqualTo(4));

            Assert.That(
                aldeano.TipoCarga,
                Is.EqualTo(
                    TipoRecurso.Oro));

            Assert.That(
                recurso.CantidadRestante,
                Is.EqualTo(6));
        }

        // Caso Aldeano: verifica no mezcla tipos en la misma carga.
        [Test]
        public void Aldeano_NoMezclaTiposEnLaMismaCarga()
        {
            var aldeano =
                new Aldeano(
                    new Coordenada(1, 1),
                    10);

            var oro =
                new Recurso(
                    TipoRecurso.Oro,
                    new Coordenada(2, 1),
                    10);

            var madera =
                new Recurso(
                    TipoRecurso.Madera,
                    new Coordenada(3, 1),
                    10);

            Assert.That(
                aldeano.RecolectarDesde(
                    oro,
                    3),
                Is.EqualTo(3));

            Assert.That(
                aldeano.RecolectarDesde(
                    madera,
                    3),
                Is.Zero);

            Assert.That(
                aldeano.CargaActual,
                Is.EqualTo(3));

            Assert.That(
                madera.CantidadRestante,
                Is.EqualTo(10));
        }

        // Caso Aldeano: verifica vaciar carga - devuelve cantidad y tipo.
        [Test]
        public void Aldeano_VaciarCarga_DevuelveCantidadYTipo()
        {
            var aldeano =
                new Aldeano(
                    new Coordenada(1, 1),
                    10);

            var comida =
                new Recurso(
                    TipoRecurso.Comida,
                    new Coordenada(2, 1),
                    10);

            aldeano.RecolectarDesde(
                comida,
                6);

            int cantidad =
                aldeano.VaciarCarga(
                    out TipoRecurso? tipo);

            Assert.That(
                cantidad,
                Is.EqualTo(6));

            Assert.That(
                tipo,
                Is.EqualTo(
                    TipoRecurso.Comida));

            Assert.That(
                aldeano.CargaActual,
                Is.Zero);

            Assert.That(
                aldeano.TipoCarga,
                Is.Null);
        }

        // Caso Unidades Distintas: verifica tienen ids distintos y no vacios.
        [Test]
        public void UnidadesDistintas_TienenIdsDistintosYNoVacios()
        {
            var primera = new Aldeano(new Coordenada(1, 1));
            var segunda = new Aldeano(new Coordenada(2, 2));

            Assert.That(primera.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(segunda.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(primera.Id, Is.Not.EqualTo(segunda.Id));
        }

        // Caso Id: verifica permanece al cambiar coordenada.
        [Test]
        public void Id_PermaneceAlCambiarCoordenada()
        {
            var unidad = new UnidadPrueba(new Coordenada(1, 1));
            Guid idOriginal = unidad.Id;

            unidad.CambiarCoordenada(new Coordenada(4, 5));

            Assert.That(unidad.Id, Is.EqualTo(idOriginal));
            Assert.That(unidad.Coordenada.X, Is.EqualTo(4));
            Assert.That(unidad.Coordenada.Y, Is.EqualTo(5));
        }

        private sealed class UnidadPrueba : Unidad
        {
            public UnidadPrueba(Coordenada coordenada) : base(coordenada)
            {
            }

            public void CambiarCoordenada(Coordenada coordenada)
            {
                Coordenada = coordenada;
            }
        }
    }
}
