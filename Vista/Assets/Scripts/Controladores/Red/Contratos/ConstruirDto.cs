using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Solicitud de construcción de un edificio por un aldeano.</summary>
    [Serializable]
    public class ConstruirDto
    {
        /// <summary>Identificador del aldeano constructor.</summary>
        public string aldeanoId;
        /// <summary>Tipo de edificio a construir (CentroUrbano).</summary>
        public string tipoEdificio;
        /// <summary>Casilla destino donde se levantará la obra.</summary>
        public CoordenadaDto destino;
    }
}