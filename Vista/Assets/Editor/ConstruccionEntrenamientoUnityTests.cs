#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pruebas de construccion y entrenamiento.</summary>
public class ConstruccionEntrenamientoUnityTests
{
    private GameObject raiz;
    private ControladorSeleccion seleccion;
    private ControladorAcciones acciones;
    private ControladorConexionApi conexion;
    private VistaPartida vista;
    /// <summary>HUD observado en la prueba.</summary>
    private VistaHud hud;

    /// <summary>Aldeano usado en la prueba de construccion.</summary>
    private EntidadSeleccionableVista aldeano;
    /// <summary>Edificio usado en la prueba de entrenamiento.</summary>
    private EntidadSeleccionableVista castillo;

    /// <summary>Selector de unidad observado en la prueba.</summary>
    private GameObject selectorEntrenamiento;
    private Text mensaje;

    private const string IdAldeano =
        "33333333-3333-3333-3333-333333333333";

    /// <summary>Crea la escena minima de construccion y entrenamiento.</summary>
    [SetUp]
    public void Preparar()
    {
        raiz = new GameObject(
            "Prueba construccion entrenamiento");

        raiz.SetActive(false);

        vista = raiz.AddComponent<VistaPartida>();
        seleccion = raiz.AddComponent<ControladorSeleccion>();
        acciones = raiz.AddComponent<ControladorAcciones>();
        conexion = raiz.AddComponent<ControladorConexionApi>();
        hud = raiz.AddComponent<VistaHud>();

        var objetoMensaje = new GameObject(
            "Mensaje",
            typeof(RectTransform),
            typeof(Text));

        objetoMensaje.transform.SetParent(raiz.transform);
        mensaje = objetoMensaje.GetComponent<Text>();

        selectorEntrenamiento =
            new GameObject("SelectorEntrenamiento");

        selectorEntrenamiento.transform.SetParent(
            raiz.transform);

        Campo(hud, "mensaje", mensaje);

        Campo(
            hud,
            "selectorEntrenamiento",
            selectorEntrenamiento);

        Campo(seleccion, "vistaPartida", vista);

        Campo(
            acciones,
            "controladorSeleccion",
            seleccion);

        Campo(
            acciones,
            "vistaHud",
            hud);

        Campo(
            acciones,
            "conexionApi",
            conexion);

        Campo(
            conexion,
            "vistaHud",
            hud);

        var objetoAldeano = new GameObject(
            "Aldeano",
            typeof(SpriteRenderer),
            typeof(EntidadSeleccionableVista));

        objetoAldeano.transform.SetParent(
            raiz.transform);

        aldeano =
            objetoAldeano.GetComponent<
                EntidadSeleccionableVista>();

        aldeano.Configurar(
            CategoriaEntidadVisual.Unidad,
            IdAldeano,
            "Aldeano",
            "Humano",
            1,
            1);

        var objetoCentro = new GameObject(
            "Castillo",
            typeof(SpriteRenderer),
            typeof(EntidadSeleccionableVista));

        objetoCentro.transform.SetParent(
            raiz.transform);

        castillo =
            objetoCentro.GetComponent<
                EntidadSeleccionableVista>();

        castillo.Configurar(
            CategoriaEntidadVisual.Edificio,
            "",
            "Castillo",
            "Humano",
            2,
            2);

        selectorEntrenamiento.SetActive(false);
        raiz.SetActive(true);

        Invocar(seleccion, "OnEnable");
        Invocar(acciones, "OnEnable");
    }

    /// <summary>Destruye la escena de prueba.</summary>
    [TearDown]
    public void Limpiar()
    {
        Invocar(acciones, "OnDisable");
        Invocar(seleccion, "OnDisable");

        Object.DestroyImmediate(raiz);
    }

    /// <summary>Verifica que el aldeano prepara la construccion.</summary>
    [Test]
    public void AldeanoHumano_PreparaConstruccion()
    {
        Invocar(
            seleccion,
            "Seleccionar",
            aldeano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Construir");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.True);

        Assert.That(
            LeerCampo(
                acciones,
                "unidadIdPendiente"),
            Is.EqualTo(IdAldeano));

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.EqualTo("Construir"));

        Assert.That(
            mensaje.text,
            Does.Contain("Castillo"));
    }

