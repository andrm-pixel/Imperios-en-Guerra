using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Contrato de solicitud de entrenamiento desde un edificio.</summary>
    [Serializable]
    public class EntrenarDto
    {
        /// <summary>Coordenada del edificio que entrena la unidad.</summary>
        public CoordenadaDto edificioOrigen;
        /// <summary>Tipo de unidad solicitada (Aldeano, Guerrero, etc.).</summary>
        public string tipoUnidad;
        /// <summary>Casilla de referencia donde aparecera la unidad.</summary>
        public CoordenadaDto destino;
    }
}
