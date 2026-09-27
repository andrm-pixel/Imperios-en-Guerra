#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using ImperiosEnGuerra.Controladores;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Vistas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pruebas de preparacion y vista del movimiento.</summary>
public class MovimientoUnityTests
{
    /// <summary>Raiz de los objetos de prueba de movimiento.</summary>
    private GameObject raiz;
    private ControladorSeleccion seleccion;
    private ControladorAcciones acciones;
    private ControladorConexionApi conexion;
    /// <summary>Vista observada en la prueba de movimiento.</summary>
    private VistaPartida vista;
    /// <summary>Unidad usada en la prueba de movimiento.</summary>
    private EntidadSeleccionableVista unidad;
    private Text mensaje;
    private const string IdModelo = "11111111-1111-1111-1111-111111111111";

    /// <summary>Crea la escena minima para probar el movimiento.</summary>
    [SetUp]
    public void Preparar()
    {
        raiz = new GameObject("Prueba movimiento");
        raiz.SetActive(false);
        vista = raiz.AddComponent<VistaPartida>();
        seleccion = raiz.AddComponent<ControladorSeleccion>();
        acciones = raiz.AddComponent<ControladorAcciones>();
        conexion = raiz.AddComponent<ControladorConexionApi>();
        var hud = raiz.AddComponent<VistaHud>();
        var texto = new GameObject("Mensaje", typeof(RectTransform), typeof(Text));
        texto.transform.SetParent(raiz.transform);
        mensaje = texto.GetComponent<Text>();
        Campo(hud, "mensaje", mensaje);
        Campo(seleccion, "vistaPartida", vista);
        Campo(acciones, "controladorSeleccion", seleccion);
        Campo(acciones, "vistaHud", hud);
        Campo(acciones, "conexionApi", conexion);
        Campo(conexion, "vistaHud", hud);
        var objeto = new GameObject("Unidad", typeof(SpriteRenderer), typeof(EntidadSeleccionableVista));
        objeto.transform.SetParent(raiz.transform);
        unidad = objeto.GetComponent<EntidadSeleccionableVista>();
        unidad.Configurar(CategoriaEntidadVisual.Unidad, IdModelo, "Aldeano", "Humano", 1, 1);
        raiz.SetActive(true);
        // Estos MonoBehaviour no usan ExecuteAlways: EditMode no ejecuta su ciclo de vida.
        Invocar(seleccion, "OnEnable");
        Invocar(acciones, "OnEnable");
        Invocar(seleccion, "Seleccionar", unidad);
    }

    /// <summary>Destruye la escena de prueba de movimiento.</summary>
    [TearDown]
    public void Limpiar()
    {
        Invocar(acciones, "OnDisable");
        Invocar(seleccion, "OnDisable");
        Object.DestroyImmediate(raiz);
    }

    [TestCase(CategoriaEntidadVisual.Edificio, "Humano", IdModelo)]
    [TestCase(CategoriaEntidadVisual.Recurso, "Humano", IdModelo)]
    [TestCase(CategoriaEntidadVisual.Unidad, "Maquina", IdModelo)]
    /// <summary>Verifica que lo inapropiado no prepara movimiento.</summary>
    [TestCase(CategoriaEntidadVisual.Unidad, "Humano", "")]
    public void SeleccionInapropiada_NoPreparaMovimiento(CategoriaEntidadVisual categoria, string propietario, string id)
    {
        unidad.Configurar(categoria, id, "Aldeano", propietario, 1, 1);
        Invocar(acciones, "PrepararAccion", "Mover");
        Assert.That(seleccion.CapturandoDestino, Is.False);
        Assert.That(conexion.MovimientoEnCurso, Is.False);
    }

