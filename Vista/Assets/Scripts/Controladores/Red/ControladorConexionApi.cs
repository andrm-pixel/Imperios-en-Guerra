using System.Collections;
using System.Collections.Generic;
using System.Text;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Vistas;
using UnityEngine;
using UnityEngine.Networking;

namespace ImperiosEnGuerra.Controladores.Red
{
    /// <summary>Puente entre la Vista Unity y la API interna o externa.</summary>
    public class ControladorConexionApi : MonoBehaviour
    {
        [SerializeField]
        /// <summary>URL base de la API externa de partida.</summary>
        private string urlBaseApi = "http://localhost:5086";

        [SerializeField]
        /// <summary>Vista que dibuja el estado de la partida.</summary>
        private VistaPartida vistaPartida;

        [SerializeField]
        /// <summary>HUD para recursos, mensajes y opciones.</summary>
        private VistaHud vistaHud;

        [Header("API interna (sin terminal)")]
        [SerializeField]
        /// <summary>API interna usada sin terminal externa.</summary>
        private ApiInterna.ApiInternaJuego apiInterna;

        [SerializeField]
        /// <summary>Usa la API HTTP externa en vez de la interna.</summary>
        private bool usarApiExterna = false;

        /// <summary>Costos vigentes para describir precios.</summary>
        private EconomiaEstadoDto economiaActual;

    /// <summary>Indica si hay un movimiento en curso.</summary>
    public bool MovimientoEnCurso { get; private set; }
    /// <summary>Indica si hay una recolección en curso.</summary>
    public bool RecoleccionEnCurso { get; private set; }
    /// <summary>Indica si hay una construcción en curso.</summary>
    public bool ConstruccionEnCurso { get; private set; }
    /// <summary>Indica si hay un entrenamiento en curso.</summary>
    public bool EntrenamientoEnCurso { get; private set; }
    /// <summary>Indica si hay un ataque en curso.</summary>
    public bool AtaqueEnCurso { get; private set; }

    /// <summary>Contador de movimientos concurrentes activos.</summary>
    private int movimientosActivos;
    /// <summary>Contador de recolecciones concurrentes activas.</summary>
    private int recoleccionesActivas;
    /// <summary>Contador de construcciones concurrentes activas.</summary>
    private int construccionesActivas;
    /// <summary>Contador de entrenamientos concurrentes activos.</summary>
    private int entrenamientosActivos;
    /// <summary>Contador de ataques concurrentes activos.</summary>
    private int ataquesActivos;

    /// <summary>Procesos activos por identificador de unidad.</summary>
    private readonly System.Collections.Generic.Dictionary<string, System.Guid> procesosPorUnidad =
        new System.Collections.Generic.Dictionary<string, System.Guid>();

    /// <summary>Asocia un proceso a su unidad.</summary>
    private void RegistrarProceso(string claveUnidad, System.Guid procesoId)
    {
        if (!string.IsNullOrWhiteSpace(claveUnidad))
            procesosPorUnidad[claveUnidad] = procesoId;
    }

    /// <summary>Elimina la asociación proceso-unidad.</summary>
    private void OlvidarProceso(string claveUnidad)
    {
        if (!string.IsNullOrWhiteSpace(claveUnidad))
            procesosPorUnidad.Remove(claveUnidad);
    }

    /// <summary>
    /// Cancela la orden activa de una unidad para sacarla de su tarea.
    /// Solo modo interno; sin API interna no hay nada que cancelar.
    /// </summary>
    public void CancelarOrdenesDe(string unidadId)
    {
        if (usarApiExterna)
        {
            MostrarError("La cancelación solo está disponible en modo interno.");
            return;
        }

        if (string.IsNullOrWhiteSpace(unidadId) ||
            !procesosPorUnidad.TryGetValue(unidadId, out System.Guid procesoId))
        {
            if (vistaHud != null)
                vistaHud.MostrarMensaje("La unidad no tiene una orden activa para cancelar.");
            return;
        }

        try
        {
            ExigirApiInterna();
            if (apiInterna.CancelarProceso(procesoId))
            {
                if (vistaHud != null)
                    vistaHud.MostrarMensaje("Orden cancelada. La unidad queda libre.");
            }
            else if (vistaHud != null)
            {
                vistaHud.MostrarMensaje("La orden ya había terminado.");
            }
        }
        catch (System.Exception ex)
        {
            MostrarError(ex.Message);
        }
        finally
        {
            OlvidarProceso(unidadId);
        }
    }

    /// <summary>
    /// Guarda el progreso en progreso.txt (tecla F5). Solo modo interno.
    /// </summary>
    public void GuardarProgreso()
    {
        if (usarApiExterna)
        {
            MostrarError("El guardado solo está disponible en modo interno.");
            return;
        }

        try
        {
            ExigirApiInterna();
            string mensaje = apiInterna.GuardarProgreso();

            if (vistaHud != null)
                vistaHud.MostrarMensaje(mensaje);
        }
        catch (System.Exception ex)
        {
            MostrarError(ex.Message);
        }
    }

    /// <summary>
    /// Carga el progreso desde progreso.txt (tecla F9). Solo modo interno.
    /// </summary>
    public void CargarProgreso()
    {
        if (usarApiExterna)
        {
            MostrarError("La carga solo está disponible en modo interno.");
            return;
        }

        StartCoroutine(CargarProgresoInterno());
    }

    private IEnumerator CargarProgresoInterno()
    {
        string mensaje = string.Empty;

        try
        {
            ExigirApiInterna();
            mensaje = apiInterna.CargarProgreso();
        }
        catch (System.Exception ex)
        {
            MostrarError(ex.Message);
            yield break;
        }

        yield return SincronizarEstadoInterno(
            string.IsNullOrWhiteSpace(mensaje) ? "Progreso cargado." : mensaje);
    }

/// <summary>Indica si alguna acción concurrente sigue activa.</summary>
public bool AccionEnCurso =>
    MovimientoEnCurso ||
    RecoleccionEnCurso ||
    ConstruccionEnCurso ||
    EntrenamientoEnCurso ||
    AtaqueEnCurso;

/// <summary>Indica si se puede iniciar un movimiento.</summary>
public bool PuedeIniciarMovimiento =>
    usarApiExterna ? isActiveAndEnabled : ApiInternaDisponible;

/// <summary>Indica si se puede iniciar una recolección.</summary>
public bool PuedeIniciarRecoleccion =>
    usarApiExterna ? isActiveAndEnabled : ApiInternaDisponible;

/// <summary>Indica si se puede iniciar una construcción.</summary>
public bool PuedeIniciarConstruccion =>
    usarApiExterna ? isActiveAndEnabled : ApiInternaDisponible;

/// <summary>Indica si se puede iniciar un entrenamiento.</summary>
public bool PuedeIniciarEntrenamiento =>
    usarApiExterna ? isActiveAndEnabled : ApiInternaDisponible;

/// <summary>Indica si se puede iniciar un ataque.</summary>
public bool PuedeIniciarAtaque =>
    usarApiExterna ? isActiveAndEnabled : ApiInternaDisponible;

