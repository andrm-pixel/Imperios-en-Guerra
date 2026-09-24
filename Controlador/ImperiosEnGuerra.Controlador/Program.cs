using Microsoft.Extensions.Options;
using ImperiosEnGuerra.Controlador.Configuracion;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Mapeadores;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Persistencia;
using ImperiosEnGuerra.Modelo.Concurrencia;
using ImperiosEnGuerra.Modelo.Unidades;

// Punto de entrada: configura servicios y la aplicación web mínima.
// Registra archivos, estado de partida y servicios de acciones concurrentes.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(
    new ServicioArchivos(
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "DatosPartida")));
builder.Services.AddSingleton<EstadoPartidaService>();
builder.Services.AddSingleton<GestorProcesosConcurrentes>();
builder.Services.AddSingleton<ServicioOrdenesUnidad>();
builder.Services.AddSingleton<ServicioAccionesConcurrentes>();


var app = builder.Build();

// Expone OpenAPI solo en desarrollo para documentar los endpoints.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Devuelve el estado del servicio del controlador.
app.MapGet("/api/estado", () =>
{
    return Results.Ok(new
    {
        estado = "activo",
        servicio = "ImperiosEnGuerra.Controlador"
    });
})
.WithName("ObtenerEstado");

// Verifica la conexión con el modelo creando un mapa de prueba.
app.MapGet("/api/modelo/prueba", () =>
{
    Mapa mapa = new Mapa(10, 8);

    return Results.Ok(new
    {
        modelo = "conectado",
        ancho = mapa.Ancho,
        alto = mapa.Alto,
        casillas = mapa.Ancho * mapa.Alto
    });
})
.WithName("ProbarModelo");