    /// <summary>Verifica que preparar conserva el id sin mover.</summary>
    [Test]
    public void Preparar_ConservaIdSinEnviarNiMover()
    {
        Vector3 posicion = unidad.transform.position;
        Invocar(acciones, "PrepararAccion", "Mover");
        Assert.That(seleccion.CapturandoDestino, Is.True);
        Assert.That(LeerCampo(acciones, "unidadIdPendiente"), Is.EqualTo(IdModelo));
        Assert.That(mensaje.text, Is.EqualTo("Selecciona una casilla destino."));
        Assert.That(conexion.MovimientoEnCurso, Is.False);
        Assert.That(unidad.transform.position, Is.EqualTo(posicion));
    }

    /// <summary>Verifica que Escape cancela sin deseleccionar.</summary>
    [Test]
    public void CancelacionUsadaPorEscape_LimpiaIntencionSinEnviarNiDeseleccionar()
    {
        Invocar(acciones, "PrepararAccion", "Mover");
        Invocar(seleccion, "CancelarCapturaDestino");
        ComprobarCancelacion();
        Assert.That(seleccion.SeleccionActual, Is.SameAs(unidad));
    }

    /// <summary>Verifica que cambiar seleccion cancela el movimiento.</summary>
    [Test]
    public void CambioSeleccion_CancelaSinEnviar()
    {
        Invocar(acciones, "PrepararAccion", "Mover");
        seleccion.LimpiarSeleccion();
        ComprobarCancelacion();
    }

    /// <summary>Verifica que destruir la unidad cancela.</summary>
    [Test]
    public void UnidadDesaparece_CancelaSinEnviar()
    {
        Invocar(acciones, "PrepararAccion", "Mover");
        Object.DestroyImmediate(unidad.gameObject);
        Invocar(acciones, "Update");
        ComprobarCancelacion();
    }

    /// <summary>Verifica que no se duplica orden activa.</summary>
    [Test]
    public void UnidadConOrdenActiva_NoAceptaSegundaOrdenSimultanea()
    {
        unidad.ActualizarDatosLogicos(
            1,
            1,
            "Moviendo",
            "Mover");

        Invocar(
            acciones,
            "PrepararAccion",
            "Mover");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.False);

