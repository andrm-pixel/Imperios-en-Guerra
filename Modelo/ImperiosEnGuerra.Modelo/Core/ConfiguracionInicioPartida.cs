using System;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Core
{
    /// <summary>
    /// Balance inicial propio del prototipo academico.
    /// La guia exige mapa, recursos y Castillo, pero no fija estos valores.
    /// </summary>
    public sealed class ConfiguracionInicioPartida
    {
        /// <summary>
        /// Representa el campo aldeanos iniciales predeterminados.
        /// </summary>
        public const int AldeanosInicialesPredeterminados = 2;
        /// <summary>
        /// Representa el campo oro inicial predeterminado.
        /// </summary>
        public const int OroInicialPredeterminado = 0;
        /// <summary>
        /// Representa el campo madera inicial predeterminada.
        /// </summary>
        public const int MaderaInicialPredeterminada = 20;
        /// <summary>
        /// Representa el campo comida inicial predeterminada.
        /// </summary>
        public const int ComidaInicialPredeterminada = 30;
        /// <summary>
        /// Representa el campo piedra inicial predeterminada.
        /// </summary>
        public const int PiedraInicialPredeterminada = 0;
        /// <summary>
        /// Representa el campo hierro inicial predeterminado.
        /// </summary>
        public const int HierroInicialPredeterminado = 0;

        /// <summary>
        /// Obtiene aldeanos iniciales.
        /// </summary>
        public int AldeanosIniciales { get; }
        /// <summary>
        /// Obtiene oro inicial.
        /// </summary>
        public int OroInicial { get; }
        /// <summary>
        /// Obtiene madera inicial.
        /// </summary>
        public int MaderaInicial { get; }
        /// <summary>
        /// Obtiene comida inicial.
        /// </summary>
        public int ComidaInicial { get; }
        /// <summary>
        /// Obtiene piedra inicial.
        /// </summary>
        public int PiedraInicial { get; }
        /// <summary>
        /// Obtiene hierro inicial.
        /// </summary>
        public int HierroInicial { get; }

        /// <summary>
        /// Inicializa una nueva instancia de ConfiguracionInicioPartida.
        /// </summary>
        /// <param name="aldeanosIniciales">El valor de aldeanos iniciales.</param>
        /// <param name="oroInicial">El valor de oro inicial.</param>
        /// <param name="maderaInicial">El valor de madera inicial.</param>
        /// <param name="comidaInicial">El valor de comida inicial.</param>
        /// <param name="piedraInicial">El valor de piedra inicial.</param>
        /// <param name="hierroInicial">El valor de hierro inicial.</param>
        public ConfiguracionInicioPartida(
            int aldeanosIniciales = AldeanosInicialesPredeterminados,
            int oroInicial = OroInicialPredeterminado,
            int maderaInicial = MaderaInicialPredeterminada,
            int comidaInicial = ComidaInicialPredeterminada,
            int piedraInicial = PiedraInicialPredeterminada,
            int hierroInicial = HierroInicialPredeterminado)
        {
            if (aldeanosIniciales <= 0)
                throw new ArgumentOutOfRangeException(nameof(aldeanosIniciales));
            if (oroInicial < 0)
                throw new ArgumentOutOfRangeException(nameof(oroInicial));
            if (maderaInicial < 0)
                throw new ArgumentOutOfRangeException(nameof(maderaInicial));
            if (comidaInicial < 0)
                throw new ArgumentOutOfRangeException(nameof(comidaInicial));
            if (piedraInicial < 0)
                throw new ArgumentOutOfRangeException(nameof(piedraInicial));
            if (hierroInicial < 0)
                throw new ArgumentOutOfRangeException(nameof(hierroInicial));

            AldeanosIniciales = aldeanosIniciales;
            OroInicial = oroInicial;
            MaderaInicial = maderaInicial;
            ComidaInicial = comidaInicial;
            PiedraInicial = piedraInicial;
            HierroInicial = hierroInicial;
        }

        /// <summary>
        /// Aplica saldo inicial.
        /// </summary>
        /// <param name="recursos">El valor de recursos.</param>
        public void AplicarSaldoInicial(
            RecursosJugador recursos)
        {
            if (recursos == null)
                throw new ArgumentNullException(nameof(recursos));

            recursos.Agregar(TipoRecurso.Oro, OroInicial);
            recursos.Agregar(TipoRecurso.Madera, MaderaInicial);
            recursos.Agregar(TipoRecurso.Comida, ComidaInicial);
            recursos.Agregar(TipoRecurso.Piedra, PiedraInicial);
            recursos.Agregar(TipoRecurso.Hierro, HierroInicial);
        }
    }
}
