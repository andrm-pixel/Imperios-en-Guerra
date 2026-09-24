#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pruebas de preparacion y validacion del ataque.</summary>
public class AtaqueUnityTests
{
    /// <summary>Raiz de los objetos de prueba de ataque.</summary>
    private GameObject raiz;
    /// <summary>Seleccion usada en la prueba de ataque.</summary>
    private ControladorSeleccion seleccion;
    /// <summary>Acciones bajo prueba de ataque.</summary>
    private ControladorAcciones acciones;
    /// <summary>Conexion observada en la prueba de ataque.</summary>
    private ControladorConexionApi conexion;
    /// <summary>Unidad atacante de la prueba.</summary>
    private EntidadSeleccionableVista atacante;
    /// <summary>Objetivo enemigo valido de la prueba.</summary>
    private EntidadSeleccionableVista objetivoEnemigo;
    /// <summary>Objetivo propio invalido de la prueba.</summary>
    private EntidadSeleccionableVista objetivoPropio;
    /// <summary>Texto del HUD observado en la prueba.</summary>
    private Text mensaje;

    private const string IdAtacante =
        "44444444-4444-4444-4444-444444444444";

    private const string IdObjetivo =
        "55555555-5555-5555-5555-555555555555";

    /// <summary>Crea la escena minima para probar el ataque.</summary>
    [SetUp]
    public void Preparar()
    {
        raiz = new GameObject("Prueba ataque");
        raiz.SetActive(false);

        var vista = raiz.AddComponent<VistaPartida>();
        seleccion = raiz.AddComponent<ControladorSeleccion>();
        acciones = raiz.AddComponent<ControladorAcciones>();
        conexion = raiz.AddComponent<ControladorConexionApi>();
        var hud = raiz.AddComponent<VistaHud>();

        var texto = new GameObject(
            "Mensaje",
            typeof(RectTransform),
            typeof(Text));

        texto.transform.SetParent(raiz.transform);
        mensaje = texto.GetComponent<Text>();

        Campo(hud, "mensaje", mensaje);
        Campo(seleccion, "vistaPartida", vista);
        Campo(acciones, "controladorSeleccion", seleccion);
        Campo(acciones, "vistaHud", hud);
        Campo(acciones, "conexionApi", conexion);
        Campo(conexion, "vistaHud", hud);

        atacante = CrearEntidad(
            "Atacante",
            IdAtacante,
            "Guerrero",
            "Humano",
            1,
            1);

        objetivoEnemigo = CrearEntidad(
            "ObjetivoMaquina",
            IdObjetivo,
            "Arquero",
            "Maquina",
            4,
            4);

        objetivoPropio = CrearEntidad(
            "ObjetivoHumano",
            "66666666-6666-6666-6666-666666666666",
            "Arquero",
            "Humano",
            2,
            2);

        raiz.SetActive(true);

        Invocar(seleccion, "OnEnable");
        Invocar(acciones, "OnEnable");
        Invocar(seleccion, "Seleccionar", atacante);
    }

    /// <summary>Destruye la escena de prueba de ataque.</summary>
    [TearDown]
    public void Limpiar()
    {
        Invocar(acciones, "OnDisable");
        Invocar(seleccion, "OnDisable");
        Object.DestroyImmediate(raiz);
    }