        Assert.That(
            mensaje.text,
            Does.Contain("entidad humana apropiada"));
    }

    /// <summary>Verifica que el curso no bloquea preparar.</summary>
    [Test]
    public void MovimientoEnCurso_NoBloqueaPrepararOtraOrden()
    {
        CampoAutomatico(
            conexion,
            "MovimientoEnCurso",
            true);

        Invocar(
            acciones,
            "PrepararAccion",
            "Mover");

        Assert.That(
            seleccion.CapturandoDestino,
            Is.True);

        Assert.That(
            mensaje.text,
            Is.EqualTo("Selecciona una casilla destino."));
    }

    /// <summary>Verifica el JSON del contrato de movimiento.</summary>
    [Test]
    public void Dto_UsaIdSeleccionadoYNombresDelContrato()
    {
        Invocar(acciones, "PrepararAccion", "Mover");
        var dto = new MoverUnidadDto
        {
            unidadId = (string)LeerCampo(acciones, "unidadIdPendiente"),
            destino = new CoordenadaDto(4, 5)
        };
        Assert.That(JsonUtility.ToJson(dto),
            Is.EqualTo("{\"unidadId\":\"" + IdModelo + "\",\"destino\":{\"x\":4,\"y\":5}}"));
    }

    /// <summary>Verifica los DTOs concurrentes de movimiento.</summary>
    [Test]
    public void DtoConcurrente_LeeInicioYResultadoDelWorker()
    {
        string inicioJson =
            "{\"procesoId\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\",\"nombre\":\"MOVER\",\"estado\":\"iniciado\"}";

        ProcesoIniciadoDto inicio =
            JsonUtility.FromJson<ProcesoIniciadoDto>(inicioJson);

        Assert.That(
            inicio.procesoId,
            Is.EqualTo("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        Assert.That(inicio.nombre, Is.EqualTo("MOVER"));
        Assert.That(inicio.estado, Is.EqualTo("iniciado"));

        string resultadoJson =
            "{\"procesoId\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\",\"nombre\":\"MOVER\",\"estado\":\"Completado\",\"hiloTrabajoId\":7,\"exito\":true,\"mensaje\":\"Movimiento realizado.\",\"errorTecnico\":null}";

        ResultadoProcesoDto resultado =
            JsonUtility.FromJson<ResultadoProcesoDto>(
                resultadoJson);

        Assert.That(resultado.estado, Is.EqualTo("Completado"));
        Assert.That(resultado.hiloTrabajoId, Is.EqualTo(7));
        Assert.That(resultado.exito, Is.True);
        Assert.That(
            resultado.mensaje,
            Is.EqualTo("Movimiento realizado."));
    }

    [TestCase(8f, 10f, 4, 5)]
    /// <summary>Verifica la conversion mundo a casilla.</summary>
    [TestCase(0f, 0f, 0, 0)]
    public void CasillaVacia_SeConvierteSinEntidad(float mundoX, float mundoY, int esperadoX, int esperadoY)
    {
        Campo(vista, "anchoVisual", 10);
        Campo(vista, "altoVisual", 10);
        Assert.That(vista.TryObtenerCoordenadaLogica(new Vector3(mundoX, mundoY), out int x, out int y), Is.True);
        Assert.That(x, Is.EqualTo(esperadoX));
        Assert.That(y, Is.EqualTo(esperadoY));
    }

    /// <summary>Verifica que la vista no teletransporta.</summary>
    [Test]
    public void VistaMovimiento_ActualizaDatosSinTeletransportarSprite()
    {
        Vector3 posicionInicial =
            new Vector3(2f, 2f, 0f);

        unidad.transform.position =
            posicionInicial;

        Assert.That(
            vista.ActualizarMovimientoUnidad(
                IdModelo,
                2,
                1,
                "Moviendo",
                "Mover"),
            Is.True);

        Assert.That(
            unidad.X,
            Is.EqualTo(2));

        Assert.That(
            unidad.Y,
            Is.EqualTo(1));

        Assert.That(
            unidad.EstadoLogico,
            Is.EqualTo("Moviendo"));

        Assert.That(
            unidad.OrdenActiva,
            Is.EqualTo("Mover"));

        Assert.That(
            unidad.transform.position,
            Is.EqualTo(posicionInicial));
    }

    /// <summary>Verifica que un id desconocido no mueve.</summary>
    [Test]
    public void VistaMovimiento_IdDesconocido_NoCreaMovimiento()
    {
        Assert.That(
            vista.ActualizarMovimientoUnidad(
                "id-inexistente",
                4,
                4,
                "Moviendo",
                "Mover"),
            Is.False);
    }

    /// <summary>Comprueba el estado tras cancelar el movimiento.</summary>
    private void ComprobarCancelacion()
    {
        Assert.That(seleccion.CapturandoDestino, Is.False);
        Assert.That(LeerCampo(acciones, "unidadIdPendiente"), Is.Null);
        Assert.That(conexion.MovimientoEnCurso, Is.False);
        Assert.That(mensaje.text, Is.EqualTo("Movimiento cancelado."));
    }

    /// <summary>Lee un campo privado por reflexion.</summary>
    private static object LeerCampo(object objeto, string nombre) => objeto.GetType()
        .GetField(nombre, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(objeto);

    /// <summary>Asigna un campo privado por reflexion.</summary>
    private static void Campo(object objeto, string nombre, object valor) => objeto.GetType()
        .GetField(nombre, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(objeto, valor);

    /// <summary>Asigna una propiedad automatica por reflexion.</summary>
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

    /// <summary>Invoca un metodo privado por reflexion.</summary>
    private static void Invocar(object objeto, string nombre, params object[] argumentos) => objeto.GetType()
        .GetMethod(nombre, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(objeto, argumentos);
}
#endif
