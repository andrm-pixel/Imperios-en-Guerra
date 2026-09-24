using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Core
{
    /// <summary>
    /// Disposición inicial única del mapa 15x15: centros laterales y nodos
    /// en celdas pares para que nunca formen muros (siempre hay corredor).
    /// La usan la API interna, el modo externo y las pruebas de regresión.
    /// </summary>
    public static class DisposicionInicial
    {
        public const int AnchoMapa = 15;
        public const int AltoMapa = 15;

        public static readonly Coordenada CentroHumano = new Coordenada(13, 7);
        public static readonly Coordenada CentroMaquina = new Coordenada(1, 7);

        public static IReadOnlyList<Recurso> RecursosHumano()
        {
            return new List<Recurso>
            {
                new Recurso(TipoRecurso.Oro, new Coordenada(10, 6)),
                new Recurso(TipoRecurso.Oro, new Coordenada(14, 8)),
                new Recurso(TipoRecurso.Oro, new Coordenada(12, 12)),
                new Recurso(TipoRecurso.Madera, new Coordenada(12, 6)),
                new Recurso(TipoRecurso.Madera, new Coordenada(8, 8)),
                new Recurso(TipoRecurso.Madera, new Coordenada(14, 10)),
                new Recurso(TipoRecurso.Madera, new Coordenada(10, 12)),
                new Recurso(TipoRecurso.Madera, new Coordenada(8, 12)),
                new Recurso(TipoRecurso.Comida, new Coordenada(10, 8)),
                new Recurso(TipoRecurso.Comida, new Coordenada(14, 12)),
                new Recurso(TipoRecurso.Comida, new Coordenada(8, 10)),
                new Recurso(TipoRecurso.Piedra, new Coordenada(12, 8)),
                new Recurso(TipoRecurso.Piedra, new Coordenada(10, 10)),
                new Recurso(TipoRecurso.Piedra, new Coordenada(14, 4)),
                new Recurso(TipoRecurso.Hierro, new Coordenada(12, 10)),
                new Recurso(TipoRecurso.Hierro, new Coordenada(8, 6))
            };
        }

        public static IReadOnlyList<Recurso> RecursosMaquina()
        {
            return new List<Recurso>
            {
                new Recurso(TipoRecurso.Oro, new Coordenada(4, 6)),
                new Recurso(TipoRecurso.Oro, new Coordenada(0, 8)),
                new Recurso(TipoRecurso.Oro, new Coordenada(2, 12)),
                new Recurso(TipoRecurso.Madera, new Coordenada(2, 6)),
                new Recurso(TipoRecurso.Madera, new Coordenada(6, 8)),
                new Recurso(TipoRecurso.Madera, new Coordenada(0, 10)),
                new Recurso(TipoRecurso.Madera, new Coordenada(4, 12)),
                new Recurso(TipoRecurso.Madera, new Coordenada(6, 12)),
                new Recurso(TipoRecurso.Comida, new Coordenada(4, 8)),
                new Recurso(TipoRecurso.Comida, new Coordenada(0, 12)),
                new Recurso(TipoRecurso.Comida, new Coordenada(6, 10)),
                new Recurso(TipoRecurso.Piedra, new Coordenada(2, 8)),
                new Recurso(TipoRecurso.Piedra, new Coordenada(4, 10)),
                new Recurso(TipoRecurso.Piedra, new Coordenada(0, 4)),
                new Recurso(TipoRecurso.Hierro, new Coordenada(2, 10)),
                new Recurso(TipoRecurso.Hierro, new Coordenada(6, 6))
            };
        }

        /// <summary>
        /// Posiciones de la guarnición inicial de la máquina junto a su base.
        /// </summary>
        public static IReadOnlyList<Coordenada> GuarnicionMaquina()
        {
            return new List<Coordenada>
            {
                new Coordenada(4, 7),
                new Coordenada(3, 9)
            };
        }
    }
}
