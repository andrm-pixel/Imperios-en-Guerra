using System.IO;
using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Mapeadores;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Modelo.Persistencia;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Recursos;
using System.Linq; // para usar FirstOrDefault()

namespace ImperiosEnGuerra.Modelo.Servicios;

/// <summary>
/// Representa estado partida service dentro del modelo del juego.
/// </summary>
public sealed class EstadoPartidaService
{
    private readonly object sincronizacion = new();
    private readonly ServicioArchivos? servicioArchivos;
    private readonly ConfiguracionEconomia configuracionEconomia =
        new ConfiguracionEconomia();
    private Partida? partidaActiva;

    /// <summary>
    /// Inicializa una nueva instancia de EstadoPartidaService.
    /// </summary>
    public EstadoPartidaService()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de EstadoPartidaService.
    /// </summary>
    /// <param name="servicioArchivos">El valor de servicio archivos.</param>
    public EstadoPartidaService(ServicioArchivos servicioArchivos)
    {
        this.servicioArchivos =
            servicioArchivos ?? throw new ArgumentNullException(nameof(servicioArchivos));
    }

    /// <summary>
    /// Mueve unidad.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion MoverUnidad(MoverUnidadRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("La solicitud de movimiento es obligatoria."));

            if (!Guid.TryParse(request.UnidadId, out Guid unidadId))
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("El ID de la unidad debe tener formato Guid válido."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("El destino es obligatorio."));