    /// <summary>Verifica que un no aldeano no construye.</summary>
    [Test]
    public void UnidadNoAldeano_NoPreparaConstruccion()
    {
        aldeano.Configurar(
            CategoriaEntidadVisual.Unidad,
            IdAldeano,
            "Guerrero",
            "Humano",
            1,
            1);

        Invocar(
            seleccion,
            "Seleccionar",
            aldeano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Construir");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);
    }

    /// <summary>Verifica que el centro muestra el selector.</summary>
    [Test]
    public void CastilloHumano_MuestraSelectorEntrenamiento()
    {
        Invocar(
            seleccion,
            "Seleccionar",
            castillo);

        Invocar(
            acciones,
            "PrepararAccion",
            "Entrenar");

        Assert.That(
            selectorEntrenamiento.activeSelf,
            Is.True);

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.EqualTo("Entrenar"));

        Assert.That(
            mensaje.text,
            Does.Contain("tipo de unidad"));
    }

    /// <summary>Verifica que elegir arquero pide destino.</summary>
    [Test]
    public void SeleccionarArquero_PreparaDestino()
    {
        Invocar(
            seleccion,
            "Seleccionar",
            castillo);

        Invocar(
            acciones,
            "PrepararAccion",
            "Entrenar");

        Invocar(
            acciones,
            "SeleccionarTipoUnidad",
            "Arquero");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.True);