        /// <summary>Indica si la API interna está lista.</summary>
        private bool ApiInternaDisponible
        {
            get { return apiInterna != null && apiInterna.EstaDisponible; }
        }

        /// <summary>Lanza error si la API interna no está disponible.</summary>
        private void ExigirApiInterna()
        {
            if (ApiInternaDisponible)
                return;
            throw new System.InvalidOperationException("La API interna no está disponible.");
        }

        /// <summary>
        /// Interrumpe la orden activa de una unidad y espera a que quede
        /// disponible para recibir la nueva. Puente sin hilos.
        /// </summary>
        private IEnumerator SustituirOrden(string unidadId)
        {
            if (string.IsNullOrWhiteSpace(unidadId) ||
                !procesosPorUnidad.TryGetValue(unidadId, out System.Guid anterior))
            {
                yield break;
            }

            try
            {
                ExigirApiInterna();
                apiInterna.CancelarProceso(anterior);
            }
            catch (System.Exception ex)
            {
                MostrarError(ex.Message);
                yield break;
            }
            finally
            {
                OlvidarProceso(unidadId);
            }

            const int maximoIntentos = 30;
            for (int i = 0; i < maximoIntentos && isActiveAndEnabled; i++)
            {
                bool disponible = false;
                try
                {
                    ExigirApiInterna();
                    UnidadEstadoDto unidad =
                        BuscarUnidadHumana(apiInterna.ObtenerEstado(), unidadId);
                    disponible = unidad != null && unidad.disponible;
                }
                catch
                {
                    yield break;
                }

                if (disponible)
                    yield break;

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        /// <summary>Solicita el movimiento de una unidad.</summary>
        public void MoverUnidad(string unidadId, int x, int y)
        {
            if (!PuedeIniciarMovimiento)
            {
                MostrarError("La conexión con la API no está disponible.");
                return;
            }

            if (!usarApiExterna)
            {
                StartCoroutine(EjecutarMovimientoInterno(unidadId, x, y));
                return;
            }

            StartCoroutine(EnviarMovimiento(new MoverUnidadDto
            {
                unidadId = unidadId,
                destino = new CoordenadaDto(x, y)
            }));
        }

        /// <summary>Solicita la recolección de un aldeano.</summary>
        public void IniciarRecoleccion(string aldeanoId, int x, int y)
        {
            if (!PuedeIniciarRecoleccion)
            {
                MostrarError("La conexión con la API no está disponible.");
                return;
            }

            if (!usarApiExterna)
            {
                StartCoroutine(EjecutarRecoleccionInterna(aldeanoId, x, y));
                return;
            }

            StartCoroutine(EnviarRecoleccion(new RecolectarDto
            {
                aldeanoId = aldeanoId,
                objetivo = new CoordenadaDto(x, y)
            }));
        }

        /// <summary>Solicita la construcción de un edificio.</summary>
        public void Construir(
        string aldeanoId,
        string tipoEdificio,
        int x,
        int y)
    {
        if (!PuedeIniciarConstruccion)
        {
            MostrarError(
                "La conexión con la API no está disponible.");
            return;
        }

        if (!usarApiExterna)
        {
            StartCoroutine(EjecutarConstruccionInterna(aldeanoId, tipoEdificio, x, y));
            return;
        }

        StartCoroutine(
            EnviarConstruccion(
                new ConstruirDto
                {
                    aldeanoId = aldeanoId,
                    tipoEdificio = tipoEdificio,
                    destino = new CoordenadaDto(x, y)
                }));
    }

        /// <summary>Solicita el entrenamiento de una unidad.</summary>
        public void Entrenar(
            int edificioX,
            int edificioY,
            string tipoUnidad,
            int destinoX,
            int destinoY)
        {
            if (!PuedeIniciarEntrenamiento)
            {
                MostrarError(
                    "La conexión con la API no está disponible.");
                return;
            }

            if (!usarApiExterna)
            {
                StartCoroutine(EjecutarEntrenamientoInterno(edificioX, edificioY, tipoUnidad, destinoX, destinoY));
                return;
            }

            StartCoroutine(
                EnviarEntrenamiento(
                    new EntrenarDto
                    {
                        edificioOrigen =
                            new CoordenadaDto(
                                edificioX,
                                edificioY),

                        tipoUnidad = tipoUnidad,

                        destino =
                            new CoordenadaDto(
                                destinoX,
                                destinoY)
                    }));
        }

        /// <summary>Solicita un ataque a un objetivo.</summary>
        public void Atacar(
            string atacanteId,
            string objetivoId)
        {
            if (!PuedeIniciarAtaque)
            {
                MostrarError(
                    "La conexión con la API no está disponible.");
                return;
            }

            if (!usarApiExterna)
            {
                StartCoroutine(EjecutarAtaqueInterno(atacanteId, objetivoId));
                return;
            }

            StartCoroutine(
                EnviarAtaque(
                    new AtaqueDto
                    {
                        atacanteId = atacanteId,
                        objetivoId = objetivoId
                    }));
        }

        /// <summary>
        /// Ordena a todo el ejército atacar sin detener entrenamiento,
        /// recolección ni construcción (cada unidad pelea en su worker).
        /// Solo modo interno.
        /// </summary>
        public void IniciarBatalla()
        {
            if (!PuedeIniciarAtaque)
            {
                MostrarError(
                    "La conexión con la API no está disponible.");
                return;
            }

            if (!usarApiExterna)
            {
                StartCoroutine(EjecutarBatallaInterna());
                return;
            }

            MostrarError(
                "La batalla total solo está disponible en modo interno.");
        }

        private IEnumerator EjecutarBatallaInterna()
        {
            IReadOnlyList<NucleoBatalla.ProcesoBatalla> ordenes = null;

            try
            {
                ExigirApiInterna();

                foreach (var pendiente in new System.Collections.Generic.List<string>(
                    procesosPorUnidad.Keys))
                {
                    if (procesosPorUnidad.TryGetValue(
                        pendiente,
                        out System.Guid previo))
                    {
                        apiInterna.CancelarProceso(previo);
                        OlvidarProceso(pendiente);
                    }
                }

                ordenes = apiInterna.IniciarBatalla();
            }
            catch (System.Exception ex)
            {
                MostrarError(ex.Message);
                yield break;
            }

            if (ordenes == null || ordenes.Count == 0)
            {
                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "No hay tropas para la batalla. Entrena guerreros o arqueros.");
                }

                yield break;
            }

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    $"¡A la batalla! {ordenes.Count} unidades atacan. Aldeanos y obras siguen en curso.");
            }

