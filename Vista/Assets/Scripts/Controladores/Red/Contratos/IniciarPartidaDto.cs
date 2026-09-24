using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Datos iniciales para crear una partida de prueba.</summary>
    [Serializable]
    public class IniciarPartidaDto
    {
        /// <summary>Nombre del jugador humano.</summary>
        public string nombreHumano;
        /// <summary>Nombre del jugador máquina.</summary>
        public string nombreMaquina;

        /// <summary>Ancho del mapa en casillas.</summary>
        public int anchoMapa;
        /// <summary>Alto del mapa en casillas.</summary>
        public int altoMapa;

        /// <summary>Posición inicial del centro urbano humano.</summary>
        public CoordenadaDto centroHumano;
        /// <summary>Posición inicial del centro urbano máquina.</summary>
        public CoordenadaDto centroMaquina;

        /// <summary>Recursos iniciales del lado humano.</summary>
        public RecursoInicialDto[] recursosHumano;
        /// <summary>Recursos iniciales del lado máquina.</summary>
        public RecursoInicialDto[] recursosMaquina;
    }

    /// <summary>Coordenada lógica serializable para los contratos de la API.</summary>
    [Serializable]
    public class CoordenadaDto
    {
        /// <summary>Columna lógica en el mapa.</summary>
        public int x;
        /// <summary>Fila lógica en el mapa.</summary>
        public int y;

        /// <summary>Crea una coordenada con los valores indicados.</summary>
        public CoordenadaDto(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>Recurso inicial colocado al crear la partida.</summary>
    [Serializable]
    public class RecursoInicialDto
    {
        /// <summary>Tipo de recurso (Oro, Madera, Comida, Piedra, Hierro).</summary>
        public string tipo;
        /// <summary>Columna lógica del recurso.</summary>
        public int x;
        /// <summary>Fila lógica del recurso.</summary>
        public int y;

        /// <summary>Crea un recurso inicial con tipo y posición.</summary>
        public RecursoInicialDto(string tipo, int x, int y)
        {
            this.tipo = tipo;
            this.x = x;
            this.y = y;
        }
    }
}