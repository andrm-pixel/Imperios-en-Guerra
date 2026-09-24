using System;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Costo económico inmutable de los cinco recursos del proyecto.
    /// </summary>
    public sealed class CostoRecursos
    {
        public int Oro { get; }
        public int Madera { get; }
        public int Comida { get; }
        public int Piedra { get; }
        public int Hierro { get; }

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

        public bool EsCero =>
            Oro == 0 &&
            Madera == 0 &&
            Comida == 0 &&
            Piedra == 0 &&
            Hierro == 0;

        public override string ToString()
        {
            return $"Oro {Oro}, Madera {Madera}, Comida {Comida}, Piedra {Piedra}, Hierro {Hierro}";
        }
    }
}
