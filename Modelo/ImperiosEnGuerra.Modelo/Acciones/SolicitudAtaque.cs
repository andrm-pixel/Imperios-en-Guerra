using System;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Identifica atacante y objetivo mediante IDs estables; la operación valida la intención.</summary>
    public sealed class SolicitudAtaque : SolicitudAccion
    {
        /// <summary>
        /// Obtiene atacante id.
        /// </summary>
        public Guid AtacanteId { get; }
        /// <summary>
        /// Obtiene objetivo id.
        /// </summary>
        public Guid ObjetivoId { get; }

        /// <summary>
        /// Inicializa una nueva instancia de SolicitudAtaque.
        /// </summary>
        /// <param name="atacanteId">El valor de atacante id.</param>
        /// <param name="objetivoId">El valor de objetivo id.</param>
        public SolicitudAtaque(Guid atacanteId, Guid objetivoId)
            : base(TipoAccionJuego.Atacar)
        {
            AtacanteId = atacanteId;
            ObjetivoId = objetivoId;
        }
    }
}
