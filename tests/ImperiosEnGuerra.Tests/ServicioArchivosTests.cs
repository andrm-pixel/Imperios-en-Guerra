using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Persistencia;

namespace ImperiosEnGuerra.Tests.Editor
{
    /// <summary>Pruebas de Servicio Archivos: verifica servicio archivos.</summary>
    public class ServicioArchivosTests
    {
        private string directorioTemporal;

        [SetUp]
        public void Preparar()
        {
            directorioTemporal = Path.Combine(
                Path.GetTempPath(), "ImperiosEnGuerraTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directorioTemporal);
        }

        [TearDown]
        public void Limpiar()
        {
            if (Directory.Exists(directorioTemporal))
            {
                Directory.Delete(directorioTemporal, true);
            }
        }

        // Caso Constructor: verifica rechaza directorio invalido.
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_RechazaDirectorioInvalido(string directorio)
        {
            Assert.Throws<ArgumentException>(() => new ServicioArchivos(directorio));
        }

        // Caso Constructor: verifica crea directorio sin crear archivos.
        [Test]
        public void Constructor_CreaDirectorioSinCrearArchivos()
        {
            string directorio = Ruta("nuevo");
            Assert.That(Directory.Exists(directorio), Is.False);

            new ServicioArchivos(directorio);

            Assert.That(Directory.Exists(directorio), Is.True);
            Assert.That(File.Exists(Path.Combine(directorio, "configuracion.txt")), Is.False);
            Assert.That(File.Exists(Path.Combine(directorio, "log_partida.txt")), Is.False);
            Assert.That(File.Exists(Path.Combine(directorio, "resultado_final.txt")), Is.False);
            Assert.That(Directory.GetFiles(directorio), Is.Empty);
        }

        // Caso Guardar Configuracion: verifica crea archivo y reemplaza contenido.
        [Test]
        public void GuardarConfiguracion_CreaArchivoYReemplazaContenido()
        {
            ServicioArchivos servicio = new ServicioArchivos(directorioTemporal);
            string ruta = Ruta("configuracion.txt");
            string contenido = "Configuración inicial\nSegunda línea";

            servicio.GuardarConfiguracion(contenido);

            Assert.That(File.Exists(ruta), Is.True);
            Assert.That(File.ReadAllText(ruta), Is.EqualTo(contenido));

            servicio.GuardarConfiguracion("Nueva");

            Assert.That(File.ReadAllText(ruta), Is.EqualTo("Nueva"));
        }

        // Caso Guardar Configuracion: verifica rechaza null sin crear archivo.
        [Test]
        public void GuardarConfiguracion_RechazaNullSinCrearArchivo()
        {
            ServicioArchivos servicio = new ServicioArchivos(directorioTemporal);
            Assert.Throws<ArgumentNullException>(() => servicio.GuardarConfiguracion(null));
            Assert.That(File.Exists(Ruta("configuracion.txt")), Is.False);
        }

        // Caso Registrar Evento: verifica crea archivo y conserva eventos en orden.
        [Test]
        public void RegistrarEvento_CreaArchivoYConservaEventosEnOrden()
        {
            ServicioArchivos servicio = new ServicioArchivos(directorioTemporal);
            string ruta = Ruta("log_partida.txt");

            servicio.RegistrarEvento("Primer evento");

            Assert.That(File.Exists(ruta), Is.True);
            Assert.That(File.ReadAllText(ruta), Is.EqualTo("Primer evento" + Environment.NewLine));

            servicio.RegistrarEvento("Segundo evento");

            Assert.That(File.ReadAllText(ruta), Is.EqualTo(
                "Primer evento" + Environment.NewLine + "Segundo evento" + Environment.NewLine));
        }

        // Caso Registrar Evento: verifica rechaza null sin crear archivo.
        [Test]
        public void RegistrarEvento_RechazaNullSinCrearArchivo()
        {
            ServicioArchivos servicio = new ServicioArchivos(directorioTemporal);
            Assert.Throws<ArgumentNullException>(() => servicio.RegistrarEvento(null));
            Assert.That(File.Exists(Ruta("log_partida.txt")), Is.False);
        }

        // Caso Guardar Resultado Final: verifica crea archivo y reemplaza contenido.
        [Test]
        public void GuardarResultadoFinal_CreaArchivoYReemplazaContenido()
        {
            ServicioArchivos servicio = new ServicioArchivos(directorioTemporal);
            string ruta = Ruta("resultado_final.txt");
            string contenido = "Resultado de la partida\nDescripción";

            servicio.GuardarResultadoFinal(contenido);

            Assert.That(File.Exists(ruta), Is.True);
            Assert.That(File.ReadAllText(ruta), Is.EqualTo(contenido));

            servicio.GuardarResultadoFinal("Final");

            Assert.That(File.ReadAllText(ruta), Is.EqualTo("Final"));
        }

        // Caso Guardar Resultado Final: verifica rechaza null sin crear archivo.
        [Test]
        public void GuardarResultadoFinal_RechazaNullSinCrearArchivo()
        {
            ServicioArchivos servicio = new ServicioArchivos(directorioTemporal);
            Assert.Throws<ArgumentNullException>(() => servicio.GuardarResultadoFinal(null));
            Assert.That(File.Exists(Ruta("resultado_final.txt")), Is.False);
        }

        // Caso Guardar Configuracion Inicial: verifica rechaza partida null.
        [Test]
        public void GuardarConfiguracionInicial_RechazaPartidaNull()
        {
            ServicioArchivos servicio = new ServicioArchivos(directorioTemporal);
            Assert.Throws<ArgumentNullException>(() => servicio.GuardarConfiguracionInicial(null));
            Assert.That(File.Exists(Ruta("configuracion.txt")), Is.False);
        }

        // Caso Guardar Configuracion Inicial: verifica incluye ambos jugadores y sobrescribe.
        [Test]
        public void GuardarConfiguracionInicial_IncluyeAmbosJugadoresYSobrescribe()
        {
            ServicioArchivos servicio = new ServicioArchivos(directorioTemporal);
            Partida partida = CrearPartidaValida();
            string ruta = Ruta("configuracion.txt");

            servicio.GuardarConfiguracionInicial(partida);

            Assert.That(File.Exists(ruta), Is.True);
            string contenido = File.ReadAllText(ruta);
            Assert.That(contenido, Does.StartWith("PARTIDA\n"));
            int inicioHumano = contenido.IndexOf("[JUGADOR_HUMANO]", StringComparison.Ordinal);
            int inicioMaquina = contenido.IndexOf("[JUGADOR_MAQUINA]", StringComparison.Ordinal);
            Assert.That(inicioHumano, Is.GreaterThanOrEqualTo(0));
            Assert.That(inicioMaquina, Is.GreaterThan(inicioHumano));

            string humano = contenido.Substring(inicioHumano, inicioMaquina - inicioHumano);
            string maquina = contenido.Substring(inicioMaquina);
            VerificarLineas(humano,
                "Nombre=Ana", "Tipo=Humano", "Mapa=5x4",
                "Oro=11", "Madera=32", "Comida=43", "Edificios:",
                "Castillo=(0,0)", "Unidades:",
                "Aldeano=(0,1)", "Aldeano=(0,2)", "RecursosMapa:",
                "Oro=(1,0)", "Madera=(2,0)", "Comida=(3,0)");
            VerificarLineas(maquina,
                "Nombre=Rival", "Tipo=Maquina", "Mapa=6x5",
                "Oro=21", "Madera=42", "Comida=53", "Edificios:",
                "Castillo=(5,4)", "Unidades:",
                "Aldeano=(4,4)", "Aldeano=(5,3)", "RecursosMapa:",
                "Oro=(1,1)", "Madera=(2,1)", "Comida=(3,1)");

            servicio.GuardarConfiguracionInicial(partida);

            Assert.That(File.ReadAllText(ruta), Is.EqualTo(contenido));
            Assert.That(File.Exists(Ruta("log_partida.txt")), Is.False);
            Assert.That(File.Exists(Ruta("resultado_final.txt")), Is.False);
        }

        private string Ruta(string nombre)
        {
            return Path.Combine(directorioTemporal, nombre);
        }

        private List<Recurso> CrearRecursosIniciales(int fila)
        {
            return new List<Recurso>
            {
                new Recurso(TipoRecurso.Oro, new Coordenada(1, fila)),
                new Recurso(TipoRecurso.Madera, new Coordenada(2, fila)),
                new Recurso(TipoRecurso.Comida, new Coordenada(3, fila))
            };
        }

        private Partida CrearPartidaValida()
        {
            Partida partida = new InicializadorPartida().Crear(
                "Ana", new Mapa(5, 4), new Coordenada(0, 0), CrearRecursosIniciales(0),
                "Rival", new Mapa(6, 5), new Coordenada(5, 4), CrearRecursosIniciales(1));

            partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Oro, 11);
            partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Madera, 12);
            partida.JugadorHumano.Recursos.Agregar(TipoRecurso.Comida, 13);
            partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Oro, 21);
            partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Madera, 22);
            partida.JugadorMaquina.Recursos.Agregar(TipoRecurso.Comida, 23);
            return partida;
        }

        private void VerificarLineas(string contenido, params string[] esperadas)
        {
            string[] lineas = contenido.Split('\n');
            foreach (string esperada in esperadas)
            {
                Assert.That(lineas, Does.Contain(esperada));
            }
        }
    }
}
