using System;
using System.Collections.Generic;
using UnityEngine;
using ImperiosEnGuerra.Modelo.Concurrencia;
using ImperiosEnGuerra.Modelo.Contratos;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Mapeadores;
using ImperiosEnGuerra.Modelo.Persistencia;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Reglas;
using ImperiosEnGuerra.Modelo.Servicios;
using ImperiosEnGuerra.Modelo.Unidades;
using ContratosUnity = ImperiosEnGuerra.Controladores.Red.Contratos;

namespace ImperiosEnGuerra.Controladores.ApiInterna
{
    /// <summary>
    /// API interna del juego. Vive en el Controlador como puente.
    /// No crea Thread/Task: toda la concurrencia esta en el Modelo.
    /// Sin este componente el juego no funciona (los controladores lo exigen).
    /// Reemplaza al proceso externo por terminal (localhost:5086).
    /// </summary>
    public class ApiInternaJuego : MonoBehaviour
    {
        /// <summary>Instancia unica persistente de la API interna.</summary>
        public static ApiInternaJuego Instancia { get; private set; }

        /// <summary>Servicio de archivos para la partida interna.</summary>
        private ServicioArchivos servicioArchivos;
        /// <summary>Servicio del Modelo con la partida activa.</summary>
        private EstadoPartidaService estadoPartida;
        /// <summary>Gestor de workers del Modelo.</summary>
        private TareasJuego gestorProcesos;
        /// <summary>Fachada de acciones concurrentes del Modelo.</summary>
        private MotorAcciones accionesConcurrentes;
        /// <summary>Proceso del turno continuo de la maquina.</summary>
        private Guid procesoIA = Guid.Empty;

        /// <summary>Indica si la API interna tiene partida activa.</summary>
        public bool EstaDisponible { get; private set; }

        /// <summary>Crea la instancia unica e inicializa el nucleo.</summary>
        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;
            DontDestroyOnLoad(gameObject);
            InicializarNucleo();
        }

        /// <summary>Libera los workers y limpia la instancia.</summary>
        private void OnDestroy()
        {
            if (Instancia == this)
            {
                gestorProcesos?.Dispose();
                Instancia = null;
            }
        }

        /// <summary>Crea los servicios y la partida de prueba interna.</summary>
        private void InicializarNucleo()
        {
            try
            {
                string baseDatos = System.IO.Path.Combine(
                    Application.persistentDataPath,
                    "DatosPartida");

                servicioArchivos = new ServicioArchivos(baseDatos);
                estadoPartida = new EstadoPartidaService(servicioArchivos);
                gestorProcesos = new TareasJuego();
                accionesConcurrentes = new MotorAcciones(
                    estadoPartida,
                    gestorProcesos);

                IniciarPartidaPruebaInterna();
                EstaDisponible = estadoPartida.HayPartidaActiva();
            }
            catch (Exception ex)
            {
                Debug.LogError($"No se pudo inicializar la API interna: {ex.Message}", this);
                EstaDisponible = false;
            }
        }

        /// <summary>Genera el mapa y los recursos de demostracion.</summary>
        private void IniciarPartidaPruebaInterna()
        {
            var mapa = new Mapa(
                InicializadorPartida.AnchoMapa,
                InicializadorPartida.AltoMapa);
            var inicializador = new InicializadorPartida();

            Partida partida = inicializador.Crear(
                "Griegos",
                mapa,
                InicializadorPartida.CentroHumano,
                new List<Recurso>(InicializadorPartida.RecursosHumano()),
                "Troya",
                mapa,
                InicializadorPartida.CentroMaquina,
                new List<Recurso>(InicializadorPartida.RecursosMaquina()));

            servicioArchivos.GuardarConfiguracionInicial(partida);
            estadoPartida.EstablecerPartida(partida);

            // Guarnicion inicial de la maquina: patrulla su base y caza
            // humanos en un radio de 7 casillas.
            foreach (Coordenada posicion in InicializadorPartida.GuarnicionMaquina())
            {
                if ((posicion.X + posicion.Y) % 2 == 0)
                    estadoPartida.ObtenerPartida()?.JugadorMaquina.AgregarUnidad(
                        new Soldado(posicion));
                else
                    estadoPartida.ObtenerPartida()?.JugadorMaquina.AgregarUnidad(
                        new Arquero(posicion));
            }

            procesoIA = accionesConcurrentes.IniciarIA().Id;
        }

