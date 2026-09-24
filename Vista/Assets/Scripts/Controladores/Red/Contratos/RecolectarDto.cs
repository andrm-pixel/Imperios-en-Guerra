using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Solicitud de recoleccion de un recurso por un aldeano.</summary>
    [Serializable]
    public class RecolectarDto
    {
        /// <summary>Identificador del aldeano recolector.</summary>
        public string aldeanoId;
        /// <summary>Casilla del recurso objetivo a recolectar.</summary>
        public CoordenadaDto objetivo;
    }
}
