using System;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Costo economico inmutable de los cinco recursos del proyecto.
    /// </summary>
    public sealed class CostoRecursos
    {
        /// <summary>
        /// Obtiene oro.
        /// </summary>
        public int Oro { get; }
        /// <summary>
        /// Obtiene madera.
        /// </summary>
        public int Madera { get; }
        /// <summary>
        /// Obtiene comida.
        /// </summary>
        public int Comida { get; }
        /// <summary>
        /// Obtiene piedra.
        /// </summary>
        public int Piedra { get; }
        /// <summary>
        /// Obtiene hierro.
        /// </summary>
        public int Hierro { get; }

        /// <summary>
        /// Inicializa una nueva instancia de CostoRecursos.
        /// </summary>
        /// <param name="oro">El valor de oro.</param>
        /// <param name="madera">El valor de madera.</param>
        /// <param name="comida">El valor de comida.</param>
        /// <param name="piedra">El valor de piedra.</param>
        /// <param name="hierro">El valor de hierro.</param>
        public CostoRecursos(
            int oro,
            int madera,
            int comida,
            int piedra = 0,
            int hierro = 0)
        {
            if (oro < 0)
                throw new ArgumentOutOfRangeException(nameof(oro));
            if (madera < 0)
                throw new ArgumentOutOfRangeException(nameof(madera));
            if (comida < 0)
                throw new ArgumentOutOfRangeException(nameof(comida));
            if (piedra < 0)
                throw new ArgumentOutOfRangeException(nameof(piedra));
            if (hierro < 0)
                throw new ArgumentOutOfRangeException(nameof(hierro));

            Oro = oro;
            Madera = madera;
            Comida = comida;
            Piedra = piedra;
            Hierro = hierro;
        }

        /// <summary>
        /// Obtiene es cero.
        /// </summary>
        public bool EsCero =>
            Oro == 0 &&
            Madera == 0 &&
            Comida == 0 &&
            Piedra == 0 &&
            Hierro == 0;

        /// <summary>
        /// Ejecuta la operacion to string.
        /// </summary>
        /// <returns>Resultado de la operacion.</returns>
        public override string ToString()
        {
            return $"Oro {Oro}, Madera {Madera}, Comida {Comida}, Piedra {Piedra}, Hierro {Hierro}";
        }
    }
}
