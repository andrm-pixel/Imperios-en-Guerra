using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Persistencia
{
    /// <summary>
    /// Guarda y restaura el progreso en texto plano versionado.
    /// Lo usan <see cref="Servicios.EstadoPartidaService"/> (guardar/cargar)
    /// y la tecla F5/F9 del juego. Formato PROGRESO_V1 con secciones por
    /// jugador; cualquier linea malformada o tipo desconocido lanza excepcion.
    /// </summary>
    public static class ProgresoPartida
    {
        private const string Version = "PROGRESO_V1";

        /// <summary>
        /// Serializa la partida completa: mapa, saldos, edificios, unidades
        /// (con vida y carga) y recursos fisicos restantes.
        /// </summary>
        public static string Serializar(Partida partida)
        {
            if (partida == null)
                throw new ArgumentNullException(nameof(partida));

            var texto = new StringBuilder();
            texto.Append(Version).Append('\n');
            texto.AppendFormat(
                CultureInfo.InvariantCulture,
                "Mapa={0}x{1}\n",
                partida.JugadorHumano.Mapa.Ancho,
                partida.JugadorHumano.Mapa.Alto);

            SerializarJugador(texto, "JUGADOR_HUMANO", partida.JugadorHumano);
            SerializarJugador(texto, "JUGADOR_MAQUINA", partida.JugadorMaquina);

            texto.Append("RecursosMapa:\n");

            foreach (Recurso recurso in partida.JugadorHumano.Mapa.Recursos)
            {
                texto.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0}=({1},{2},{3})\n",
                    recurso.Tipo,
                    recurso.Coordenada.X,
                    recurso.Coordenada.Y,
                    recurso.CantidadRestante);
            }

            return texto.ToString();
        }

        private static void SerializarJugador(
            StringBuilder texto,
            string seccion,
            Jugador jugador)
        {
            texto.Append('[').Append(seccion).Append("]\n");
            texto.Append("Nombre=").Append(jugador.Nombre).Append('\n');
            texto.AppendFormat(
                CultureInfo.InvariantCulture,
                "Saldo={0},{1},{2},{3},{4}\n",
                jugador.Recursos.ObtenerCantidad(TipoRecurso.Oro),
                jugador.Recursos.ObtenerCantidad(TipoRecurso.Madera),
                jugador.Recursos.ObtenerCantidad(TipoRecurso.Comida),
                jugador.Recursos.ObtenerCantidad(TipoRecurso.Piedra),
                jugador.Recursos.ObtenerCantidad(TipoRecurso.Hierro));

            texto.Append("Edificios:\n");

            foreach (Edificio edificio in jugador.Edificios)
            {
                texto.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0}=({1},{2},{3})\n",
                    edificio.GetType().Name,
                    edificio.Coordenada.X,
                    edificio.Coordenada.Y,
                    edificio.Vida);
            }

            texto.Append("Unidades:\n");

            foreach (Unidad unidad in jugador.Unidades)
            {
                if (unidad is Aldeano aldeano)
                {
                    string carga =
                        aldeano.TipoCarga.HasValue
                            ? aldeano.TipoCarga.Value.ToString()
                            : "-";

                    texto.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "{0}=({1},{2},{3},{4},{5})\n",
                        unidad.GetType().Name,
                        unidad.Coordenada.X,
                        unidad.Coordenada.Y,
                        unidad.Vida,
                        aldeano.CargaActual,
                        carga);
                }
                else
                {
                    texto.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "{0}=({1},{2},{3})\n",
                        unidad.GetType().Name,
                        unidad.Coordenada.X,
                        unidad.Coordenada.Y,
                        unidad.Vida);
                }
            }
        }

        /// <summary>
        /// Reconstruye una partida desde el texto de <see cref="Serializar"/>.
        /// </summary>
        public static Partida Deserializar(string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
                throw new ArgumentException(
                    "El contenido del progreso está vacío.",
                    nameof(contenido));

            string[] lineas = contenido.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.None);

            int i = 0;

            if (lineas.Length == 0 || lineas[i].Trim() != Version)
                throw new FormatException("Versión de progreso no reconocida.");

            i++;

            (int ancho, int alto) = LeerMapa(LeerLinea(lineas, ref i));

            var mapa = new Mapa(ancho, alto);

            Jugador humano = LeerJugador(lineas, ref i, mapa, "JUGADOR_HUMANO", TipoJugador.Humano);
            Jugador maquina = LeerJugador(lineas, ref i, mapa, "JUGADOR_MAQUINA", TipoJugador.Maquina);

            LeerRecursosMapa(lineas, ref i, mapa);

            return new Partida(humano, maquina);
        }

        private static string LeerLinea(string[] lineas, ref int i)
        {
            if (i >= lineas.Length)
                throw new FormatException("Progreso incompleto.");

            return lineas[i++].Trim();
        }

        private static (int, int) LeerMapa(string linea)
        {
            // Formato: Mapa=15x15
            int igual = linea.IndexOf('=');

            if (igual < 0)
                throw new FormatException("Falta la línea Mapa=AxA.");

            string[] partes = linea.Substring(igual + 1).Split('x');

            if (partes.Length != 2 ||
                !int.TryParse(partes[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int ancho) ||
                !int.TryParse(partes[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int alto))
            {
                throw new FormatException("Dimensiones de mapa inválidas.");
            }

            return (ancho, alto);
        }

        private static Jugador LeerJugador(
            string[] lineas,
            ref int i,
            Mapa mapa,
            string seccion,
            TipoJugador tipo)
        {
            if (LeerLinea(lineas, ref i) != "[" + seccion + "]")
                throw new FormatException("Falta la sección [" + seccion + "].");

            string nombre = Valor(LeerLinea(lineas, ref i), "Nombre");
            int[] saldo = Enteros(Valor(LeerLinea(lineas, ref i), "Saldo"), 5);

            var jugador = new Jugador(
                nombre,
                tipo,
                mapa,
                new RecursosJugador());

            jugador.Recursos.Agregar(TipoRecurso.Oro, saldo[0]);
            jugador.Recursos.Agregar(TipoRecurso.Madera, saldo[1]);
            jugador.Recursos.Agregar(TipoRecurso.Comida, saldo[2]);
            jugador.Recursos.Agregar(TipoRecurso.Piedra, saldo[3]);
            jugador.Recursos.Agregar(TipoRecurso.Hierro, saldo[4]);

            if (LeerLinea(lineas, ref i) != "Edificios:")
                throw new FormatException("Falta la sección Edificios.");

            while (i < lineas.Length &&
                !lineas[i].Trim().Equals("Unidades:") &&
                !lineas[i].Trim().StartsWith("[") &&
                !lineas[i].Trim().Equals("RecursosMapa:") &&
                lineas[i].Trim().Length > 0)
            {
                LeerEdificio(LeerLinea(lineas, ref i), jugador, mapa);
            }

            if (LeerLinea(lineas, ref i) != "Unidades:")
                throw new FormatException("Falta la sección Unidades.");

            while (i < lineas.Length &&
                !lineas[i].Trim().StartsWith("[") &&
                !lineas[i].Trim().Equals("RecursosMapa:") &&
                lineas[i].Trim().Length > 0)
            {
                LeerUnidad(LeerLinea(lineas, ref i), jugador);
            }

            return jugador;
        }

        private static void LeerEdificio(
            string linea,
            Jugador jugador,
            Mapa mapa)
        {
            // Formato: Castillo=(x,y,vida)
            int igual = linea.IndexOf('=');
            string tipo = igual < 0 ? linea : linea.Substring(0, igual);
            int[] v = Enteros(ExtraerParentesis(linea), 3);

            Edificio edificio;

            if (tipo == nameof(Castillo))
            {
                edificio = new Castillo(new Coordenada(v[0], v[1]));
            }
            else
            {
                throw new FormatException("Tipo de edificio desconocido: " + tipo);
            }

            edificio.RestaurarVida(v[2]);
            jugador.AgregarEdificio(edificio);
            mapa.ObtenerCasilla(v[0], v[1])?.Ocupar();
        }

        private static void LeerUnidad(string linea, Jugador jugador)
        {
            // Formato: Tipo=(x,y,vida) o Aldeano=(x,y,vida,carga,tipoCarga)
            int igual = linea.IndexOf('=');
            string tipo = igual < 0 ? linea : linea.Substring(0, igual);
            string[] partes = ExtraerParentesis(linea).Split(',');

            if (partes.Length < 3)
                throw new FormatException("Unidad malformada: " + linea);

            var coordenada = new Coordenada(
                Entero(partes[0]),
                Entero(partes[1]));

            Unidad unidad = FabricaUnidades.Crear(tipo, coordenada);

            if (unidad == null)
                throw new FormatException("Tipo de unidad desconocido: " + tipo);

            unidad.RestaurarVida(Entero(partes[2]));

            if (unidad is Aldeano aldeano &&
                partes.Length >= 5 &&
                int.TryParse(
                    partes[3],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int carga) &&
                carga > 0 &&
                Enum.TryParse(partes[4], true, out TipoRecurso tipoCarga))
            {
                aldeano.RecolectarDesde(
                    new Recurso(tipoCarga, coordenada),
                    carga);
            }

            jugador.AgregarUnidad(unidad);
        }

        private static void LeerRecursosMapa(
            string[] lineas,
            ref int i,
            Mapa mapa)
        {
            if (LeerLinea(lineas, ref i) != "RecursosMapa:")
                throw new FormatException("Falta la sección RecursosMapa.");

            while (i < lineas.Length && lineas[i].Trim().Length > 0)
            {
                string linea = LeerLinea(lineas, ref i);

                // Formato: Oro=(x,y,cantidad)
                int igual = linea.IndexOf('=');
                string tipo = igual < 0 ? linea : linea.Substring(0, igual);

                if (!Enum.TryParse(tipo, true, out TipoRecurso recurso))
                    throw new FormatException("Tipo de recurso desconocido: " + tipo);

                int[] v = Enteros(ExtraerParentesis(linea), 3);

                if (!mapa.ColocarRecurso(
                    new Recurso(
                        recurso,
                        new Coordenada(v[0], v[1]),
                        v[2])))
                {
                    throw new FormatException(
                        "No se pudo colocar el recurso: " + linea);
                }
            }
        }

        private static string Valor(string linea, string clave)
        {
            if (!linea.StartsWith(clave + "=", StringComparison.Ordinal))
                throw new FormatException("Falta la clave " + clave + ".");

            return linea.Substring(clave.Length + 1);
        }

        private static string ExtraerParentesis(string linea)
        {
            int abre = linea.IndexOf('(');
            int cierra = linea.LastIndexOf(')');

            if (abre < 0 || cierra < 0 || cierra <= abre)
                throw new FormatException("Coordenada malformada: " + linea);

            return linea.Substring(abre + 1, cierra - abre - 1);
        }

        private static int[] Enteros(string texto, int esperados)
        {
            string[] partes = texto.Split(',');

            if (partes.Length != esperados)
                throw new FormatException("Se esperaban " + esperados + " valores: " + texto);

            var resultado = new int[esperados];

            for (int k = 0; k < esperados; k++)
                resultado[k] = Entero(partes[k]);

            return resultado;
        }

        private static int Entero(string texto)
        {
            if (!int.TryParse(
                texto.Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int valor))
            {
                throw new FormatException("Número inválido: " + texto);
            }

            return valor;
        }
    }
}