// Inicia una nueva partida y arranca la IA de la máquina.
app.MapPost(
    "/api/partida/iniciar",
    (
        IniciarPartidaRequest request,
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes accionesConcurrentes,
        ServicioArchivos servicioArchivos) =>
{
    try
    {
        accionesConcurrentes.CancelarTodos();

        Mapa mapa = new Mapa(
            request.AnchoMapa,
            request.AltoMapa);

        Coordenada centroHumano =
            PartidaRequestMapper.ConvertirCoordenada(
                request.CentroHumano);

        Coordenada centroMaquina =
            PartidaRequestMapper.ConvertirCoordenada(
                request.CentroMaquina);

        var recursosHumano =
            PartidaRequestMapper.ConvertirRecursos(
                request.RecursosHumano);

        var recursosMaquina =
            PartidaRequestMapper.ConvertirRecursos(
                request.RecursosMaquina);

        var inicializador = new InicializadorPartida();

        Partida partida = inicializador.Crear(
            request.NombreHumano ?? string.Empty,
            mapa,
            centroHumano,
            recursosHumano,
            request.NombreMaquina ?? string.Empty,
            mapa,
            centroMaquina,
            recursosMaquina);

        servicioArchivos.GuardarConfiguracionInicial(
            partida);

        estadoPartida.EstablecerPartida(partida);

        DestacarGuarnicionMaquina(partida, centroMaquina);

        accionesConcurrentes.IniciarIA();

        return Results.Ok(new
        {
            estado = "iniciada",

            mapa = new
            {
                ancho = mapa.Ancho,
                alto = mapa.Alto
            },

            jugadorHumano = new
            {
                nombre = partida.JugadorHumano.Nombre,
                tipo = partida.JugadorHumano.Tipo.ToString(),
                edificios = partida.JugadorHumano.Edificios.Count,
                unidades = partida.JugadorHumano.Unidades.Count
            },

            jugadorMaquina = new
            {
                nombre = partida.JugadorMaquina.Nombre,
                tipo = partida.JugadorMaquina.Tipo.ToString(),
                edificios = partida.JugadorMaquina.Edificios.Count,
                unidades = partida.JugadorMaquina.Unidades.Count
            }
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new
        {
            error = ex.Message
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new
        {
            error = ex.Message
        });
    }
    catch (IOException ex)
    {
        return Results.Problem(
            title: "No se pudo guardar configuracion.txt.",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Problem(
            title: "No se pudo guardar configuracion.txt.",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("IniciarPartida");

// Obtiene el estado actual de la partida activa.
app.MapGet("/api/partida", (EstadoPartidaService estadoPartida) =>
{
    EstadoPartidaResponse? respuesta = estadoPartida.ObtenerEstado();

    if (respuesta == null)
    {
        return Results.NotFound(new
        {
            error = "No hay una partida activa."
        });
    }

    return Results.Ok(respuesta);
})
.WithName("ObtenerPartidaActiva");

// Mueve una unidad de forma síncrona.
app.MapPost(
    "/api/partida/mover",
    (MoverUnidadRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.MoverUnidad(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("MoverUnidad");

// Inicia un movimiento como proceso concurrente (devuelve 202).
app.MapPost(
    "/api/partida/mover-concurrente",
    (
        MoverUnidadRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    var proceso =
        accionesConcurrentes.IniciarMovimiento(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarMovimientoConcurrente");

// Consulta el resultado de un proceso concurrente por su ID.
app.MapGet(
    "/api/procesos/{procesoId:guid}/resultado",
    (
        Guid procesoId,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    if (!accionesConcurrentes.IntentarObtenerResultado(
        procesoId,
        out ResultadoProcesoConcurrente resultado))
    {
        return Results.NoContent();
    }

    return Results.Ok(new
    {
        procesoId = resultado.ProcesoId,
        nombre = resultado.Nombre,
        estado = resultado.Estado.ToString(),
        hiloTrabajoId = resultado.HiloTrabajoId,
        exito = resultado.Resultado?.Exito ?? false,
        mensaje = resultado.Resultado?.Mensaje,
        errorTecnico = resultado.ErrorTecnico
    });
})
.WithName("ObtenerResultadoProcesoPorId");

// Obtiene el siguiente resultado disponible de cualquier proceso.
app.MapGet(
    "/api/procesos/resultado",
    (ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    if (!accionesConcurrentes.IntentarObtenerResultado(
        out ResultadoProcesoConcurrente resultado))
    {
        return Results.NoContent();
    }

    return Results.Ok(new
    {
        procesoId = resultado.ProcesoId,
        nombre = resultado.Nombre,
        estado = resultado.Estado.ToString(),
        hiloTrabajoId = resultado.HiloTrabajoId,
        exito = resultado.Resultado?.Exito ?? false,
        mensaje = resultado.Resultado?.Mensaje,
        errorTecnico = resultado.ErrorTecnico
    });
})
.WithName("ObtenerResultadoProceso");

// Solicita la cancelación de un proceso concurrente activo.
app.MapPost(
    "/api/procesos/{procesoId:guid}/cancelar",
    (
        Guid procesoId,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    return accionesConcurrentes.Cancelar(procesoId)
        ? Results.Ok(new
        {
            procesoId,
            estado = "cancelacion_solicitada"
        })
        : Results.NotFound(new
        {
            error = "No existe un proceso activo con ese ID."
        });
})
.WithName("CancelarProceso");

// Inicia una recolección como proceso concurrente (devuelve 202).
app.MapPost(
    "/api/partida/recolectar-concurrente",
    (
        RecolectarRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    var proceso =
        accionesConcurrentes.IniciarRecoleccion(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarRecoleccionConcurrente");

// Inicia una recolección de recursos de forma síncrona.
app.MapPost(
    "/api/partida/recolectar",
    (RecolectarRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.IniciarRecoleccion(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("IniciarRecoleccion");

// Inicia una construcción como proceso concurrente (devuelve 202).
app.MapPost(
    "/api/partida/construir-concurrente",
    (
        ConstruirRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    var proceso =
        accionesConcurrentes.IniciarConstruccion(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarConstruccionConcurrente");

// Construye un edificio de forma síncrona.
app.MapPost(
    "/api/partida/construir",
    (ConstruirRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.Construir(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("Construir");


// Inicia un entrenamiento como proceso concurrente (devuelve 202).
app.MapPost(
    "/api/partida/entrenar-concurrente",
    (
        EntrenarRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    var proceso =
        accionesConcurrentes.IniciarEntrenamiento(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarEntrenamientoConcurrente");

// Entrena una unidad de forma síncrona.
app.MapPost(
    "/api/partida/entrenar",
    (EntrenarRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.Entrenar(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("Entrenar");

// Inicia un ataque como proceso concurrente (devuelve 202).
app.MapPost(
    "/api/partida/atacar-concurrente",
    (
        AtacarRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    ProcesoConcurrente proceso =
        accionesConcurrentes.IniciarAtaque(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarAtaqueConcurrente");

// Ejecuta un ataque entre unidades de forma síncrona.
app.MapPost(
    "/api/partida/atacar",
    (AtacarRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.Atacar(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("Atacar");

// Coloca la guarnición inicial (guerrero y lancero) junto al centro de la máquina.
static void DestacarGuarnicionMaquina(Partida partida, Coordenada centroMaquina)
{
    var candidatos = new[]
    {
        new Coordenada(centroMaquina.X - 2, centroMaquina.Y),
        new Coordenada(centroMaquina.X - 1, centroMaquina.Y - 1)
    };

    var colocados = new List<Unidad>();

    foreach (Coordenada posicion in candidatos)
    {
        if (!partida.JugadorMaquina.Mapa.EstaDentroDeLimites(posicion) ||
            !partida.JugadorMaquina.Mapa.PuedeColocar(posicion))
        {
            continue;
        }

        bool ocupadaPorUnidad =
            partida.JugadorHumano.Unidades.Any(u => u.Coordenada != null && u.Coordenada.X == posicion.X && u.Coordenada.Y == posicion.Y) ||
            partida.JugadorMaquina.Unidades.Any(u => u.Coordenada != null && u.Coordenada.X == posicion.X && u.Coordenada.Y == posicion.Y);

        if (ocupadaPorUnidad)
            continue;

        colocados.Add(
            colocados.Count == 0
                ? (Unidad)new Guerrero(posicion)
                : (Unidad)new Lancero(posicion));
    }

    foreach (Unidad unidad in colocados)
        partida.JugadorMaquina.AgregarUnidad(unidad);
}

app.Run();