        /// <summary>Devuelve el estado actual como DTO de Unity.</summary>
        public ContratosUnity.EstadoPartidaDto ObtenerEstado()
        {
            ExigirDisponible();
            EstadoPartidaResponse respuesta = estadoPartida.ObtenerEstado();
            if (respuesta == null)
                throw new InvalidOperationException("No hay una partida activa.");
            return AdaptadorEstadoPartida.Convertir(respuesta);
        }

        /// <summary>Inicia el movimiento concurrente de una unidad.</summary>
        public Guid IniciarMovimiento(string unidadId, int x, int y)
        {
            ExigirDisponible();
            var proceso = accionesConcurrentes.IniciarMovimiento(new MoverUnidadRequest
            {
                UnidadId = unidadId,
                Destino = new CoordenadaRequest { X = x, Y = y }
            });
            return proceso.Id;
        }

        /// <summary>Inicia la recoleccion concurrente de un aldeano.</summary>
        public Guid IniciarRecoleccion(string aldeanoId, int x, int y)
        {
            ExigirDisponible();
            var proceso = accionesConcurrentes.IniciarRecoleccion(new RecolectarRequest
            {
                AldeanoId = aldeanoId,
                Objetivo = new CoordenadaRequest { X = x, Y = y }
            });
            return proceso.Id;
        }

        /// <summary>Inicia la construccion concurrente de un aldeano.</summary>
        public Guid IniciarConstruccion(string aldeanoId, string tipoEdificio, int x, int y)
        {
            ExigirDisponible();
            var proceso = accionesConcurrentes.IniciarConstruccion(new ConstruirRequest
            {
                AldeanoId = aldeanoId,
                TipoEdificio = tipoEdificio,
                Destino = new CoordenadaRequest { X = x, Y = y }
            });
            return proceso.Id;
        }

        /// <summary>Inicia el entrenamiento concurrente en un edificio.</summary>
        public Guid IniciarEntrenamiento(int edificioX, int edificioY, string tipoUnidad, int destinoX, int destinoY)
        {
            ExigirDisponible();
            var proceso = accionesConcurrentes.IniciarEntrenamiento(new EntrenarRequest
            {
                EdificioOrigen = new CoordenadaRequest { X = edificioX, Y = edificioY },
                TipoUnidad = tipoUnidad,
                Destino = new CoordenadaRequest { X = destinoX, Y = destinoY }
            });
            return proceso.Id;
        }

        /// <summary>Inicia el ataque concurrente a un objetivo.</summary>
        public Guid IniciarAtaque(string atacanteId, string objetivoId)
        {
            ExigirDisponible();
            var proceso = accionesConcurrentes.IniciarAtaque(new AtacarRequest
            {
                AtacanteId = atacanteId,
                ObjetivoId = objetivoId
            });
            return proceso.Id;
        }

        public IReadOnlyList<MotorAcciones.ProcesoBatalla> IniciarBatalla()
        {
            ExigirDisponible();

            return accionesConcurrentes.IniciarBatalla();
        }

        /// <summary>Lee el resultado de un proceso si ya termino.</summary>
        public bool IntentarObtenerResultado(Guid procesoId, out ContratosUnity.ResultadoProcesoDto dto)
        {
            dto = null;
            ExigirDisponible();
            if (!accionesConcurrentes.IntentarObtenerResultado(procesoId, out ResultadoProcesoConcurrente r))
                return false;
            dto = new ContratosUnity.ResultadoProcesoDto
            {
                procesoId = r.ProcesoId.ToString("D"),
                nombre = r.Nombre,
                estado = r.Estado.ToString(),
                hiloTrabajoId = r.HiloTrabajoId,
                exito = r.Resultado?.Exito ?? false,
                mensaje = r.Resultado?.Mensaje,
                errorTecnico = r.ErrorTecnico
            };
            return true;
        }