            var solicitud = new SolicitudMovimiento(
                unidadId,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));

            return RegistrarResultado(
                "MOVER",
                new OperacionMovimiento().Ejecutar(partidaActiva, solicitud));
        }
    }

    /// <summary>
    /// Ejecuta la operación preparar movimiento progresivo.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <param name="permitirOrdenMovimientoActiva">El valor de permitir orden movimiento activa.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoPlanMovimiento PrepararMovimientoProgresivo(
        MoverUnidadRequest? request,
        bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoPlanMovimiento.Fallido(
                    "No hay una partida activa.");

            if (request == null)
                return ResultadoPlanMovimiento.Fallido(
                    "La solicitud de movimiento es obligatoria.");

            if (!Guid.TryParse(
                    request.UnidadId,
                    out Guid unidadId))
            {
                return ResultadoPlanMovimiento.Fallido(
                    "El ID de la unidad debe tener formato Guid válido.");
            }

            if (request.Destino == null)
                return ResultadoPlanMovimiento.Fallido(
                    "El destino es obligatorio.");

            var solicitud =
                new SolicitudMovimiento(
                    unidadId,
                    PartidaRequestMapper.ConvertirCoordenada(
                        request.Destino));

            return new PlanificadorMovimiento()
                .Preparar(
                    partidaActiva,
                    solicitud,
                    permitirOrdenMovimientoActiva);
        }
    }

    /// <summary>
    /// Intenta iniciar orden unidad.
    /// </summary>
    /// <param name="unidadId">El valor de unidad id.</param>
    /// <param name="tipo">El valor de tipo.</param>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool IntentarIniciarOrdenUnidad(
        Guid unidadId,
        TipoAccionJuego tipo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return false;

            Unidad? unidad =
                partidaActiva.JugadorHumano.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            return unidad != null &&
                   unidad.IntentarIniciarOrden(tipo);
        }
    }

    /// <summary>
    /// Intenta reemplazar orden unidad.
    /// </summary>
    /// <param name="unidadId">El valor de unidad id.</param>
    /// <param name="tipo">El valor de tipo.</param>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool IntentarReemplazarOrdenUnidad(
        Guid unidadId,
        TipoAccionJuego tipo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return false;

            Unidad? unidad =
                partidaActiva.JugadorHumano.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            return unidad != null &&
                   unidad.IntentarReemplazarOrden(tipo);
        }
    }

    /// <summary>
    /// Completa orden unidad.
    /// </summary>
    /// <param name="unidadId">El valor de unidad id.</param>
    public void CompletarOrdenUnidad(
        Guid unidadId)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return;

            Unidad? unidad =
                partidaActiva.JugadorHumano.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            unidad?.CompletarOrden();
        }
    }

    /// <summary>
    /// Ejecuta la operación avanzar movimiento.
    /// </summary>
    /// <param name="unidadId">El valor de unidad id.</param>
    /// <param name="siguiente">El valor de siguiente.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion AvanzarMovimiento(
        Guid unidadId,
        Coordenada siguiente)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            return new OperacionPasoMovimiento()
                .Ejecutar(
                    partidaActiva,
                    unidadId,
                    siguiente);
        }
    }

    /// <summary>
    /// Ejecuta la operación preparar aproximacion recurso.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <param name="permitirOrdenMovimientoActiva">El valor de permitir orden movimiento activa.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAproximacionRecurso PrepararAproximacionRecurso(
        RecolectarRequest? request,
        bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "No hay una partida activa.");
            }

            if (request == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "La solicitud de recolección es obligatoria.");
            }

            if (!Guid.TryParse(
                    request.AldeanoId,
                    out Guid aldeanoId))
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El ID del Aldeano debe tener formato Guid válido.");
            }

            if (request.Objetivo == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El objetivo de recolección es obligatorio.");
            }

            var solicitud =
                new SolicitudRecoleccion(
                    aldeanoId,
                    PartidaRequestMapper.ConvertirCoordenada(
                        request.Objetivo));

            return new PlanificadorAproximacionRecurso()
                .Preparar(
                    partidaActiva,
                    solicitud,
                    permitirOrdenMovimientoActiva);
        }
    }

    /// <summary>
    /// Ejecuta la operación preparar aproximacion deposito.
    /// </summary>
    /// <param name="aldeanoId">El valor de aldeano id.</param>
    /// <param name="permitirOrdenMovimientoActiva">El valor de permitir orden movimiento activa.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAproximacionDeposito PrepararAproximacionDeposito(
        Guid aldeanoId,
        bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No hay una partida activa.");
            }

            return new PlanificadorAproximacionDeposito()
                .Preparar(
                    partidaActiva,
                    aldeanoId,
                    permitirOrdenMovimientoActiva);
        }
    }

    /// <summary>
    /// Ejecuta la operación depositar carga.
    /// </summary>
    /// <param name="aldeanoId">El valor de aldeano id.</param>
    /// <param name="castillo">El valor de centro urbano.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoDepositoRecoleccion DepositarCarga(
        Guid aldeanoId,
        Coordenada castillo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No hay una partida activa.");
            }

            return new OperacionDepositoRecoleccion()
                .Ejecutar(
                    partidaActiva,
                    aldeanoId,
                    castillo);
        }
    }

    /// <summary>
    /// Ejecuta la operación recurso existe.
    /// </summary>
    /// <param name="objetivo">El valor de objetivo.</param>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool RecursoExiste(
        Coordenada objetivo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null ||
                objetivo == null)
            {
                return false;
            }

            return partidaActiva.JugadorHumano.Mapa
                .ObtenerRecursoEn(objetivo) != null;
        }
    }

    /// <summary>
    /// Ejecuta la operación recurso disponible.
    /// </summary>
    /// <param name="objetivo">El valor de objetivo.</param>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool RecursoDisponible(
        Coordenada objetivo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null ||
                objetivo == null)
            {
                return false;
            }

            var recurso =
                partidaActiva.JugadorHumano.Mapa
                    .ObtenerRecursoEn(objetivo);

            return recurso != null &&
                   !recurso.Agotado;
        }
    }

    /// <summary>
    /// Recolecta paso.
    /// </summary>
    /// <param name="aldeanoId">El valor de aldeano id.</param>
    /// <param name="objetivo">El valor de objetivo.</param>
    /// <param name="tasa">El valor de tasa.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoPasoRecoleccion RecolectarPaso(
        Guid aldeanoId,
        Coordenada objetivo,
        int tasa)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "No hay una partida activa.");
            }

            return new OperacionPasoRecoleccion()
                .Ejecutar(
                    partidaActiva,
                    aldeanoId,
                    objetivo,
                    tasa);
        }
    }

    /// <summary>
    /// Inicia recoleccion.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion IniciarRecoleccion(RecolectarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("La solicitud de recolección es obligatoria."));

            if (!Guid.TryParse(request.AldeanoId, out Guid aldeanoId))
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("El ID del Aldeano debe tener formato Guid válido."));

            if (request.Objetivo == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("El objetivo de recolección es obligatorio."));

            var solicitud = new SolicitudRecoleccion(
                aldeanoId,
                PartidaRequestMapper.ConvertirCoordenada(request.Objetivo));

            return RegistrarResultado(
                "RECOLECTAR",
                new OperacionRecoleccion().Ejecutar(partidaActiva, solicitud));
        }
    }

    /// <summary>
    /// Ejecuta la operación reservar costo construccion.
    /// </summary>
    /// <param name="tipoEdificio">El valor de tipo edificio.</param>
    /// <param name="costo">El valor de costo.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion ReservarCostoConstruccion(
        string tipoEdificio,
        out CostoRecursos costo)
    {
        lock (sincronizacion)
        {
            costo = null;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (!configuracionEconomia
                .IntentarObtenerCostoEdificio(
                    tipoEdificio,
                    out costo))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de edificio no tiene un costo configurado.");
            }

            if (!partidaActiva.JugadorHumano.Recursos
                .IntentarGastar(costo))
            {
                return ResultadoAccion.Fallido(
                    $"Recursos insuficientes. Costo: {costo}.");
            }

            return ResultadoAccion.Exitoso(
                $"Costo reservado: {costo}.");
        }
    }

    /// <summary>
    /// Ejecuta la operación reservar costo entrenamiento.
    /// </summary>
    /// <param name="tipoUnidad">El valor de tipo unidad.</param>
    /// <param name="costo">El valor de costo.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion ReservarCostoEntrenamiento(
        string tipoUnidad,
        out CostoRecursos costo)
    {
        lock (sincronizacion)
        {
            costo = null;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (!configuracionEconomia
                .IntentarObtenerCostoUnidad(
                    tipoUnidad,
                    out costo))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de unidad no tiene un costo configurado.");
            }

            if (!partidaActiva.JugadorHumano.Recursos
                .IntentarGastar(costo))
            {
                return ResultadoAccion.Fallido(
                    $"Recursos insuficientes. Costo: {costo}.");
            }

            return ResultadoAccion.Exitoso(
                $"Costo reservado: {costo}.");
        }
    }

    /// <summary>
    /// Ejecuta la operación reembolsar costo.
    /// </summary>
    /// <param name="costo">El valor de costo.</param>
    public void ReembolsarCosto(
        CostoRecursos costo)
    {
        if (costo == null)
            return;

        lock (sincronizacion)
        {
            partidaActiva?.JugadorHumano
                .Recursos.Reintegrar(costo);
        }
    }

    /// <summary>
    /// Inicia obra.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <param name="obraId">El valor de obra id.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion IniciarObra(
        ConstruirRequest? request,
        out Guid obraId)
    {
        lock (sincronizacion)
        {
            obraId = Guid.Empty;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (request == null)
            {
                return ResultadoAccion.Fallido(
                    "La solicitud de construcción es obligatoria.");
            }

            if (!Guid.TryParse(
                    request.AldeanoId,
                    out Guid aldeanoId))
            {
                return ResultadoAccion.Fallido(
                    "El ID del Aldeano debe tener formato Guid válido.");
            }

            Aldeano? aldeano =
                partidaActiva.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (aldeano == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe un Aldeano humano con ese ID.");
            }

            if (!aldeano.Disponible)
            {
                return ResultadoAccion.Fallido(
                    "El Aldeano no está disponible.");
            }

            if (request.Destino == null)
            {
                return ResultadoAccion.Fallido(
                    "La posición de construcción es obligatoria.");
            }

            if (!string.Equals(
                    request.TipoEdificio,
                    nameof(Castillo),
                    StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de edificio indicado no está permitido.");
            }

            Coordenada destino =
                PartidaRequestMapper.ConvertirCoordenada(
                    request.Destino);

            Mapa mapa =
                partidaActiva.JugadorHumano.Mapa;

            if (!mapa.EstaDentroDeLimites(destino))
            {
                return ResultadoAccion.Fallido(
                    "La posición está fuera del mapa.");
            }

            if (!mapa.PuedeColocar(destino))
            {
                return ResultadoAccion.Fallido(
                    "La posición indicada no está disponible.");
            }

            Casilla? casilla =
                mapa.ObtenerCasilla(
                    destino.X,
                    destino.Y);

            if (casilla == null ||
                !casilla.Ocupar())
            {
                return ResultadoAccion.Fallido(
                    "No se pudo reservar la casilla de construcción.");
            }

            var obra =
                new ObraConstruccion(
                    aldeanoId,
                    nameof(Castillo),
                    destino);

            partidaActiva.JugadorHumano
                .AgregarObraConstruccion(
                    obra);

            obraId = obra.Id;

            return ResultadoAccion.Exitoso(
                "Obra reservada correctamente.");
        }
    }

    /// <summary>
    /// Ejecuta la operación preparar aproximacion construccion.
    /// </summary>
    /// <param name="aldeanoId">El valor de aldeano id.</param>
    /// <param name="obraId">El valor de obra id.</param>
    /// <param name="permitirOrdenMovimientoActiva">El valor de permitir orden movimiento activa.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAproximacionConstruccion
        PrepararAproximacionConstruccion(
            Guid aldeanoId,
            Guid obraId,
            bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No hay una partida activa.");
            }

            ObraConstruccion? obra =
                partidaActiva.JugadorHumano.ObrasConstruccion
                    .FirstOrDefault(
                        o => o.Id == obraId);

            if (obra == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No existe la obra indicada.");
            }

            return new PlanificadorAproximacionConstruccion()
                .Preparar(
                    partidaActiva,
                    aldeanoId,
                    obra.Coordenada,
                    permitirOrdenMovimientoActiva);
        }
    }

    /// <summary>
    /// Ejecuta la operación avanzar obra.
    /// </summary>
    /// <param name="obraId">El valor de obra id.</param>
    /// <param name="incremento">El valor de incremento.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoProgresoConstruccion AvanzarObra(
        Guid obraId,
        int incremento)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoProgresoConstruccion.Fallido(
                    "No hay una partida activa.");
            }

            ObraConstruccion? obra =
                partidaActiva.JugadorHumano.ObrasConstruccion
                    .FirstOrDefault(
                        o => o.Id == obraId);

            if (obra == null)
            {
                return ResultadoProgresoConstruccion.Fallido(
                    "No existe la obra indicada.");
            }

            int progreso =
                obra.Avanzar(
                    incremento);

            bool terminada =
                obra.Terminada;

            if (terminada)
            {
                partidaActiva.JugadorHumano
                    .EliminarObraConstruccion(
                        obra);

                partidaActiva.JugadorHumano
                    .AgregarEdificio(
                        new Castillo(
                            obra.Coordenada));
            }

            return ResultadoProgresoConstruccion.Exitoso(
                progreso,
                terminada);
        }
    }

    /// <summary>
    /// Cancela obra.
    /// </summary>
    /// <param name="obraId">El valor de obra id.</param>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool CancelarObra(
        Guid obraId)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return false;

            ObraConstruccion? obra =
                partidaActiva.JugadorHumano.ObrasConstruccion
                    .FirstOrDefault(
                        o => o.Id == obraId);

            if (obra == null)
                return false;

            partidaActiva.JugadorHumano
                .EliminarObraConstruccion(
                    obra);

            partidaActiva.JugadorHumano.Mapa
                .ObtenerCasilla(
                    obra.Coordenada.X,
                    obra.Coordenada.Y)
                ?.Liberar();

            return true;
        }
    }

    /// <summary>
    /// Construye el elemento solicitado.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion Construir(ConstruirRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("La solicitud de construcción es obligatoria."));

            if (!Guid.TryParse(request.AldeanoId, out Guid aldeanoId))
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("El ID del Aldeano debe tener formato Guid válido."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("La posición de construcción es obligatoria."));

            var solicitud = new SolicitudConstruccion(
                aldeanoId,
                request.TipoEdificio ?? string.Empty,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));

            if (!configuracionEconomia.IntentarObtenerCostoEdificio(
                    request.TipoEdificio,
                    out CostoRecursos costo))
            {
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido(
                        "El tipo de edificio no tiene un costo configurado."));
            }

            if (!partidaActiva.JugadorHumano.Recursos
                .IntentarGastar(costo))
            {
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido(
                        $"Recursos insuficientes. Costo: {costo}."));
            }

            ResultadoAccion resultado =
                new OperacionConstruccion()
                    .Ejecutar(
                        partidaActiva,
                        solicitud);

            if (!resultado.Exito)
            {
                partidaActiva.JugadorHumano.Recursos
                    .Reintegrar(costo);
            }

            return RegistrarResultado(
                "CONSTRUIR",
                resultado);
        }
    }

    /// <summary>
    /// Ejecuta la operación encolar entrenamiento.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
    /// <param name="castillo">El valor de centro urbano.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion EncolarEntrenamiento(
        EntrenarRequest? request,
        out Guid entrenamientoId,
        out Coordenada castillo)
    {
        lock (sincronizacion)
        {
            entrenamientoId = Guid.Empty;
            castillo = null;

            if (partidaActiva == null)
            {
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");
            }

            if (request == null)
            {
                return ResultadoAccion.Fallido(
                    "La solicitud de entrenamiento es obligatoria.");
            }

            if (request.EdificioOrigen == null)
            {
                return ResultadoAccion.Fallido(
                    "El edificio de origen es obligatorio.");
            }

            if (!configuracionEconomia.IntentarObtenerCostoUnidad(
                    request.TipoUnidad,
                    out _))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de unidad indicado no está permitido.");
            }

            Coordenada origen =
                PartidaRequestMapper.ConvertirCoordenada(
                    request.EdificioOrigen);

            Castillo? centro =
                partidaActiva.JugadorHumano.Edificios
                    .OfType<Castillo>()
                    .FirstOrDefault(
                        e =>
                            e.Coordenada.X == origen.X &&
                            e.Coordenada.Y == origen.Y);

            if (centro == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe un Castillo humano en la posición indicada.");
            }

            Coordenada reunion =
                request.Destino == null
                    ? null
                    : PartidaRequestMapper.ConvertirCoordenada(
                        request.Destino);

            EntrenamientoPendiente pendiente =
                centro.EncolarEntrenamiento(
                    request.TipoUnidad ?? string.Empty,
                    reunion);

            entrenamientoId =
                pendiente.Id;

            castillo =
                centro.Coordenada;

            return ResultadoAccion.Exitoso(
                "Entrenamiento agregado a la cola.");
        }
    }

    /// <summary>
    /// Ejecuta la operación es turno entrenamiento.
    /// </summary>
    /// <param name="castillo">El valor de centro urbano.</param>
    /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool EsTurnoEntrenamiento(
        Coordenada castillo,
        Guid entrenamientoId)
    {
        lock (sincronizacion)
        {
            Castillo? centro =
                BuscarCentroHumano(
                    castillo);

            return centro != null &&
                   centro.EsPrimero(
                       entrenamientoId);
        }
    }

    /// <summary>
    /// Ejecuta la operación avanzar entrenamiento.
    /// </summary>
    /// <param name="castillo">El valor de centro urbano.</param>
    /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
    /// <param name="incremento">El valor de incremento.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoProgresoEntrenamiento AvanzarEntrenamiento(
        Coordenada castillo,
        Guid entrenamientoId,
        int incremento)
    {
        lock (sincronizacion)
        {
            Castillo? centro =
                BuscarCentroHumano(
                    castillo);

            return centro == null
                ? ResultadoProgresoEntrenamiento.Fallido(
                    "No existe el Castillo indicado.")
                : centro.AvanzarEntrenamiento(
                    entrenamientoId,
                    incremento);
        }
    }

    /// <summary>
    /// Completa entrenamiento con spawn.
    /// </summary>
    /// <param name="castillo">El valor de centro urbano.</param>
    /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
    /// <param name="tipoUnidad">El valor de tipo unidad.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoSpawnEntrenamiento CompletarEntrenamientoConSpawn(
        Coordenada castillo,
        Guid entrenamientoId,
        string tipoUnidad)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoSpawnEntrenamiento.Fallido(
                    "No hay una partida activa.");
            }

            Castillo? centro =
                BuscarCentroHumano(
                    castillo);

            if (centro == null ||
                !centro.EsPrimero(
                    entrenamientoId))
            {
                return ResultadoSpawnEntrenamiento.Fallido(
                    "La orden no está al frente de la cola.");
            }

            EntrenamientoPendiente? pendiente =
                centro.ColaEntrenamiento
                    .FirstOrDefault();

            if (pendiente == null ||
                pendiente.Id != entrenamientoId ||
                pendiente.Progreso < 100)
            {
                return ResultadoSpawnEntrenamiento.Fallido(
                    "El entrenamiento todavía no está completo.");
            }

            Coordenada spawn =
                new BuscadorCasillaSpawn()
                    .Buscar(
                        partidaActiva,
                        centro.Coordenada);

            if (spawn == null)
            {
                return ResultadoSpawnEntrenamiento.Fallido(
                    "No existe una casilla libre cercana para crear la unidad.");
            }

            Unidad unidad =
                FabricaUnidades.Crear(
                    tipoUnidad,
                    spawn);

            if (unidad == null)
            {
                return ResultadoSpawnEntrenamiento.Fallido(
                    "El tipo de unidad indicado no está permitido.");
            }

            Casilla? casilla =
                partidaActiva.JugadorHumano.Mapa
                    .ObtenerCasilla(
                        spawn.X,
                        spawn.Y);

            if (casilla == null ||
                !casilla.Ocupar())
            {
                return ResultadoSpawnEntrenamiento.Fallido(
                    "La casilla de aparición dejó de estar disponible.");
            }

            partidaActiva.JugadorHumano
                .AgregarUnidad(
                    unidad);

            if (!centro.CompletarEntrenamiento(
                    entrenamientoId))
            {
                partidaActiva.JugadorHumano
                    .EliminarUnidad(
                        unidad);

                casilla.Liberar();

                return ResultadoSpawnEntrenamiento.Fallido(
                    "No se pudo retirar la orden completada de la cola.");
            }

            return ResultadoSpawnEntrenamiento.Exitoso(
                unidad.Id,
                spawn,
                tipoUnidad);
        }
    }

    /// <summary>
    /// Cancela entrenamiento cola.
    /// </summary>
    /// <param name="castillo">El valor de centro urbano.</param>
    /// <param name="entrenamientoId">El valor de entrenamiento id.</param>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool CancelarEntrenamientoCola(
        Coordenada castillo,
        Guid entrenamientoId)
    {
        lock (sincronizacion)
        {
            Castillo? centro =
                BuscarCentroHumano(
                    castillo);

            return centro != null &&
                   centro.CancelarEntrenamiento(
                       entrenamientoId);
        }
    }

    private Castillo? BuscarCentroHumano(
        Coordenada coordenada)
    {
        if (partidaActiva == null ||
            coordenada == null)
        {
            return null;
        }

        return partidaActiva.JugadorHumano.Edificios
            .OfType<Castillo>()
            .FirstOrDefault(
                e =>
                    e.Coordenada.X == coordenada.X &&
                    e.Coordenada.Y == coordenada.Y);
    }

    /// <summary>
    /// Entrena el elemento solicitado.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion Entrenar(EntrenarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("La solicitud de entrenamiento es obligatoria."));

            if (request.EdificioOrigen == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("El edificio de origen es obligatorio."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("La posición de aparición es obligatoria."));

            var solicitud = new SolicitudEntrenamiento(
                PartidaRequestMapper.ConvertirCoordenada(request.EdificioOrigen),
                request.TipoUnidad ?? string.Empty,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));

            if (!configuracionEconomia.IntentarObtenerCostoUnidad(
                    request.TipoUnidad,
                    out CostoRecursos costo))
            {
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido(
                        "El tipo de unidad no tiene un costo configurado."));
            }

            if (!partidaActiva.JugadorHumano.Recursos
                .IntentarGastar(costo))
            {
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido(
                        $"Recursos insuficientes. Costo: {costo}."));
            }

            ResultadoAccion resultado =
                new OperacionEntrenamiento()
                    .Ejecutar(
                        partidaActiva,
                        solicitud);

            if (!resultado.Exito)
            {
                partidaActiva.JugadorHumano.Recursos
                    .Reintegrar(costo);
            }

            return RegistrarResultado(
                "ENTRENAR",
                resultado);
        }
    }

    /// <summary>
    /// Ataca el elemento solicitado.
    /// </summary>
    /// <param name="request">El valor de request.</param>
    /// <returns>Resultado de la operación.</returns>
    public ResultadoAccion Atacar(AtacarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("La solicitud de ataque es obligatoria."));

            if (!Guid.TryParse(request.AtacanteId, out Guid atacanteId))
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("El ID del atacante debe tener formato Guid válido."));

            if (!Guid.TryParse(request.ObjetivoId, out Guid objetivoId))
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("El ID del objetivo debe tener formato Guid válido."));

            var solicitud = new SolicitudAtaque(
                atacanteId,
                objetivoId);

            ResultadoAccion resultado =
                new OperacionAtaque().Ejecutar(partidaActiva, solicitud);

            if (resultado.Exito &&
                resultado.Mensaje != null &&
                resultado.Mensaje.Contains("¡Victoria!"))
            {
                RegistrarEventoSeguro(
                    $"VICTORIA|EXITO|{partidaActiva.JugadorHumano.Nombre} derrota a {partidaActiva.JugadorMaquina.Nombre}.");
                GuardarResultadoFinalSeguro(
                    $"Ganador={partidaActiva.JugadorHumano.Nombre}\n" +
                    $"Perdedor={partidaActiva.JugadorMaquina.Nombre}\n" +
                    $"UnidadesRestantesHumano={partidaActiva.JugadorHumano.Unidades.Count}\n");
            }

            return RegistrarResultado(
                "ATACAR",
                resultado);
        }
    }

    /// <summary>
    /// Obtiene estado.
    /// </summary>
    /// <returns>Resultado de la operación.</returns>
    public EstadoPartidaResponse? ObtenerEstado()
    {
        lock (sincronizacion)
        {
            return partidaActiva == null
                ? null
                : PartidaEstadoMapper.Convertir(partidaActiva);
        }
    }

    /// <summary>
    /// Ejecuta la operación establecer partida.
    /// </summary>
    /// <param name="partida">El valor de partida.</param>
    public void EstablecerPartida(Partida partida)
    {
        if (partida == null) throw new ArgumentNullException(nameof(partida));

        lock (sincronizacion)
        {
            partidaActiva = partida;
            RegistrarEventoSeguro("PARTIDA|EXITO|Partida establecida.");
        }
    }

    /// <summary>
    /// Guarda el progreso actual en progreso.txt (tecla F5).
    /// </summary>
    public ResultadoAccion GuardarProgreso()
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");

            if (servicioArchivos == null)
                return ResultadoAccion.Fallido("Sin persistencia configurada.");

            try
            {
                servicioArchivos.GuardarProgreso(
                    ProgresoPartida.Serializar(partidaActiva));
                RegistrarEventoSeguro("PROGRESO|EXITO|Progreso guardado.");
                return ResultadoAccion.Exitoso("Progreso guardado.");
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException)
            {
                return ResultadoAccion.Fallido(
                    "No se pudo guardar el progreso: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Restaura el progreso desde progreso.txt (tecla F9).
    /// </summary>
    public ResultadoAccion CargarProgreso()
    {
        string contenido;

        if (servicioArchivos == null)
            return ResultadoAccion.Fallido("Sin persistencia configurada.");

        try
        {
            contenido = servicioArchivos.LeerProgreso();
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException)
        {
            return ResultadoAccion.Fallido(
                "No se pudo leer el progreso: " + ex.Message);
        }

        Partida partida;

        try
        {
            partida = ProgresoPartida.Deserializar(contenido);
        }
        catch (Exception ex) when (
            ex is FormatException ||
            ex is ArgumentException ||
            ex is InvalidOperationException)
        {
            return ResultadoAccion.Fallido(
                "Progreso inválido: " + ex.Message);
        }

        lock (sincronizacion)
        {
            partidaActiva = partida;
            RegistrarEventoSeguro("PROGRESO|EXITO|Progreso cargado.");
            return ResultadoAccion.Exitoso("Progreso cargado.");
        }
    }

    /// <summary>
    /// Obtiene partida.
    /// </summary>
    /// <returns>Resultado de la operación.</returns>
    public Partida? ObtenerPartida()
    {
        lock (sincronizacion)
        {
            return partidaActiva;
        }
    }
    /// <summary>
    /// Obtiene unidad.
    /// </summary>
    /// <param name="id">El valor de id.</param>
    /// <returns>Resultado de la operación.</returns>
    public Unidad? ObtenerUnidad(Guid id)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return null;

            return partidaActiva.JugadorHumano.Unidades
                .FirstOrDefault(u => u.Id == id);
        }
    }

    /// <summary>
    /// Ejecuta un turno de la máquina (caza y guardia) bajo el lock global.
    /// Toda la decisión vive en InteligenciaMaquina.
    /// </summary>
    public ResultadoAccion EjecutarTurnoMaquina()
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");

            ResultadoAccion resultado =
                new InteligenciaMaquina().EjecutarTurno(partidaActiva);

            if (resultado.Exito &&
                resultado.Mensaje != null &&
                resultado.Mensaje.Contains("¡Victoria!"))
            {
                RegistrarEventoSeguro(
                    $"VICTORIA|EXITO|{partidaActiva.JugadorMaquina.Nombre} derrota a {partidaActiva.JugadorHumano.Nombre}.");
                GuardarResultadoFinalSeguro(
                    $"Ganador={partidaActiva.JugadorMaquina.Nombre}\n" +
                    $"Perdedor={partidaActiva.JugadorHumano.Nombre}\n" +
                    $"UnidadesRestantesMaquina={partidaActiva.JugadorMaquina.Unidades.Count}\n");
            }

            return RegistrarResultado("IA_MAQUINA", resultado);
        }
    }

    /// <summary>
    /// Posición actual de un objetivo enemigo (unidad o edificio).
    /// Null si no existe (destruido) o no hay partida.
    /// </summary>
    public Coordenada? ObtenerCoordenadaObjetivoEnemigo(Guid objetivoId)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return null;

            Unidad? unidad =
                partidaActiva.JugadorMaquina.Unidades
                    .FirstOrDefault(u => u.Id == objetivoId);

            if (unidad != null)
                return unidad.Coordenada;

            Edificio? edificio =
                partidaActiva.JugadorMaquina.Edificios
                    .FirstOrDefault(e => e.Id == objetivoId);

            return edificio?.Coordenada;
        }
    }

    /// <summary>
    /// Obtiene centro urbano.
    /// </summary>
    /// <param name="coordenada">El valor de coordenada.</param>
    /// <returns>Resultado de la operación.</returns>
    public Castillo? ObtenerCastillo(Coordenada coordenada)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null || coordenada == null)
                return null;

            return partidaActiva.JugadorHumano.Edificios
                .OfType<Castillo>()
                .FirstOrDefault(e =>
                    e.Coordenada.X == coordenada.X &&
                    e.Coordenada.Y == coordenada.Y);
        }
    }
    /// <summary>
    /// Ejecuta la operación hay partida activa.
    /// </summary>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool HayPartidaActiva()
    {
        lock (sincronizacion)
        {
            return partidaActiva != null;
        }
    }

    private ResultadoAccion RegistrarResultado(
        string accion,
        ResultadoAccion resultado)
    {
        string estado = resultado.Exito ? "EXITO" : "RECHAZADO";
        RegistrarEventoSeguro($"{accion}|{estado}|{resultado.Mensaje}");
        return resultado;
    }

    private void RegistrarEventoSeguro(string contenido)
    {
        if (servicioArchivos == null)
            return;

        try
        {
            servicioArchivos.RegistrarEvento(contenido);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(
                $"No se pudo escribir log_partida.txt: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(
                $"No se pudo escribir log_partida.txt: {ex.Message}");
        }
    }

    private void GuardarResultadoFinalSeguro(string contenido)
    {
        if (servicioArchivos == null)
            return;

        try
        {
            servicioArchivos.GuardarResultadoFinal(contenido);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(
                $"No se pudo escribir resultado_final.txt: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(
                $"No se pudo escribir resultado_final.txt: {ex.Message}");
        }
    }
}

