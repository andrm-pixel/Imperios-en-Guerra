using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Solicitud de ataque entre dos unidades por identificador.</summary>
    [Serializable]
    public class AtaqueDto
    {
        /// <summary>Identificador de la unidad atacante propia.</summary>
        public string atacanteId;
        /// <summary>Identificador de la unidad enemiga objetivo.</summary>
        public string objetivoId;
    }
}
