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
    /// No crea Thread/Task: toda la concurrencia está en el Modelo.
    /// Sin este componente el juego no funciona (los controladores lo exigen).
    /// Reemplaza al proceso externo por terminal (localhost:5086).
    /// </summary>
    public class ApiInternaJuego : MonoBehaviour
    {
        public static ApiInternaJuego Instancia { get; private set; }

        private ServicioArchivos servicioArchivos;
        private EstadoPartidaService estadoPartida;
        private GestorProcesosConcurrentes gestorProcesos;
        private ServicioOrdenesUnidad servicioOrdenes;
        private ServicioAccionesConcurrentes accionesConcurrentes;

        public bool EstaDisponible { get; private set; }

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

        private void OnDestroy()
        {
            if (Instancia == this)
            {
                gestorProcesos?.Dispose();
                Instancia = null;
            }
        }

        private void InicializarNucleo()
        {
            try
            {
                string baseDatos = System.IO.Path.Combine(
                    Application.persistentDataPath,
                    "DatosPartida");

                servicioArchivos = new ServicioArchivos(baseDatos);
                estadoPartida = new EstadoPartidaService(servicioArchivos);
                gestorProcesos = new GestorProcesosConcurrentes();
                servicioOrdenes = new ServicioOrdenesUnidad();
                accionesConcurrentes = new ServicioAccionesConcurrentes(
                    estadoPartida,
                    gestorProcesos,
                    servicioOrdenes);

                IniciarPartidaPruebaInterna();
                EstaDisponible = estadoPartida.HayPartidaActiva();
            }
            catch (Exception ex)
            {
                Debug.LogError($"No se pudo inicializar la API interna: {ex.Message}", this);
                EstaDisponible = false;
            }
        }

        private void IniciarPartidaPruebaInterna()
        {
            var mapa = new Mapa(15, 15);
            var inicializador = new InicializadorPartida();

            Partida partida = inicializador.Crear(
                "Jugador",
                mapa,
                new Coordenada(1, 7),
                new List<Recurso>
                {
                    new Recurso(TipoRecurso.Oro, new Coordenada(3, 6)),
                    new Recurso(TipoRecurso.Oro, new Coordenada(1, 9)),
                    new Recurso(TipoRecurso.Madera, new Coordenada(4, 7)),
                    new Recurso(TipoRecurso.Madera, new Coordenada(2, 9)),
                    new Recurso(TipoRecurso.Comida, new Coordenada(3, 8)),
                    new Recurso(TipoRecurso.Comida, new Coordenada(5, 7)),
                    new Recurso(TipoRecurso.Piedra, new Coordenada(2, 5)),
                    new Recurso(TipoRecurso.Piedra, new Coordenada(4, 9)),
                    new Recurso(TipoRecurso.Hierro, new Coordenada(5, 5)),
                    new Recurso(TipoRecurso.Hierro, new Coordenada(1, 5)),
                    new Recurso(TipoRecurso.Madera, new Coordenada(0, 6)),
                    new Recurso(TipoRecurso.Madera, new Coordenada(4, 5)),
                    new Recurso(TipoRecurso.Oro, new Coordenada(2, 7)),
                    new Recurso(TipoRecurso.Comida, new Coordenada(0, 8)),
                    new Recurso(TipoRecurso.Piedra, new Coordenada(5, 9)),
                    new Recurso(TipoRecurso.Hierro, new Coordenada(3, 4))
                },
                "CPU",
                mapa,
                new Coordenada(13, 7),
                new List<Recurso>
                {
                    new Recurso(TipoRecurso.Oro, new Coordenada(11, 6)),
                    new Recurso(TipoRecurso.Oro, new Coordenada(13, 9)),
                    new Recurso(TipoRecurso.Madera, new Coordenada(10, 7)),
                    new Recurso(TipoRecurso.Madera, new Coordenada(12, 9)),
                    new Recurso(TipoRecurso.Comida, new Coordenada(11, 8)),
                    new Recurso(TipoRecurso.Comida, new Coordenada(9, 7)),
                    new Recurso(TipoRecurso.Piedra, new Coordenada(12, 5)),
                    new Recurso(TipoRecurso.Piedra, new Coordenada(10, 9)),
                    new Recurso(TipoRecurso.Hierro, new Coordenada(9, 9)),
                    new Recurso(TipoRecurso.Hierro, new Coordenada(12, 11)),
                    new Recurso(TipoRecurso.Madera, new Coordenada(14, 6)),
                    new Recurso(TipoRecurso.Madera, new Coordenada(10, 5)),
                    new Recurso(TipoRecurso.Oro, new Coordenada(14, 9)),
                    new Recurso(TipoRecurso.Comida, new Coordenada(14, 8)),
                    new Recurso(TipoRecurso.Piedra, new Coordenada(9, 5)),
                    new Recurso(TipoRecurso.Hierro, new Coordenada(11, 10))
                });

            servicioArchivos.GuardarConfiguracionInicial(partida);
            estadoPartida.EstablecerPartida(partida);

            // Guarnición inicial de la máquina: patrulla su base y caza
            // humanos en un radio de 7 casillas.
            estadoPartida.ObtenerPartida()?.JugadorMaquina.AgregarUnidad(
                new Guerrero(new Coordenada(11, 7)));
            estadoPartida.ObtenerPartida()?.JugadorMaquina.AgregarUnidad(
                new Lancero(new Coordenada(12, 6)));

            accionesConcurrentes.IniciarIA();
        }

        public ContratosUnity.EstadoPartidaDto ObtenerEstado()
        {
            ExigirDisponible();
            EstadoPartidaResponse respuesta = estadoPartida.ObtenerEstado();
            if (respuesta == null)
                throw new InvalidOperationException("No hay una partida activa.");
            return AdaptadorEstadoPartida.Convertir(respuesta);
        }

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

        public bool CancelarProceso(Guid procesoId)
        {
            ExigirDisponible();
            return accionesConcurrentes.Cancelar(procesoId);
        }

        public bool PermiteMover(string propietario, string categoria, string ordenActiva)
        {
            return ReglasAcciones.PermiteMover(propietario, categoria, ordenActiva);
        }

        public bool PermiteRecolectar(string propietario, string categoria, string tipo, string orden)
        {
            return ReglasAcciones.PermiteRecolectar(propietario, categoria, tipo, orden);
        }

        public bool PermiteConstruir(string propietario, string categoria, string tipo, string orden)
        {
            return ReglasAcciones.PermiteConstruir(propietario, categoria, tipo, orden);
        }

        public bool PermiteEntrenar(string propietario, string categoria, string tipo)
        {
            return ReglasAcciones.PermiteEntrenar(propietario, categoria, tipo);
        }

        public bool PermiteAtacar(string propietario, string categoria, string tipo, string orden)
        {
            return ReglasAcciones.PermiteAtacar(propietario, categoria, tipo, orden);
        }

        public bool EsObjetivoAtaqueValido(string categoria, string propietario, string id)
        {
            return ReglasAcciones.EsObjetivoAtaqueValido(categoria, propietario, id);
        }

        public string TipoCentroUrbano
        {
            get { return ReglasAcciones.TipoCentroUrbano; }
        }

        private void ExigirDisponible()
        {
            if (!EstaDisponible || estadoPartida == null || accionesConcurrentes == null)
                throw new InvalidOperationException("La API interna no está disponible.");
        }
    }
}