        /// <summary>Cancela un proceso concurrente en curso.</summary>
        public bool CancelarProceso(Guid procesoId)
        {
            ExigirDisponible();
            return accionesConcurrentes.Cancelar(procesoId);
        }

        /// <summary>
        /// Lee el resultado del turno de la máquina sin bloquear.
        /// Anuncia su victoria en el HUD cuando ocurre.
        /// </summary>
        public bool IntentarResultadoIA(out ContratosUnity.ResultadoProcesoDto dto)
        {
            dto = null;

            if (!EstaDisponible || procesoIA == Guid.Empty)
                return false;

            if (!accionesConcurrentes.IntentarObtenerResultado(procesoIA, out ResultadoProcesoConcurrente r))
                return false;

            dto = new ContratosUnity.ResultadoProcesoDto
            {
                procesoId = r.ProcesoId.ToString("D"),
                nombre = r.Nombre,
                estado = r.Estado.ToString(),
                hiloTrabajoId = r.HiloTrabajoId,
                exito = r.Resultado?.Exito ?? false,
                mensaje = r.Resultado?.Mensaje,
                errorTecnico = r.ErrorTecnico
            };
            return true;
        }

        /// <summary>
        /// Guarda el progreso en progreso.txt (tecla F5).
        /// </summary>
        public string GuardarProgreso()
        {
            ExigirDisponible();
            var resultado = estadoPartida.GuardarProgreso();
            return resultado.Mensaje;
        }

        /// <summary>
        /// Carga el progreso desde progreso.txt (tecla F9).
        /// Cancela los workers anteriores porque la partida cambia.
        /// </summary>
        public string CargarProgreso()
        {
            ExigirDisponible();
            accionesConcurrentes.CancelarTodos();
            var resultado = estadoPartida.CargarProgreso();

            // La carga cancela el worker de IA: se relanza.
            procesoIA = accionesConcurrentes.IniciarIA().Id;

            return resultado.Mensaje;
        }

        /// <summary>Consulta si la unidad puede recibir orden de mover.</summary>
        public bool PermiteMover(string propietario, string categoria, string ordenActiva)
        {
            return ReglasAcciones.PermiteMover(propietario, categoria, ordenActiva);
        }

        /// <summary>Consulta si el aldeano puede recolectar.</summary>
        public bool PermiteRecolectar(string propietario, string categoria, string tipo, string orden)
        {
            return ReglasAcciones.PermiteRecolectar(propietario, categoria, tipo, orden);
        }

        /// <summary>Consulta si el aldeano puede construir.</summary>
        public bool PermiteConstruir(string propietario, string categoria, string tipo, string orden)
        {
            return ReglasAcciones.PermiteConstruir(propietario, categoria, tipo, orden);
        }

        /// <summary>Consulta si el edificio puede entrenar.</summary>
        public bool PermiteEntrenar(string propietario, string categoria, string tipo)
        {
            return ReglasAcciones.PermiteEntrenar(propietario, categoria, tipo);
        }

        /// <summary>Consulta si la unidad puede atacar.</summary>
        public bool PermiteAtacar(string propietario, string categoria, string tipo, string orden)
        {
            return ReglasAcciones.PermiteAtacar(propietario, categoria, tipo, orden);
        }

        /// <summary>Consulta si la entidad es objetivo valido.</summary>
        public bool EsObjetivoAtaqueValido(string categoria, string propietario, string id)
        {
            return ReglasAcciones.EsObjetivoAtaqueValido(categoria, propietario, id);
        }

        /// <summary>Nombre del tipo Castillo del Modelo.</summary>
        public string TipoCastillo
        {
            get { return ReglasAcciones.TipoCastillo; }
        }

        /// <summary>Lanza error si la API interna no esta lista.</summary>
        private void ExigirDisponible()
        {
            if (!EstaDisponible || estadoPartida == null || accionesConcurrentes == null)
                throw new InvalidOperationException("La API interna no está disponible.");
        }
    }
}
