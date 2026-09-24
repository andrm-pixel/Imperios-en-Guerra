using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Solicitud de movimiento de una unidad a una casilla.</summary>
    [Serializable]
    public class MoverUnidadDto
    {
        /// <summary>Identificador de la unidad que se movera.</summary>
        public string unidadId;
        /// <summary>Casilla destino del desplazamiento.</summary>
        public CoordenadaDto destino;
    }

    /// <summary>Respuesta simple de exito o error de una accion.</summary>
    [Serializable]
    public class ResultadoAccionDto
    {
        /// <summary>Indica si la accion fue aceptada por el Modelo.</summary>
        public bool exito;
        /// <summary>Mensaje descriptivo del resultado para el HUD.</summary>
        public string mensaje;
        /// <summary>Detalle de error logico cuando la accion se rechaza.</summary>
        public string error;
    }
}
