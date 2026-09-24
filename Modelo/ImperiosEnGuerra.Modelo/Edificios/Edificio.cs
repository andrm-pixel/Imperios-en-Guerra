using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Base de las construcciones del Modelo, con identidad estable,
    /// posición lógica y vida para combate.
    /// </summary>
    public abstract class Edificio
    {
        private readonly object sincronizacionVida =
            new object();

        /// <summary>
        /// Identificador estable e inmutable del edificio.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Posición lógica de la construcción, establecida al crearla.
        /// </summary>
        public Coordenada Coordenada { get; }

        /// <summary>
        /// Puntos de vida actuales. Llegar a cero destruye el edificio.
        /// </summary>
        public int Vida { get; private set; }

        /// <summary>
        /// Vida máxima del edificio; sirve para restaurar partidas guardadas.
        /// </summary>
        public int VidaMaxima { get; }

        /// <summary>
        /// Inicializa la posición común de las construcciones.
        /// </summary>
        /// <param name="coordenada">Posición lógica no nula.</param>
        /// <param name="vidaMaxima">Vida inicial del edificio.</param>
        /// <exception cref="ArgumentNullException">La coordenada es nula.</exception>
        protected Edificio(
            Coordenada coordenada,
            int vidaMaxima = 500)
        {
            if (coordenada == null)
            {
                throw new ArgumentNullException(nameof(coordenada));
            }

            if (vidaMaxima <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(vidaMaxima),
                    "La vida máxima debe ser positiva.");
            }

            Id = Guid.NewGuid();
            Coordenada = coordenada;
            Vida = vidaMaxima;
            VidaMaxima = vidaMaxima;
        }

        /// <summary>
        /// Restaura la vida al cargar una partida guardada.
        /// La usa <see cref="Persistencia.ProgresoPartida"/>.
        /// </summary>
        internal void RestaurarVida(int vida)
        {
            lock (sincronizacionVida)
            {
                if (vida < 0 || vida > VidaMaxima)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(vida),
                        "La vida restaurada debe estar entre 0 y la máxima.");
                }

                Vida = vida;
            }
        }

        /// <summary>
        /// Aplica daño al edificio de forma sincronizada. Devuelve true si queda destruido.
        /// </summary>
        public bool RecibirDano(int dano)
        {
            if (dano < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dano),
                    "El daño no puede ser negativo.");
            }

            lock (sincronizacionVida)
            {
                if (Vida <= 0)
                    return true;

                Vida = Math.Max(0, Vida - dano);
                return Vida <= 0;
            }
        }

        /// <summary>
        /// Indica si el edificio sigue en pie.
        /// </summary>
        public bool EstaDestruido
        {
            get
            {
                lock (sincronizacionVida)
                {
                    return Vida <= 0;
                }
            }
        }
    }
}
