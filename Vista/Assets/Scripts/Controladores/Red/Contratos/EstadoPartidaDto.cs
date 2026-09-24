using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Instantánea completa de la partida recibida de la API.</summary>
    [Serializable]
    public class EstadoPartidaDto
    {
        /// <summary>Estado general de la partida.</summary>
        public string estado;
        /// <summary>Mapa con dimensiones y recursos visibles.</summary>
        public MapaEstadoDto mapa;
        /// <summary>Datos del jugador humano.</summary>
        public JugadorEstadoDto jugadorHumano;
        /// <summary>Datos del jugador máquina.</summary>
        public JugadorEstadoDto jugadorMaquina;
        /// <summary>Costos de edificios y unidades.</summary>
        public EconomiaEstadoDto economia;
    }

    /// <summary>Costos de construcción y entrenamiento vigentes.</summary>
    [Serializable]
    public class EconomiaEstadoDto
    {
        /// <summary>Costo del centro urbano.</summary>
        public CostoEstadoDto centroUrbano;
        /// <summary>Costo del aldeano.</summary>
        public CostoEstadoDto aldeano;
        /// <summary>Costo del guerrero.</summary>
        public CostoEstadoDto guerrero;
        /// <summary>Costo del arquero.</summary>
        public CostoEstadoDto arquero;
    }

    /// <summary>Costo en recursos de una entidad.</summary>
    [Serializable]
    public class CostoEstadoDto
    {
        /// <summary>Cantidad de oro requerida.</summary>
        public int oro;
        /// <summary>Cantidad de madera requerida.</summary>
        public int madera;
        /// <summary>Cantidad de comida requerida.</summary>
        public int comida;
        /// <summary>Cantidad de piedra requerida.</summary>
        public int piedra;
        /// <summary>Cantidad de hierro requerida.</summary>
        public int hierro;
    }

    /// <summary>Dimensiones del mapa y recursos presentes.</summary>
    [Serializable]
    public class MapaEstadoDto
    {
        /// <summary>Ancho del mapa en casillas.</summary>
        public int ancho;
        /// <summary>Alto del mapa en casillas.</summary>
        public int alto;
        /// <summary>Recursos colocados en el mapa.</summary>
        public RecursoEstadoDto[] recursos;
    }

    /// <summary>Estado de un jugador con sus entidades y recursos.</summary>
    [Serializable]
    public class JugadorEstadoDto
    {
        /// <summary>Nombre del jugador.</summary>
        public string nombre;
        /// <summary>Tipo de jugador (Humano o Maquina).</summary>
        public string tipo;
        /// <summary>Recursos acumulados del jugador.</summary>
        public RecursosJugadorEstadoDto recursos;
        /// <summary>Edificios construidos del jugador.</summary>
        public EdificioEstadoDto[] edificios;
        /// <summary>Obras de construcción en curso.</summary>
        public ObraConstruccionEstadoDto[] obrasConstruccion;
        /// <summary>Unidades vivas del jugador.</summary>
        public UnidadEstadoDto[] unidades;
    }

    /// <summary>Recursos acumulados por un jugador.</summary>
    [Serializable]
    public class RecursosJugadorEstadoDto
    {
        /// <summary>Oro disponible.</summary>
        public int oro;
        /// <summary>Madera disponible.</summary>
        public int madera;
        /// <summary>Comida disponible.</summary>
        public int comida;
        /// <summary>Piedra disponible.</summary>
        public int piedra;
        /// <summary>Hierro disponible.</summary>
        public int hierro;
    }

    /// <summary>Recurso del mapa con su cantidad restante.</summary>
    [Serializable]
    public class RecursoEstadoDto
    {
        /// <summary>Tipo de recurso.</summary>
        public string tipo;
        /// <summary>Posición lógica del recurso.</summary>
        public CoordenadaEstadoDto coordenada;
        /// <summary>Cantidad aún recolectable.</summary>
        public int cantidadRestante;
    }

    /// <summary>Edificio construido con su cola de entrenamiento.</summary>
    [Serializable]
    public class EdificioEstadoDto
    {
        /// <summary>Identificador lógico del edificio.</summary>
        public string id;
        /// <summary>Tipo de edificio.</summary>
        public string tipo;
        /// <summary>Posición lógica del edificio.</summary>
        public CoordenadaEstadoDto coordenada;
        /// <summary>Unidades en cola de entrenamiento.</summary>
        public EntrenamientoEstadoDto[] colaEntrenamiento;
    }

    /// <summary>Unidad en cola de entrenamiento de un edificio.</summary>
    [Serializable]
    public class EntrenamientoEstadoDto
    {
        /// <summary>Identificador del pedido de entrenamiento.</summary>
        public string id;
        /// <summary>Tipo de unidad en entrenamiento.</summary>
        public string tipoUnidad;
        /// <summary>Progreso porcentual del entrenamiento.</summary>
        public int progreso;
    }

    /// <summary>Obra de construcción en curso con su progreso.</summary>
    [Serializable]
    public class ObraConstruccionEstadoDto
    {
        /// <summary>Identificador de la obra.</summary>
        public string id;
        /// <summary>Tipo de edificio en construcción.</summary>
        public string tipo;
        /// <summary>Posición lógica de la obra.</summary>
        public CoordenadaEstadoDto coordenada;
        /// <summary>Progreso porcentual de la obra.</summary>
        public int progreso;
    }

    /// <summary>Unidad con su posición, estado y carga actual.</summary>
    [Serializable]
    public class UnidadEstadoDto
    {
        /// <summary>Identificador lógico de la unidad.</summary>
        public string id;
        /// <summary>Tipo de unidad.</summary>
        public string tipo;
        /// <summary>Posición lógica actual.</summary>
        public CoordenadaEstadoDto coordenada;
        /// <summary>Indica si acepta nuevas órdenes.</summary>
        public bool disponible;

        /// <summary>Estado lógico (Reposo, Moviendo, etc.).</summary>
        public string estado;
        /// <summary>Orden activa que ejecuta la unidad.</summary>
        public string ordenActiva;

        /// <summary>Capacidad máxima de carga del aldeano.</summary>
        public int capacidadCarga;
        /// <summary>Carga transportada actualmente.</summary>
        public int cargaActual;
        /// <summary>Tipo de recurso cargado.</summary>
        public string tipoCarga;
}

    /// <summary>Coordenada lógica del estado de partida.</summary>
    [Serializable]
    public class CoordenadaEstadoDto
    {
        /// <summary>Columna lógica en el mapa.</summary>
        public int x;
        /// <summary>Fila lógica en el mapa.</summary>
        public int y;
    }
}
