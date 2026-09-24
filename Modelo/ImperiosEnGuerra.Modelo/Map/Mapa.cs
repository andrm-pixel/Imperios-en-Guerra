using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Map
{
    /// <summary>
    /// Administra una cuadricula de casillas y los recursos fisicos colocados en ella.
    /// </summary>
    public class Mapa
    {
        private readonly Casilla[,] casillas;
        private readonly List<Recurso> recursos;

        /// <summary>
        /// Numero de columnas de la cuadricula.
        /// </summary>
        public int Ancho { get; }
        /// <summary>
        /// Numero de filas de la cuadricula.
        /// </summary>
        public int Alto { get; }

        /// <summary>
        /// Vista de solo lectura de los recursos fisicos colocados, actualizada con la coleccion interna.
        /// </summary>
        public IReadOnlyList<Recurso> Recursos
        {
            get { return recursos.AsReadOnly(); }
        }

        /// <summary>
        /// Crea una cuadricula de casillas transitables y desocupadas, sin recursos fisicos.
        /// </summary>
        /// <param name="ancho">Numero de columnas, mayor que cero.</param>
        /// <param name="alto">Numero de filas, mayor que cero.</param>
        /// <exception cref="ArgumentOutOfRangeException">El ancho o el alto es menor o igual que cero.</exception>
        public Mapa(int ancho, int alto)
        {
            if (ancho <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ancho));
            }

            if (alto <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(alto));
            }

            Ancho = ancho;
            Alto = alto;

            casillas = new Casilla[ancho, alto];
            recursos = new List<Recurso>();

            for (int x = 0; x < ancho; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    casillas[x, y] = new Casilla(
                        new Coordenada(x, y),
                        true
                    );
                }
            }
        }

        /// <summary>
        /// Consulta una casilla por sus indices sin modificar el mapa.
        /// </summary>
        /// <param name="x">Indice horizontal desde cero.</param>
        /// <param name="y">Indice vertical desde cero.</param>
        /// <returns>La casilla existente, o null si algun indice queda fuera del mapa.</returns>
        public Casilla ObtenerCasilla(int x, int y)
        {
            if (x < 0 || x >= Ancho || y < 0 || y >= Alto)
            {
                return null;
            }

            return casillas[x, y];
        }

        /// <summary>
        /// Comprueba que ambas componentes esten entre cero incluido y la dimension correspondiente excluida.
        /// </summary>
        /// <param name="coordenada">Posicion que se consulta.</param>
        /// <returns>true si esta dentro del mapa; false si es nula o esta fuera.</returns>
        public bool EstaDentroDeLimites(Coordenada coordenada)
        {
            if (coordenada == null)
            {
                return false;
            }

            return coordenada.X >= 0 && coordenada.X < Ancho
                && coordenada.Y >= 0 && coordenada.Y < Alto;
        }

        /// <summary>
        /// Consulta si existe una casilla libre y sin recurso fisico en la posicion, sin evaluar la transitabilidad.
        /// </summary>
        /// <param name="coordenada">Posicion que se consulta.</param>
        /// <returns>true si esta disponible; false si es nula, esta fuera, ocupada o contiene un recurso.</returns>
        public bool PuedeColocar(Coordenada coordenada)
        {
            if (!EstaDentroDeLimites(coordenada))
            {
                return false;
            }

            Casilla casilla = ObtenerCasilla(coordenada.X, coordenada.Y);
            return casilla != null && !casilla.EstaOcupada
                && ObtenerRecursoEn(coordenada) == null;
        }

        /// <summary>
        /// Busca un recurso comparando los valores X e Y de su coordenada.
        /// </summary>
        /// <param name="coordenada">Posicion logica que se consulta.</param>
        /// <returns>El recurso encontrado, o null si la posicion es nula, esta fuera del mapa o no contiene recurso.</returns>
        public Recurso ObtenerRecursoEn(Coordenada coordenada)
        {
            if (!EstaDentroDeLimites(coordenada))
            {
                return null;
            }

            foreach (Recurso recurso in recursos)
            {
                if (recurso.Coordenada.X == coordenada.X
                    && recurso.Coordenada.Y == coordenada.Y)
                {
                    return recurso;
                }
            }

            return null;
        }

        /// <summary>
        /// Anade un recurso si su posicion esta disponible; no cambia la marca de ocupacion de la casilla.
        /// </summary>
        /// <param name="recurso">Recurso fisico que se intenta colocar.</param>
        /// <returns>true si se anadio; false si es nulo o su posicion no esta disponible.</returns>
        public bool ColocarRecurso(Recurso recurso)
        {
            if (recurso == null)
            {
                return false;
            }

            if (!PuedeColocar(recurso.Coordenada))
            {
                return false;
            }

            recursos.Add(recurso);
            return true;
        }
    }
}