    /// <summary>Verifica que un militar prepara el ataque.</summary>
    [Test]
    public void UnidadMilitarHumana_PreparaAtaque()
    {
        Invocar(
            acciones,
            "PrepararAccion",
            "Atacar");

        Assert.That(
            seleccion.CapturandoObjetivoEntidad,
            Is.True);

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "unidadIdPendiente"),
            Is.EqualTo(IdAtacante));

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.EqualTo("Atacar"));

        Assert.That(
            mensaje.text,
            Is.EqualTo(
                "Selecciona una unidad enemiga como objetivo."));
    }

    /// <summary>Verifica que un aldeano no prepara ataque.</summary>
    [Test]
    public void AldeanoHumano_NoPreparaAtaque()
    {
        atacante.Configurar(
            CategoriaEntidadVisual.Unidad,
            IdAtacante,
            "Aldeano",
            "Humano",
            1,
            1);

        Invocar(
            acciones,
            "PrepararAccion",
            "Atacar");

        Assert.That(
            seleccion.CapturandoObjetivoEntidad,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);
    }

    /// <summary>Verifica que el enemigo es objetivo valido.</summary>
    [Test]
    public void ObjetivoMaquina_EsValido()
    {
        Assert.That(
            EsObjetivoValido(objetivoEnemigo),
            Is.True);
    }

    /// <summary>Verifica que lo propio no es objetivo.</summary>
    [Test]
    public void ObjetivoPropio_NoEsValido()
    {
        Assert.That(
            EsObjetivoValido(objetivoPropio),
            Is.False);
    }

    /// <summary>Verifica que recurso y nulo no son objetivos.</summary>
    [Test]
    public void RecursoONulo_NoSonObjetivosValidos()
    {
        objetivoEnemigo.Configurar(
            CategoriaEntidadVisual.Recurso,
            "",
            "Oro",
            "",
            4,
            4);

        Assert.That(
            EsObjetivoValido(objetivoEnemigo),
            Is.False);

        Assert.That(
            EsObjetivoValido(null),
            Is.False);
    }

    /// <summary>Verifica que cancelar limpia la intencion.</summary>
    [Test]
    public void CancelarAtaque_LimpiaIntencion()
    {
        Invocar(
            acciones,
            "PrepararAccion",
            "Atacar");

        Invocar(
            seleccion,
            "CancelarCapturaDestino");

        Assert.That(
            seleccion.CapturandoObjetivoEntidad,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);

        Assert.That(
            mensaje.text,
            Is.EqualTo("Ataque cancelado."));
    }

    /// <summary>Verifica que cambiar seleccion cancela el ataque.</summary>
    [Test]
    public void CambioSeleccion_CancelaAtaqueSinRomperHud()
    {
        Invocar(
            acciones,
            "PrepararAccion",
            "Atacar");

        seleccion.LimpiarSeleccion();

        Assert.That(
            seleccion.CapturandoObjetivoEntidad,
            Is.False);

        Assert.That(
            LeerCampo(
                acciones,
                "accionPendiente"),
            Is.Null);

        Assert.That(
            mensaje.text,
            Is.EqualTo("Ataque cancelado."));
    }

    /// <summary>Verifica el JSON del contrato de ataque.</summary>
    [Test]
    public void DtoAtaque_UsaIdsEstables()
    {
        var dto = new AtaqueDto
        {
            atacanteId = IdAtacante,
            objetivoId = IdObjetivo
        };

        string json = JsonUtility.ToJson(dto);

        Assert.That(
            json,
            Is.EqualTo(
                "{\"atacanteId\":\"" +
                IdAtacante +
                "\",\"objetivoId\":\"" +
                IdObjetivo +
                "\"}"));
    }

    /// <summary>Crea una entidad visual para la prueba.</summary>
    private EntidadSeleccionableVista CrearEntidad(
        string nombre,
        string id,
        string tipo,
        string propietario,
        int x,
        int y)
    {
        var objeto = new GameObject(
            nombre,
            typeof(SpriteRenderer),
            typeof(EntidadSeleccionableVista));

        objeto.transform.SetParent(raiz.transform);

        var entidad =
            objeto.GetComponent<EntidadSeleccionableVista>();

        entidad.Configurar(
            CategoriaEntidadVisual.Unidad,
            id,
            tipo,
            propietario,
            x,
            y);

        return entidad;
    }

    /// <summary>Invoca la validacion privada de objetivo.</summary>
    private static bool EsObjetivoValido(
        EntidadSeleccionableVista objetivo)
    {
        return (bool)typeof(ControladorAcciones)
            .GetMethod(
                "EsObjetivoAtaqueValido",
                BindingFlags.Static |
                BindingFlags.NonPublic)
            .Invoke(
                null,
                new object[] { objetivo });
    }

    /// <summary>Lee un campo privado por reflexion.</summary>
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

    /// <summary>Asigna un campo privado por reflexion.</summary>
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

    /// <summary>Invoca un metodo privado por reflexion.</summary>
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