            foreach (var orden in ordenes)
            {
                string clave = orden.UnidadId.ToString("D");
                RegistrarProceso(clave, orden.ProcesoId);
                StartCoroutine(SeguirProcesoBatalla(orden.ProcesoId, clave));
            }
        }

        private IEnumerator SeguirProcesoBatalla(
            System.Guid procesoId,
            string unidadId)
        {
            yield return EsperarProcesoInterno(procesoId, unidadId, "Batalla");
            OlvidarProceso(unidadId);
        }

        /// <summary>Ejecuta un movimiento con la API interna.</summary>
        private IEnumerator EjecutarMovimientoInterno(string unidadId, int x, int y)
        {
            movimientosActivos++;
            MovimientoEnCurso = movimientosActivos > 0;
            try
            {
                yield return SustituirOrden(unidadId);
                System.Guid procesoId;
                try
                {
                    ExigirApiInterna();
                    procesoId = apiInterna.IniciarMovimiento(unidadId, x, y);
                }
                catch (System.Exception ex)
                {
                    MostrarError(ex.Message);
                    yield break;
                }
                RegistrarProceso(unidadId, procesoId);
                yield return EsperarProcesoInterno(procesoId, unidadId, "Movimiento");
            }
            finally
            {
                OlvidarProceso(unidadId);
                movimientosActivos = Mathf.Max(0, movimientosActivos - 1);
                MovimientoEnCurso = movimientosActivos > 0;
            }
        }

        /// <summary>Ejecuta una recolección con la API interna.</summary>
        private IEnumerator EjecutarRecoleccionInterna(string aldeanoId, int x, int y)
        {
            recoleccionesActivas++;
            RecoleccionEnCurso = recoleccionesActivas > 0;
            try
            {
                yield return SustituirOrden(aldeanoId);
                System.Guid procesoId;
                try
                {
                    ExigirApiInterna();
                    procesoId = apiInterna.IniciarRecoleccion(aldeanoId, x, y);
                }
                catch (System.Exception ex)
                {
                    MostrarError(ex.Message);
                    yield break;
                }
                RegistrarProceso(aldeanoId, procesoId);
                yield return EsperarProcesoInterno(procesoId, aldeanoId, "Recolección");
            }
            finally
            {
                OlvidarProceso(aldeanoId);
                recoleccionesActivas = Mathf.Max(0, recoleccionesActivas - 1);
                RecoleccionEnCurso = recoleccionesActivas > 0;
            }
        }

        /// <summary>Ejecuta una construcción con la API interna.</summary>
        private IEnumerator EjecutarConstruccionInterna(string aldeanoId, string tipoEdificio, int x, int y)
        {
            construccionesActivas++;
            ConstruccionEnCurso = construccionesActivas > 0;
            try
            {
                yield return SustituirOrden(aldeanoId);
                System.Guid procesoId;
                try
                {
                    ExigirApiInterna();
                    procesoId = apiInterna.IniciarConstruccion(aldeanoId, tipoEdificio, x, y);
                }
                catch (System.Exception ex)
                {
                    MostrarError(ex.Message);
                    yield break;
                }
                RegistrarProceso(aldeanoId, procesoId);
                yield return EsperarProcesoInterno(procesoId, aldeanoId, "Construcción");
            }
            finally
            {
                OlvidarProceso(aldeanoId);
                construccionesActivas = Mathf.Max(0, construccionesActivas - 1);
                ConstruccionEnCurso = construccionesActivas > 0;
            }
        }

        /// <summary>Ejecuta un entrenamiento con la API interna.</summary>
        private IEnumerator EjecutarEntrenamientoInterno(int edificioX, int edificioY, string tipoUnidad, int destinoX, int destinoY)
        {
            entrenamientosActivos++;
            EntrenamientoEnCurso = entrenamientosActivos > 0;
            try
            {
                System.Guid procesoId;
                try
                {
                    ExigirApiInterna();
                    procesoId = apiInterna.IniciarEntrenamiento(edificioX, edificioY, tipoUnidad, destinoX, destinoY);
                }
                catch (System.Exception ex)
                {
                    MostrarError(ex.Message);
                    yield break;
                }
                yield return EsperarProcesoInterno(procesoId, null, "Entrenamiento");
            }
            finally
            {
                entrenamientosActivos = Mathf.Max(0, entrenamientosActivos - 1);
                EntrenamientoEnCurso = entrenamientosActivos > 0;
            }
        }

        /// <summary>Ejecuta un ataque con la API interna.</summary>
        private IEnumerator EjecutarAtaqueInterno(string atacanteId, string objetivoId)
        {
            ataquesActivos++;
            AtaqueEnCurso = ataquesActivos > 0;
            try
            {
                yield return SustituirOrden(atacanteId);
                System.Guid procesoId;
                try
                {
                    ExigirApiInterna();
                    procesoId = apiInterna.IniciarAtaque(atacanteId, objetivoId);
                }
                catch (System.Exception ex)
                {
                    MostrarError(ex.Message);
                    yield break;
                }
                RegistrarProceso(atacanteId, procesoId);
                yield return EsperarProcesoInterno(procesoId, atacanteId, "Ataque");
            }
            finally
            {
                OlvidarProceso(atacanteId);
                ataquesActivos = Mathf.Max(0, ataquesActivos - 1);
                AtaqueEnCurso = ataquesActivos > 0;
            }
        }

        /// <summary>Espera el resultado interno y sincroniza la vista.</summary>
        private IEnumerator EsperarProcesoInterno(System.Guid procesoId, string unidadId, string nombre)
        {
            const float intervalo = 0.1f;
            while (isActiveAndEnabled)
            {
                ResultadoProcesoDto resultado = null;
                bool listo = false;
                try
                {
                    ExigirApiInterna();
                    listo = apiInterna.IntentarObtenerResultado(procesoId, out resultado);
                }
                catch (System.Exception ex)
                {
                    MostrarError(ex.Message);
                    yield break;
                }
                if (!listo || resultado == null)
                {
                    yield return ActualizarSnapshotInterno(unidadId);
                    yield return new WaitForSecondsRealtime(intervalo);
                    continue;
                }
                if (resultado.estado == "Cancelado")
                {
                    Debug.Log($"{nombre} cancelado.");
                    yield return SincronizarEstadoInterno();
                    yield break;
                }
                if (resultado.estado == "Fallido")
                {
                    MostrarError(string.IsNullOrWhiteSpace(resultado.errorTecnico)
                        ? $"El worker de {nombre.ToLower()} finalizó con error."
                        : resultado.errorTecnico);
                    yield return SincronizarEstadoInterno();
                    yield break;
                }
                if (resultado.estado != "Completado")
                {
                    MostrarError($"Estado concurrente no reconocido: {resultado.estado}");
                    yield return SincronizarEstadoInterno();
                    yield break;
                }
                if (!resultado.exito)
                {
                    MostrarError(string.IsNullOrWhiteSpace(resultado.mensaje)
                        ? $"La acción fue rechazada por el Modelo."
                        : resultado.mensaje);
                    yield return SincronizarEstadoInterno();
                    yield break;
                }
                Debug.Log($"{nombre} ejecutado por worker {resultado.hiloTrabajoId}.");
                yield return SincronizarEstadoInterno(
                    string.IsNullOrWhiteSpace(resultado.mensaje) ? $"{nombre} realizado." : resultado.mensaje);
                yield break;
            }
        }

        /// <summary>Refresca la unidad y los recursos durante el proceso.</summary>
        private IEnumerator ActualizarSnapshotInterno(string unidadId)
        {
            EstadoPartidaDto estado = null;
            try
            {
                ExigirApiInterna();
                estado = apiInterna.ObtenerEstado();
            }
            catch
            {
                yield break;
            }
            if (!string.IsNullOrWhiteSpace(unidadId))
            {
                UnidadEstadoDto unidad = BuscarUnidadHumana(estado, unidadId);
                if (unidad?.coordenada != null && vistaPartida != null)
                    vistaPartida.ActualizarMovimientoUnidad(unidad.id, unidad.coordenada.x, unidad.coordenada.y, unidad.estado, unidad.ordenActiva);
                var recursos = estado?.jugadorHumano?.recursos;
                if (vistaHud != null && recursos != null)
                {
                    vistaHud.MostrarRecursos(recursos.oro, recursos.madera, recursos.comida, recursos.piedra, recursos.hierro);
                }
                yield break;
            }
            var recursosSolo = estado?.jugadorHumano?.recursos;
            if (vistaHud != null && recursosSolo != null)
                vistaHud.MostrarRecursos(recursosSolo.oro, recursosSolo.madera, recursosSolo.comida, recursosSolo.piedra, recursosSolo.hierro);
        }

        /// <summary>Obtiene el estado interno y lo aplica a la vista.</summary>
        private IEnumerator SincronizarEstadoInterno(string mensaje = "")
        {
            EstadoPartidaDto estado = null;
            try
            {
                ExigirApiInterna();
                estado = apiInterna.ObtenerEstado();
            }
            catch (System.Exception ex)
            {
                MostrarError(ex.Message);
                yield break;
            }
            AplicarEstado(estado, mensaje);
        }

        /// <summary>Valida y dibuja un estado en vista y HUD.</summary>
        private void AplicarEstado(EstadoPartidaDto estado, string mensaje)
        {
            if (estado?.mapa == null || estado.mapa.ancho <= 0 || estado.jugadorHumano == null || estado.jugadorMaquina == null)
            {
                MostrarError("La API interna devolvió un estado incompleto.");
                return;
            }
            if (vistaPartida == null)
            {
                MostrarError("VistaPartida no está configurada en ControladorAPI.");
                return;
            }
            economiaActual = estado.economia;
            vistaPartida.Sincronizar(estado);
            if (vistaHud != null)
            {
                var recursos = estado.jugadorHumano.recursos;
                if (recursos != null)
                {
                    vistaHud.MostrarRecursos(recursos.oro, recursos.madera, recursos.comida, recursos.piedra, recursos.hierro);
                    if (!string.IsNullOrEmpty(mensaje))
                        vistaHud.MostrarMensaje(mensaje);
                }
            }
        }

        /// <summary>Detiene corrutinas y reinicia los contadores.</summary>
        private void OnDisable()
        {
            StopAllCoroutines();

            movimientosActivos = 0;
            recoleccionesActivas = 0;
            construccionesActivas = 0;
            entrenamientosActivos = 0;
            ataquesActivos = 0;

            MovimientoEnCurso = false;
            RecoleccionEnCurso = false;
            ConstruccionEnCurso = false;
            EntrenamientoEnCurso = false;
            AtaqueEnCurso = false;
        }

        /// <summary>Envía un movimiento a la API externa.</summary>
        private IEnumerator EnviarMovimiento(MoverUnidadDto movimiento)
        {
            movimientosActivos++;
            MovimientoEnCurso = movimientosActivos > 0;

            try
            {
                using var request = new UnityWebRequest(
                    $"{urlBaseApi}/api/partida/mover-concurrente",
                    UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(
                        JsonUtility.ToJson(movimiento)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar el movimiento concurrente. HTTP {request.responseCode}: {request.error}");

                    yield break;
                }

                ProcesoIniciadoDto proceso =
                    LeerProcesoIniciado(request.downloadHandler.text);

                if (proceso == null ||
                    string.IsNullOrWhiteSpace(proceso.procesoId))
                {
                    MostrarError(
                        "La API no devolvió un identificador válido para el movimiento concurrente.");

                    yield break;
                }

                yield return EsperarResultadoMovimiento(
                    proceso.procesoId,
                    movimiento.unidadId);
            }
            finally
            {
                movimientosActivos =
                    Mathf.Max(0, movimientosActivos - 1);

                MovimientoEnCurso =
                    movimientosActivos > 0;
            }
        }

        /// <summary>Consulta el worker de movimiento hasta completarlo.</summary>
        private IEnumerator EsperarResultadoMovimiento(
            string procesoId,
            string unidadId)
        {
            const float intervaloConsulta = 0.1f;
            while (isActiveAndEnabled)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/{procesoId}/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        $"Consulta temporal de movimiento fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    yield return ActualizarMovimientoEnCurso(
                        unidadId);

                    yield return new WaitForSecondsRealtime(
                        intervaloConsulta);
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto al movimiento esperado.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log("Movimiento cancelado.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de movimiento finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "El movimiento fue rechazado por el Modelo."
                            : resultado.mensaje);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                Debug.Log(
                    $"Movimiento ejecutado por worker " +
                    $"{resultado.hiloTrabajoId}.");

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Movimiento realizado."
                        : resultado.mensaje,
                    "Movimiento completado, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }

            Debug.Log(
                "Seguimiento concurrente detenido porque el controlador dejó de estar activo.");
        }

        /// <summary>Refresca la posición durante el movimiento externo.</summary>
        private IEnumerator ActualizarMovimientoEnCurso(
            string unidadId)
        {
            if (vistaPartida == null ||
                string.IsNullOrWhiteSpace(unidadId))
            {
                yield break;
            }

            using UnityWebRequest request =
                UnityWebRequest.Get(
                    $"{urlBaseApi}/api/partida");

            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    $"No se pudo actualizar el snapshot de movimiento: {request.error}",
                    this);

                yield break;
            }

            EstadoPartidaDto estado;

            try
            {
                estado =
                    JsonUtility.FromJson<EstadoPartidaDto>(
                        request.downloadHandler.text);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning(
                    $"Snapshot de movimiento inválido: {ex.Message}",
                    this);

                yield break;
            }

            UnidadEstadoDto unidad =
                BuscarUnidadHumana(
                    estado,
                    unidadId);

            if (unidad?.coordenada == null)
            {
                yield break;
            }

            vistaPartida.ActualizarMovimientoUnidad(
                unidad.id,
                unidad.coordenada.x,
                unidad.coordenada.y,
                unidad.estado,
                unidad.ordenActiva);
        }

        /// <summary>Busca una unidad humana por identificador.</summary>
        private static UnidadEstadoDto BuscarUnidadHumana(
            EstadoPartidaDto estado,
            string unidadId)
        {
            UnidadEstadoDto[] unidades =
                estado?.jugadorHumano?.unidades;

            if (unidades == null)
            {
                return null;
            }

            foreach (UnidadEstadoDto unidad in unidades)
            {
                if (unidad != null &&
                    unidad.id == unidadId)
                {
                    return unidad;
                }
            }

            return null;
        }

        /// <summary>Lee el inicio de proceso desde JSON.</summary>
        private static ProcesoIniciadoDto LeerProcesoIniciado(
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<ProcesoIniciadoDto>(json);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning(
                    $"La respuesta de inicio concurrente no es JSON válido: {ex.Message}");

                return null;
            }
        }

        /// <summary>Lee el resultado concurrente desde JSON.</summary>
        private static ResultadoProcesoDto LeerResultadoProceso(
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<ResultadoProcesoDto>(json);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning(
                    $"La respuesta concurrente no es JSON válido: {ex.Message}");

                return null;
            }
        }

        /// <summary>Envía una recolección a la API externa.</summary>
        private IEnumerator EnviarRecoleccion(RecolectarDto recoleccion)
        {
            recoleccionesActivas++;
            RecoleccionEnCurso = recoleccionesActivas > 0;

            try
            {
                using var request = new UnityWebRequest(
                    $"{urlBaseApi}/api/partida/recolectar-concurrente",
                    UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(
                        JsonUtility.ToJson(recoleccion)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar la recolección concurrente. HTTP {request.responseCode}: {request.error}");
                    yield break;
                }

                ProcesoIniciadoDto proceso =
                    LeerProcesoIniciado(request.downloadHandler.text);

                if (proceso == null ||
                    string.IsNullOrWhiteSpace(proceso.procesoId))
                {
                    MostrarError(
                        "La API no devolvió un identificador válido para la recolección concurrente.");
                    yield break;
                }

                yield return EsperarResultadoRecoleccion(
                    proceso.procesoId,
                    recoleccion.aldeanoId);
            }
            finally
            {
                recoleccionesActivas =
                    Mathf.Max(0, recoleccionesActivas - 1);

                RecoleccionEnCurso =
                    recoleccionesActivas > 0;
            }
        }

        /// <summary>Consulta el worker de recolección hasta completarlo.</summary>
        private IEnumerator EsperarResultadoRecoleccion(
            string procesoId,
            string unidadId)
        {
            const float intervaloConsulta = 0.1f;
            // La recolección orgánica incluye desplazamiento y varios ciclos
            // de carga, por lo que puede superar el límite anterior de 15 s.
            while (isActiveAndEnabled)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/{procesoId}/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        $"Consulta temporal de recolección fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    // La recolección también contiene una fase de movimiento.
                    // Consumimos snapshots intermedios para que Unity represente
                    // cada paso en vez de saltar a la posición final.
                    yield return ActualizarRecoleccionEnCurso(
                        unidadId);

                    yield return new WaitForSecondsRealtime(
                        intervaloConsulta);
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto a la recolección esperada.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log(
                        "Recolección cancelada.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de recolección finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "La recolección fue rechazada por el Modelo."
                            : resultado.mensaje);
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                Debug.Log(
                    $"Recolección ejecutada por worker " +
                    $"{resultado.hiloTrabajoId}.");

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Recolección preparada."
                        : resultado.mensaje,
                    "Recolección completada, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }

            Debug.Log(
                "Seguimiento concurrente detenido porque el controlador dejó de estar activo.");
        }

        /// <summary>Refresca posición y recursos durante la recolección.</summary>
        private IEnumerator ActualizarRecoleccionEnCurso(
            string unidadId)
        {
            if (vistaPartida == null ||
                string.IsNullOrWhiteSpace(unidadId))
            {
                yield break;
            }

            using UnityWebRequest request =
                UnityWebRequest.Get(
                    $"{urlBaseApi}/api/partida");

            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                yield break;
            }

            EstadoPartidaDto estado;

            try
            {
                estado =
                    JsonUtility.FromJson<EstadoPartidaDto>(
                        request.downloadHandler.text);
            }
            catch (System.ArgumentException)
            {
                yield break;
            }

            UnidadEstadoDto unidad =
                BuscarUnidadHumana(
                    estado,
                    unidadId);

            if (unidad?.coordenada != null)
            {
                vistaPartida.ActualizarMovimientoUnidad(
                    unidad.id,
                    unidad.coordenada.x,
                    unidad.coordenada.y,
                    unidad.estado,
                    unidad.ordenActiva);
            }

            var recursos =
                estado?.jugadorHumano?.recursos;

            if (vistaHud != null &&
                recursos != null)
            {
                vistaHud.MostrarRecursos(
                    recursos.oro,
                    recursos.madera,
                    recursos.comida,
                    recursos.piedra,
                    recursos.hierro);

            }
        }

        /// <summary>Envía una construcción a la API externa.</summary>
        private IEnumerator EnviarConstruccion(
            ConstruirDto construccion)
        {
            construccionesActivas++;
            ConstruccionEnCurso = construccionesActivas > 0;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/construir-concurrente",
                        UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler =
                    new UploadHandlerRaw(
                        Encoding.UTF8.GetBytes(
                            JsonUtility.ToJson(construccion)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar la construcción concurrente. HTTP {request.responseCode}: {request.error}");

                    yield break;
                }

                ProcesoIniciadoDto proceso =
                    LeerProcesoIniciado(
                        request.downloadHandler.text);

                if (proceso == null ||
                    string.IsNullOrWhiteSpace(
                        proceso.procesoId))
                {
                    MostrarError(
                        "La API no devolvió un identificador válido para la construcción concurrente.");

                    yield break;
                }

                yield return EsperarResultadoConstruccion(
                    proceso.procesoId);
            }
            finally
            {
                construccionesActivas =
                    Mathf.Max(0, construccionesActivas - 1);

                ConstruccionEnCurso =
                    construccionesActivas > 0;
            }
        }

        /// <summary>Consulta el worker de construcción hasta completarlo.</summary>
        private IEnumerator EsperarResultadoConstruccion(
            string procesoId)
        {
            const float intervaloConsulta = 0.1f;
            while (isActiveAndEnabled)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/{procesoId}/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        $"Consulta temporal de construcción fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    yield return ObtenerPartidaActiva(
                        "",
                        "",
                        false);

                    yield return new WaitForSecondsRealtime(
                        0.5f);
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto a la construcción esperada.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log("Construcción cancelada.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de construcción finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "La construcción fue rechazada por el Modelo."
                            : resultado.mensaje);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                Debug.Log(
                    $"Construcción ejecutada por worker " +
                    $"{resultado.hiloTrabajoId}.");

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Construcción realizada."
                        : resultado.mensaje,
                    "Construcción completada, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }

            Debug.Log(
                "Seguimiento concurrente detenido porque el controlador dejó de estar activo.");
        }
        

        /// <summary>Envía un entrenamiento a la API externa.</summary>
        private IEnumerator EnviarEntrenamiento(
            EntrenarDto entrenamiento)
        {
            entrenamientosActivos++;
            EntrenamientoEnCurso = entrenamientosActivos > 0;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/entrenar-concurrente",
                        UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler =
                    new UploadHandlerRaw(
                        Encoding.UTF8.GetBytes(
                            JsonUtility.ToJson(entrenamiento)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        $"No se pudo iniciar el entrenamiento concurrente. HTTP {request.responseCode}: {request.error}");

                    yield break;
                }

                ProcesoIniciadoDto proceso =
                    LeerProcesoIniciado(
                        request.downloadHandler.text);

                if (proceso == null ||
                    string.IsNullOrWhiteSpace(
                        proceso.procesoId))
                {
                    MostrarError(
                        "La API no devolvió un identificador válido para el entrenamiento concurrente.");

                    yield break;
                }

                yield return EsperarResultadoEntrenamiento(
                    proceso.procesoId,
                    entrenamiento.edificioOrigen);
            }
            finally
            {
                entrenamientosActivos =
                    Mathf.Max(0, entrenamientosActivos - 1);

                EntrenamientoEnCurso =
                    entrenamientosActivos > 0;
            }
        }

        /// <summary>Consulta el worker de entrenamiento hasta completarlo.</summary>
        private IEnumerator EsperarResultadoEntrenamiento(
            string procesoId,
            CoordenadaDto edificioOrigen)
        {
            const float intervaloConsulta = 0.25f;
            while (isActiveAndEnabled)
            {
                using UnityWebRequest request =
                    UnityWebRequest.Get(
                        $"{urlBaseApi}/api/procesos/{procesoId}/resultado");

                request.timeout = 5;

                yield return request.SendWebRequest();

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        $"Consulta temporal de entrenamiento fallida. Se reintentará: " +
                        $"HTTP {request.responseCode}: {request.error}",
                        this);

                    yield return new WaitForSecondsRealtime(0.5f);
                    continue;
                }

                if (request.responseCode == 204 ||
                    string.IsNullOrWhiteSpace(
                        request.downloadHandler.text))
                {
                    yield return ActualizarEntrenamientoEnCurso(
                        edificioOrigen);

                    yield return new WaitForSecondsRealtime(
                        intervaloConsulta);
                    continue;
                }

                ResultadoProcesoDto resultado =
                    LeerResultadoProceso(
                        request.downloadHandler.text);

                if (resultado == null)
                {
                    MostrarError(
                        "La API devolvió un resultado concurrente inválido.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.procesoId != procesoId)
                {
                    MostrarError(
                        "Se recibió el resultado de un proceso distinto al entrenamiento esperado.");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Cancelado")
                {
                    Debug.Log("Entrenamiento cancelado.");
                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado == "Fallido")
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.errorTecnico)
                            ? "El worker de entrenamiento finalizó con error."
                            : resultado.errorTecnico);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (resultado.estado != "Completado")
                {
                    MostrarError(
                        $"Estado concurrente no reconocido: {resultado.estado}");

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                if (!resultado.exito)
                {
                    MostrarError(
                        string.IsNullOrWhiteSpace(
                            resultado.mensaje)
                            ? "El entrenamiento fue rechazado por el Modelo."
                            : resultado.mensaje);

                    yield return SincronizarEstadoDespuesDeProceso();
                    yield break;
                }

                Debug.Log(
                    $"Entrenamiento ejecutado por worker " +
                    $"{resultado.hiloTrabajoId}.");

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Entrenamiento realizado."
                        : resultado.mensaje,
                    "Entrenamiento completado, pero no se pudo actualizar la vista. ",
                    false);

                yield break;
            }

            Debug.Log(
                "Seguimiento concurrente detenido porque el controlador dejó de estar activo.");
        }

        /// <summary>Refresca los recursos durante el entrenamiento.</summary>
        private IEnumerator ActualizarEntrenamientoEnCurso(
            CoordenadaDto edificioOrigen)
        {
            if (edificioOrigen == null)
                yield break;

            using UnityWebRequest request =
                UnityWebRequest.Get(
                    $"{urlBaseApi}/api/partida");

            request.timeout = 5;

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                yield break;
            }

            EstadoPartidaDto estado;

            try
            {
                estado =
                    JsonUtility.FromJson<EstadoPartidaDto>(
                        request.downloadHandler.text);
            }
            catch (System.ArgumentException)
            {
                yield break;
            }

            var recursos =
                estado?.jugadorHumano?.recursos;

            if (vistaHud != null &&
                recursos != null)
            {
                vistaHud.MostrarRecursos(
                    recursos.oro,
                    recursos.madera,
                    recursos.comida,
                    recursos.piedra,
                    recursos.hierro);
            }
        }

        /// <summary>Envía un ataque a la API externa.</summary>
        private IEnumerator EnviarAtaque(
            AtaqueDto ataque)
        {
            ataquesActivos++;
            AtaqueEnCurso = ataquesActivos > 0;

            try
            {
                using var request =
                    new UnityWebRequest(
                        $"{urlBaseApi}/api/partida/atacar",
                        UnityWebRequest.kHttpVerbPOST);

                request.uploadHandler =
                    new UploadHandlerRaw(
                        Encoding.UTF8.GetBytes(
                            JsonUtility.ToJson(ataque)));

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.timeout = 15;

                yield return request.SendWebRequest();

                ResultadoAccionDto resultado =
                    LeerResultado(
                        request.downloadHandler.text);

                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            $"No se pudo preparar el ataque. HTTP {request.responseCode}: {request.error}"));

                    yield break;
                }

                if (resultado == null ||
                    !resultado.exito)
                {
                    MostrarError(
                        MensajeError(
                            resultado,
                            "La API no confirmó el ataque."));

                    yield break;
                }

                yield return ObtenerPartidaActiva(
                    string.IsNullOrWhiteSpace(
                        resultado.mensaje)
                        ? "Ataque preparado."
                        : resultado.mensaje,
                    "Ataque aceptado, pero no se pudo actualizar la vista. ",
                    false);
            }
            finally
            {
                ataquesActivos =
                    Mathf.Max(0, ataquesActivos - 1);

                AtaqueEnCurso =
                    ataquesActivos > 0;
            }
        }

        /// <summary>Sincroniza la vista tras fallo o cancelación.</summary>
        private IEnumerator SincronizarEstadoDespuesDeProceso()
        {
            // Los workers limpian OrdenActiva/Estado en sus bloques finally.
            // Esta sincronización evita que Unity conserve una copia visual
            // antigua (por ejemplo Estado=Moviendo) después de un fallo,
            // cancelación o rechazo del Modelo.
            yield return ObtenerPartidaActiva(
                "",
                "",
                false);
        }

        /// <summary>Lee un resultado simple desde JSON.</summary>
        private static ResultadoAccionDto LeerResultado(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<ResultadoAccionDto>(json);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning(
                    $"La respuesta de la API no es JSON válido: {ex.Message}");

                return null;
            }
        }

        /// <summary>Elige el mensaje de error más descriptivo.</summary>
        private static string MensajeError(
            ResultadoAccionDto resultado,
            string alternativa)
        {
            if (!string.IsNullOrWhiteSpace(resultado?.mensaje))
                return resultado.mensaje;

            if (!string.IsNullOrWhiteSpace(resultado?.error))
                return resultado.error;

            return alternativa;
        }

        /// <summary>Describe el costo del centro urbano.</summary>
        public string DescribirCostoConstruccion()
        {
            return DescribirCosto(
                economiaActual?.castillo);
        }

        /// <summary>Describe el costo de un tipo de unidad.</summary>
        public string DescribirCostoUnidad(
            string tipoUnidad)
        {
            if (economiaActual == null ||
                string.IsNullOrWhiteSpace(tipoUnidad))
            {
                return string.Empty;
            }

            CostoEstadoDto costo = null;

            switch (tipoUnidad)
            {
                case "Aldeano":
                    costo = economiaActual.aldeano;
                    break;
                case "Soldado":
                    costo = economiaActual.soldado;
                    break;
                case "Arquero":
                    costo = economiaActual.arquero;
                    break;
            }

            return DescribirCosto(
                costo);
        }

        /// <summary>Formatea un costo en texto legible.</summary>
        private static string DescribirCosto(
            CostoEstadoDto costo)
        {
            if (costo == null)
                return string.Empty;

            return $"Costo: Oro {costo.oro}, " +
                   $"Madera {costo.madera}, " +
                   $"Comida {costo.comida}, " +
                   $"Piedra {costo.piedra}, " +
                   $"Hierro {costo.hierro}.";
        }

        /// <summary>Registra el error y lo muestra en el HUD.</summary>
        private void MostrarError(string mensaje)
        {
            bool tecnico =
                EsErrorTecnico(mensaje);

            if (tecnico)
            {
                Debug.LogError(
                    mensaje,
                    this);
            }
            else
            {
                Debug.LogWarning(
                    mensaje,
                    this);
            }

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    mensaje,
                    tecnico);
            }
        }

        /// <summary>Distingue errores técnicos de rechazos del Modelo.</summary>
        private static bool EsErrorTecnico(
            string mensaje)
        {
            if (string.IsNullOrWhiteSpace(
                    mensaje))
            {
                return false;
            }

            return
                mensaje.Contains("HTTP") ||
                mensaje.Contains("API no") ||
                mensaje.Contains("No se pudo consultar") ||
                mensaje.Contains("No se pudo conectar") ||
                mensaje.Contains("JSON") ||
                mensaje.Contains("worker") ||
                mensaje.Contains("Estado concurrente no reconocido") ||
                mensaje.Contains("VistaPartida no está configurada");
        }

        /// <summary>Localiza la API interna y comprueba la conexión.</summary>
        private void Start()
        {
            if (!usarApiExterna && apiInterna == null)
            {
                apiInterna = FindFirstObjectByType<ApiInterna.ApiInternaJuego>();
                if (apiInterna == null)
                {
                    var go = new GameObject("ApiInternaJuego");
                    apiInterna = go.AddComponent<ApiInterna.ApiInternaJuego>();
                }
            }
            StartCoroutine(ComprobarConexion());
        }

        /// <summary>Verifica la API y carga la partida inicial.</summary>
        private IEnumerator ComprobarConexion()
        {
            if (!usarApiExterna)
            {
                if (!ApiInternaDisponible)
                {
                    MostrarError("La API interna no está disponible.");
                    Debug.LogError("La API interna no está disponible.");
                    yield break;
                }
                Debug.Log("API interna conectada correctamente.");
                yield return SincronizarEstadoInterno("Partida recibida correctamente.");
                yield break;
            }
            string url =
                $"{urlBaseApi}/api/estado";

            using UnityWebRequest request =
                UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"No se pudo conectar con la API: {request.error}");

                yield break;
            }

            Debug.Log(
                $"API conectada correctamente: " +
                $"{request.downloadHandler.text}");

            yield return IniciarPartidaPrueba();

            yield return ObtenerPartidaActiva();
        }

        /// <summary>Crea la partida de prueba en la API externa.</summary>
        private IEnumerator IniciarPartidaPrueba()
        {
            IniciarPartidaDto partida =
                CrearPartidaPrueba();

            string json =
                JsonUtility.ToJson(partida);

            byte[] cuerpo =
                Encoding.UTF8.GetBytes(json);

            string url =
                $"{urlBaseApi}/api/partida/iniciar";

            using UnityWebRequest request =
                new UnityWebRequest(
                    url,
                    UnityWebRequest.kHttpVerbPOST);

            request.uploadHandler =
                new UploadHandlerRaw(cuerpo);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"No se pudo iniciar la partida. " +
                    $"HTTP {request.responseCode}: " +
                    $"{request.downloadHandler.text}");

                yield break;
            }

            Debug.Log(
                $"Partida iniciada correctamente: " +
                $"{request.downloadHandler.text}");
        }

        /// <summary>Descarga la partida y actualiza vista y HUD.</summary>
        private IEnumerator ObtenerPartidaActiva(
            string mensajeExito = "Partida recibida correctamente.",
            string contextoError = "",
            bool mostrarMensaje = true)
        {
            string url =
                $"{urlBaseApi}/api/partida";

            using UnityWebRequest request =
                UnityWebRequest.Get(url);

            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                MostrarError(
                    contextoError +
                    MensajeError(
                        LeerResultado(request.downloadHandler.text),
                        $"No se pudo obtener la partida activa. HTTP {request.responseCode}: {request.error}"));

                yield break;
            }

            Debug.Log(
                $"Partida activa obtenida correctamente: " +
                $"{request.downloadHandler.text}");

            if (vistaPartida == null)
            {
                MostrarError(
                    contextoError +
                    "VistaPartida no está configurada en ControladorAPI.");

                yield break;
            }

            string json =
                request.downloadHandler.text;

            EstadoPartidaDto estadoPartida;

            try
            {
                estadoPartida =
                    JsonUtility.FromJson<EstadoPartidaDto>(json);

                economiaActual =
                    estadoPartida?.economia;
            }
            catch (System.ArgumentException ex)
            {
                MostrarError(
                    contextoError +
                    $"La respuesta de la partida no es JSON válido: {ex.Message}");

                yield break;
            }

            if (estadoPartida?.mapa == null ||
                estadoPartida.mapa.ancho <= 0 ||
                estadoPartida.mapa.alto <= 0 ||
                estadoPartida.jugadorHumano == null ||
                estadoPartida.jugadorMaquina == null)
            {
                MostrarError(
                    contextoError +
                    "La API devolvió un estado de partida incompleto.");

                yield break;
            }

            vistaPartida.Sincronizar(estadoPartida);

            if (vistaHud != null)
            {
                var recursos =
                    estadoPartida.jugadorHumano?.recursos;

                if (recursos != null)
                {
                    vistaHud.MostrarRecursos(
                        recursos.oro,
                        recursos.madera,
                        recursos.comida,
                        recursos.piedra,
                        recursos.hierro);

                    if (mostrarMensaje)
                    {
                        vistaHud.MostrarMensaje(
                            mensajeExito);
                    }
                }
                else
                {
                    vistaHud.MostrarMensaje(
                        "La respuesta no contiene los recursos del jugador humano.",
                        true);
                }
            }
        }

        /// <summary>Construye el DTO de la partida de demostración.</summary>
        private IniciarPartidaDto CrearPartidaPrueba()
        {
            // Misma disposición única del Modelo para que ambas vías
            // (interna y externa) jueguen el mismo mapa 15x15.
            System.Collections.Generic.List<RecursoInicialDto> recursosHumano =
                new System.Collections.Generic.List<RecursoInicialDto>();
            System.Collections.Generic.List<RecursoInicialDto> recursosMaquina =
                new System.Collections.Generic.List<RecursoInicialDto>();

            foreach (ImperiosEnGuerra.Modelo.Recursos.Recurso recurso in
                ImperiosEnGuerra.Modelo.Core.DisposicionInicial.RecursosHumano())
            {
                recursosHumano.Add(new RecursoInicialDto(
                    recurso.Tipo.ToString(),
                    recurso.Coordenada.X,
                    recurso.Coordenada.Y));
            }

            foreach (ImperiosEnGuerra.Modelo.Recursos.Recurso recurso in
                ImperiosEnGuerra.Modelo.Core.DisposicionInicial.RecursosMaquina())
            {
                recursosMaquina.Add(new RecursoInicialDto(
                    recurso.Tipo.ToString(),
                    recurso.Coordenada.X,
                    recurso.Coordenada.Y));
            }

            return new IniciarPartidaDto
            {
                nombreHumano = "Griegos",
                nombreMaquina = "Troya",

                anchoMapa = ImperiosEnGuerra.Modelo.Core.DisposicionInicial.AnchoMapa,
                altoMapa = ImperiosEnGuerra.Modelo.Core.DisposicionInicial.AltoMapa,

                centroHumano = new CoordenadaDto(
                    ImperiosEnGuerra.Modelo.Core.DisposicionInicial.CentroHumano.X,
                    ImperiosEnGuerra.Modelo.Core.DisposicionInicial.CentroHumano.Y),

                centroMaquina = new CoordenadaDto(
                    ImperiosEnGuerra.Modelo.Core.DisposicionInicial.CentroMaquina.X,
                    ImperiosEnGuerra.Modelo.Core.DisposicionInicial.CentroMaquina.Y),

                recursosHumano = recursosHumano.ToArray(),
                recursosMaquina = recursosMaquina.ToArray()
            };
        }
    }
}