        Assert.That(
            selectorEntrenamiento.activeSelf,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "tipoUnidadPendiente"),
            Is.EqualTo("Arquero"));

        Assert.That(
            mensaje.text,
            Is.EqualTo(
                "Selecciona una casilla de referencia para Arquero."));
    }

    /// <summary>Verifica que el centro maquina no entrena.</summary>
    [Test]
    public void CastilloMaquina_NoPreparaEntrenamiento()
    {
        castillo.Configurar(
            CategoriaEntidadVisual.Edificio,
            "",
            "Castillo",
            "Maquina",
            8,
            8);

        Invocar(
            seleccion,
            "Seleccionar",
            castillo);

        Invocar(
            acciones,
            "PrepararAccion",
            "Entrenar");

        Assert.That(
            selectorEntrenamiento.activeSelf,
            Is.False);

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);
    }

    /// <summary>Verifica que el curso no bloquea construir.</summary>
    [Test]
    public void ConstruccionEnCurso_NoBloqueaOtraOrdenDeConstruccion()
    {
        CampoAutomatico(
            conexion,
            "ConstruccionEnCurso",
            true);

        Invocar(
            seleccion,
            "Seleccionar",
            aldeano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Construir");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.True);
    }

    /// <summary>Verifica que el curso no bloquea encolar.</summary>
    [Test]
    public void EntrenamientoEnCurso_NoBloqueaOtraOrdenDeCola()
    {
        CampoAutomatico(
            conexion,
            "EntrenamientoEnCurso",
            true);

        Invocar(
            seleccion,
            "Seleccionar",
            castillo);

        Invocar(
            acciones,
            "PrepararAccion",
            "Entrenar");

        Assert.That(
            selectorEntrenamiento.activeSelf,
            Is.True);
    }

    /// <summary>Verifica que cancelar limpia la construccion.</summary>
    [Test]
    public void CancelarConstruccion_LimpiaIntencion()
    {
        Invocar(
            seleccion,
            "Seleccionar",
            aldeano);

        Invocar(
            acciones,
            "PrepararAccion",
            "Construir");

        Invocar(
            seleccion,
            "CancelarCapturaDestino");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);

        Assert.That(
            mensaje.text,
            Is.EqualTo("Construcción cancelada."));
    }

    /// <summary>Verifica el JSON del contrato de construccion.</summary>
    [Test]
    public void DtoConstruccion_UsaContratoEsperado()
    {
        var dto = new ConstruirDto
        {
            aldeanoId = IdAldeano,
            tipoEdificio = "Castillo",
            destino = new CoordenadaDto(4, 5)
        };

        string json = JsonUtility.ToJson(dto);

        Assert.That(
            json,
            Is.EqualTo(
                "{\"aldeanoId\":\"" +
                IdAldeano +
                "\",\"tipoEdificio\":\"Castillo\"," +
                "\"destino\":{\"x\":4,\"y\":5}}"));
    }

    /// <summary>Verifica los DTOs concurrentes de construccion.</summary>
    [Test]
    public void DtoConcurrente_ConstruccionConservaContrato()
    {
        var inicio = new ProcesoIniciadoDto
        {
            procesoId =
                "cccccccc-cccc-cccc-cccc-cccccccccccc",
            nombre = "CONSTRUIR",
            estado = "iniciado"
        };

        string json = JsonUtility.ToJson(inicio);

        Assert.That(
            json,
            Does.Contain("\"nombre\":\"CONSTRUIR\""));

        var resultado = new ResultadoProcesoDto
        {
            procesoId = inicio.procesoId,
            nombre = "CONSTRUIR",
            estado = "Completado",
            hiloTrabajoId = 11,
            exito = true,
            mensaje = "Construcción realizada correctamente."
        };

        string resultadoJson =
            JsonUtility.ToJson(resultado);

        Assert.That(
            resultadoJson,
            Does.Contain("\"estado\":\"Completado\""));

        Assert.That(
            resultadoJson,
            Does.Contain("\"hiloTrabajoId\":11"));
    }

    /// <summary>Verifica el JSON del contrato de entrenamiento.</summary>
    [Test]
    public void DtoEntrenamiento_UsaContratoEsperado()
    {
        var dto = new EntrenarDto
        {
            edificioOrigen = new CoordenadaDto(2, 2),
            tipoUnidad = "Guerrero",
            destino = new CoordenadaDto(3, 2)
        };

        string json = JsonUtility.ToJson(dto);

        Assert.That(
            json,
            Is.EqualTo(
                "{\"edificioOrigen\":{\"x\":2,\"y\":2}," +
                "\"tipoUnidad\":\"Guerrero\"," +
                "\"destino\":{\"x\":3,\"y\":2}}"));
    }

    /// <summary>Verifica los DTOs concurrentes de entrenamiento.</summary>
    [Test]
    public void DtoConcurrente_EntrenamientoConservaContrato()
    {
        var inicio = new ProcesoIniciadoDto
        {
            procesoId =
                "dddddddd-dddd-dddd-dddd-dddddddddddd",
            nombre = "ENTRENAR",
            estado = "iniciado"
        };

        string json = JsonUtility.ToJson(inicio);

        Assert.That(
            json,
            Does.Contain("\"nombre\":\"ENTRENAR\""));

        var resultado = new ResultadoProcesoDto
        {
            procesoId = inicio.procesoId,
            nombre = "ENTRENAR",
            estado = "Completado",
            hiloTrabajoId = 13,
            exito = true,
            mensaje = "Unidad Guerrero entrenada correctamente."
        };

        string resultadoJson =
            JsonUtility.ToJson(resultado);

        Assert.That(
            resultadoJson,
            Does.Contain("\"estado\":\"Completado\""));

        Assert.That(
            resultadoJson,
            Does.Contain("\"hiloTrabajoId\":13"));
    }

    private static object LeerCampo(
        object objeto,
        string nombre)
    {
        return objeto.GetType()
            .GetField(
                nombre,
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .GetValue(objeto);
    }

    private static void Campo(
        object objeto,
        string nombre,
        object valor)
    {
        objeto.GetType()
            .GetField(
                nombre,
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .SetValue(objeto, valor);
    }

    private static void CampoAutomatico(
        object objeto,
        string nombre,
        object valor)
    {
        objeto.GetType()
            .GetField(
                $"<{nombre}>k__BackingField",
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .SetValue(objeto, valor);
    }

    private static void Invocar(
        object objeto,
        string nombre,
        params object[] argumentos)
    {
        objeto.GetType()
            .GetMethod(
                nombre,
                BindingFlags.Instance |
                BindingFlags.NonPublic)
            .Invoke(objeto, argumentos);
    }
}
#endif
