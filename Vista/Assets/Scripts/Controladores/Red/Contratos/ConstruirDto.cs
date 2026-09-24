using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Solicitud de construccion de un edificio por un aldeano.</summary>
    [Serializable]
    public class ConstruirDto
    {
        /// <summary>Identificador del aldeano constructor.</summary>
        public string aldeanoId;
        /// <summary>Tipo de edificio a construir (Castillo).</summary>
        public string tipoEdificio;
        /// <summary>Casilla destino donde se levantara la obra.</summary>
        public CoordenadaDto destino;
    }
}
