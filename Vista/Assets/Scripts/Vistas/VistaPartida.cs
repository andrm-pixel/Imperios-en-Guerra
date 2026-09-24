using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Controladores.Red.Contratos;
using UnityEngine;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>Representa los datos recibidos de la API sin modificar el estado del juego.</summary>
    public class VistaPartida : MonoBehaviour
    {
        /// <summary>Cámara que encuadra el mapa generado.</summary>
        [SerializeField] private Camera camara;
        /// <summary>Separación en mundo entre casillas lógicas.</summary>
        [SerializeField, Min(0.1f)] private float espacioCasilla = 2f;
        /// <summary>Escala aplicada a los sprites de recurso.</summary>
        [SerializeField, Min(0.01f)] private float escalaRecursos = 0.75f;
        /// <summary>Escala aplicada a los sprites de edificio.</summary>
        [SerializeField, Min(0.01f)] private float escalaEdificios = 0.58f;
        /// <summary>Escala aplicada a los sprites de unidad.</summary>
        [SerializeField, Min(0.01f)] private float escalaUnidades = 0.65f;
        /// <summary>Sprite de la baldosa de terreno.</summary>
        [SerializeField] private Sprite suelo;
        /// <summary>Sprite del recurso oro.</summary>
        [SerializeField] private Sprite oro;
        /// <summary>Sprite del recurso madera.</summary>
        [SerializeField] private Sprite madera;
        /// <summary>Sprite del recurso comida.</summary>
        [SerializeField] private Sprite comida;
        /// <summary>Sprite del recurso piedra.</summary>
        [SerializeField] private Sprite piedra;
        /// <summary>Sprite del recurso hierro.</summary>
        [SerializeField] private Sprite hierro;
        /// <summary>Sprite del centro urbano humano.</summary>
        [SerializeField] private Sprite centroHumano;
        /// <summary>Sprite del centro urbano máquina.</summary>
        [SerializeField] private Sprite centroMaquina;
        /// <summary>Sprite del aldeano humano.</summary>
        [SerializeField] private Sprite aldeanoHumano;
        /// <summary>Sprite del guerrero humano.</summary>
        [SerializeField] private Sprite guerreroHumano;
        /// <summary>Sprite del arquero humano.</summary>
        [SerializeField] private Sprite arqueroHumano;
        /// <summary>Sprite del aldeano máquina.</summary>
        [SerializeField] private Sprite aldeanoMaquina;
        /// <summary>Sprite del guerrero máquina.</summary>
        [SerializeField] private Sprite guerreroMaquina;
        /// <summary>Sprite del arquero máquina.</summary>
        [SerializeField] private Sprite arqueroMaquina;

        /// <summary>Raíz de los objetos generados para la partida.</summary>
        private GameObject contenidoGenerado;
        /// <summary>Ancho del mapa actualmente representado.</summary>
        private int anchoVisual;
        /// <summary>Alto del mapa actualmente representado.</summary>
        private int altoVisual;

        [SerializeField, Min(0.1f)]
        /// <summary>Velocidad de interpolación del movimiento visual.</summary>
        private float velocidadMovimientoVisual = 4f;

        /// <summary>Movimiento visual pendiente hacia un destino.</summary>
        private sealed class MovimientoVisualPendiente
        {
            /// <summary>Entidad que se desplaza en la vista.</summary>
            public EntidadSeleccionableVista Entidad;
            /// <summary>Posición de mundo destino del desplazamiento.</summary>
            public Vector3 Destino;
        }

        private readonly Dictionary<string, MovimientoVisualPendiente>
            /// <summary>Movimientos visuales pendientes por identificador.</summary>
            movimientosVisuales =
                new Dictionary<string, MovimientoVisualPendiente>();

        /// <summary>Avisa antes de destruir el contenido generado.</summary>
        public event System.Action AntesDeLimpiarContenido;

        /// <summary>Reconstruye toda la escena visual desde un estado.</summary>
        public void Renderizar(EstadoPartidaDto estado)
        {
            Limpiar();
            if (estado == null || estado.mapa == null)
            {
                Debug.LogError("No se puede representar una partida sin estado o mapa.", this);
                return;
            }

            if (estado.mapa.ancho <= 0 || estado.mapa.alto <= 0)
            {
                Debug.LogError("El mapa recibido no tiene dimensiones visualizables.", this);
                return;
            }

            anchoVisual = estado.mapa.ancho;
            altoVisual = estado.mapa.alto;
            contenidoGenerado = new GameObject("ContenidoGenerado");
            contenidoGenerado.transform.SetParent(transform, false);
            Transform mapa = CrearContenedor("Mapa");
            Transform recursos = CrearContenedor("Recursos");
            Transform edificios = CrearContenedor("Edificios");
            Transform unidades = CrearContenedor("Unidades");

            RenderizarMapa(estado.mapa, mapa);
            RenderizarRecursos(estado.mapa.recursos, recursos);
            RenderizarJugador(estado.jugadorHumano, true, edificios, unidades);
            RenderizarJugador(estado.jugadorMaquina, false, edificios, unidades);
            AjustarCamara(estado.mapa);
        }

        /// <summary>
        /// Aplica un snapshot sin destruir la escena generada. Esto conserva
        /// la selección y permite que varias órdenes concurrentes actualicen
        /// unidades/obras sin interrumpir al jugador.
        /// </summary>
        public void Sincronizar(EstadoPartidaDto estado)
        {
            if (estado == null || estado.mapa == null)
            {
                Debug.LogError(
                    "No se puede sincronizar una partida sin estado o mapa.",
                    this);
                return;
            }

            if (contenidoGenerado == null ||
                anchoVisual != estado.mapa.ancho ||
                altoVisual != estado.mapa.alto)
            {
                Renderizar(estado);
                return;
            }

            Transform recursos =
                contenidoGenerado.transform.Find("Recursos");
            Transform edificios =
                contenidoGenerado.transform.Find("Edificios");
            Transform unidades =
                contenidoGenerado.transform.Find("Unidades");

            if (recursos == null ||
                edificios == null ||
                unidades == null)
            {
                Renderizar(estado);
                return;
            }

            SincronizarRecursos(
                estado.mapa.recursos);

            LimpiarObrasVisuales(
                edificios);

            SincronizarJugador(
                estado.jugadorHumano,
                true,
                edificios,
                unidades);

            SincronizarJugador(
                estado.jugadorMaquina,
                false,
                edificios,
                unidades);
        }

        /// <summary>Muestra u oculta recursos según su cantidad restante.</summary>
        private void SincronizarRecursos(
            RecursoEstadoDto[] recursos)
        {
            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(
                    true);

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad == null ||
                    entidad.Categoria != CategoriaEntidadVisual.Recurso)
                {
                    continue;
                }

                RecursoEstadoDto recurso =
                    BuscarRecurso(
                        recursos,
                        entidad.TipoLogico,
                        entidad.X,
                        entidad.Y);

                bool visible =
                    recurso != null &&
                    recurso.cantidadRestante > 0;

                if (entidad.gameObject.activeSelf != visible)
                {
                    entidad.gameObject.SetActive(
                        visible);
                }
            }
        }

        /// <summary>Busca un recurso por tipo y coordenada.</summary>
        private static RecursoEstadoDto BuscarRecurso(
            RecursoEstadoDto[] recursos,
            string tipo,
            int x,
            int y)
        {
            if (recursos == null)
                return null;

            foreach (RecursoEstadoDto recurso in recursos)
            {
                if (recurso != null &&
                    recurso.coordenada != null &&
                    recurso.tipo == tipo &&
                    recurso.coordenada.x == x &&
                    recurso.coordenada.y == y)
                {
                    return recurso;
                }
            }

            return null;
        }

        /// <summary>Sincroniza edificios, obras y unidades de un jugador.</summary>
        private void SincronizarJugador(
            JugadorEstadoDto jugador,
            bool humano,
            Transform edificios,
            Transform unidades)
        {
            if (jugador == null)
                return;

            string propietario =
                humano
                    ? "Humano"
                    : "Maquina";

            SincronizarEdificios(
                jugador.edificios,
                humano,
                propietario,
                edificios);

            SincronizarObras(
                jugador.obrasConstruccion,
                humano,
                propietario,
                edificios);

            SincronizarUnidades(
                jugador.unidades,
                humano,
                propietario,
                unidades);
        }

        /// <summary>Crea los edificios nuevos y retira los destruidos.</summary>
        private void SincronizarEdificios(
            EdificioEstadoDto[] datos,
            bool humano,
            string propietario,
            Transform contenedor)
        {
            if (datos == null)
                return;

            foreach (EdificioEstadoDto edificio in datos)
            {
                if (edificio == null ||
                    edificio.coordenada == null ||
                    edificio.tipo != "CentroUrbano")
                {
                    continue;
                }

                EntidadSeleccionableVista existente =
                    BuscarEntidad(
                        CategoriaEntidadVisual.Edificio,
                        edificio.id,
                        edificio.tipo,
                        propietario,
                        edificio.coordenada.x,
                        edificio.coordenada.y,
                        ignorarCoordenada: true);

                if (existente != null)
                    continue;

                GameObject objeto =
                    CrearSprite(
                        $"Edificio_{propietario}_{edificio.tipo}_{edificio.coordenada.x}_{edificio.coordenada.y}",
                        humano
                            ? centroHumano
                            : centroMaquina,
                        edificio.coordenada.x,
                        edificio.coordenada.y,
                        20,
                        contenedor,
                        Vector3.one * escalaEdificios);

                ConfigurarSeleccionable(
                    objeto,
                    CategoriaEntidadVisual.Edificio,
                    edificio.tipo,
                    propietario,
                    edificio.coordenada,
                    edificio.id);
            }

            EliminarEntidadesDestruidas(
                CategoriaEntidadVisual.Edificio,
                propietario,
                datos.Select(edificio => edificio != null ? edificio.id : null));
        }

        /// <summary>
        /// Retira los visuales destruidos en el Modelo para que el mapa
        /// refleje bajas de combate. Solo presentación, sin reglas.
        /// </summary>
        private void EliminarEntidadesDestruidas(
            CategoriaEntidadVisual categoria,
            string propietario,
            System.Collections.Generic.IEnumerable<string> idsVivos)
        {
            var vivos =
                new System.Collections.Generic.HashSet<string>(
                    idsVivos ?? System.Array.Empty<string>());

            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(true);

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad == null ||
                    entidad.Categoria != categoria ||
                    entidad.Propietario != propietario ||
                    string.IsNullOrWhiteSpace(entidad.IdLogico) ||
                    vivos.Contains(entidad.IdLogico))
                {
                    continue;
                }

                movimientosVisuales.Remove(entidad.IdLogico);
                Destroy(entidad.gameObject);
            }
        }

        /// <summary>Redibuja las obras en curso con su progreso.</summary>
        private void SincronizarObras(
            ObraConstruccionEstadoDto[] obras,
            bool humano,
            string propietario,
            Transform contenedor)
        {
            if (obras == null)
                return;

            foreach (ObraConstruccionEstadoDto obra in obras)
            {
                if (obra == null ||
                    obra.coordenada == null ||
                    obra.tipo != "CentroUrbano")
                {
                    continue;
                }

                Sprite sprite =
                    humano
                        ? centroHumano
                        : centroMaquina;

                float factor =
                    Mathf.Lerp(
                        0.35f,
                        0.85f,
                        Mathf.Clamp01(
                            obra.progreso / 100f));

                GameObject objeto =
                    CrearSprite(
                        $"Obra_{propietario}_{obra.tipo}_{obra.coordenada.x}_{obra.coordenada.y}",
                        sprite,
                        obra.coordenada.x,
                        obra.coordenada.y,
                        18,
                        contenedor,
                        Vector3.one *
                        escalaEdificios *
                        factor);

                if (objeto == null)
                    continue;

                SpriteRenderer renderer =
                    objeto.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    Color color =
                        renderer.color;

                    color.a = 0.55f;
                    renderer.color = color;
                }
            }
        }

        /// <summary>Crea o desplaza las unidades según el estado.</summary>
        private void SincronizarUnidades(
            UnidadEstadoDto[] datos,
            bool humano,
            string propietario,
            Transform contenedor)
        {
            if (datos == null)
                return;

            foreach (UnidadEstadoDto unidad in datos)
            {
                if (unidad == null ||
                    unidad.coordenada == null ||
                    string.IsNullOrWhiteSpace(
                        unidad.id))
                {
                    continue;
                }

                EntidadSeleccionableVista existente =
                    BuscarEntidad(
                        CategoriaEntidadVisual.Unidad,
                        unidad.id,
                        unidad.tipo,
                        propietario,
                        unidad.coordenada.x,
                        unidad.coordenada.y,
                        ignorarCoordenada: true);

                if (existente != null)
                {
                    existente.ActualizarDatosLogicos(
                        unidad.coordenada.x,
                        unidad.coordenada.y,
                        unidad.estado,
                        unidad.ordenActiva);

                    movimientosVisuales[unidad.id] =
                        new MovimientoVisualPendiente
                        {
                            Entidad = existente,
                            Destino = PosicionVisual(
                                unidad.coordenada.x,
                                unidad.coordenada.y)
                        };

                    continue;
                }

                Sprite sprite =
                    ObtenerSpriteUnidad(
                        unidad.tipo,
                        humano);

                if (sprite == null)
                    continue;

                GameObject objeto =
                    CrearSprite(
                        $"Unidad_{propietario}_{unidad.tipo}_{unidad.coordenada.x}_{unidad.coordenada.y}",
                        sprite,
                        unidad.coordenada.x,
                        unidad.coordenada.y,
                        30,
                        contenedor,
                        Vector3.one * escalaUnidades);

                ConfigurarSeleccionable(
                    objeto,
                    CategoriaEntidadVisual.Unidad,
                    unidad.tipo,
                    propietario,
                    unidad.coordenada,
                    unidad.id,
                    unidad.estado,
                    unidad.ordenActiva);
            }

            EliminarEntidadesDestruidas(
                CategoriaEntidadVisual.Unidad,
                propietario,
                datos.Select(unidad => unidad != null ? unidad.id : null));
        }

        /// <summary>Busca una entidad visual por identidad y posición.</summary>
        private EntidadSeleccionableVista BuscarEntidad(
            CategoriaEntidadVisual categoria,
            string id,
            string tipo,
            string propietario,
            int x,
            int y,
            bool ignorarCoordenada = false)
        {
            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(
                    true);

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad == null ||
                    entidad.Categoria != categoria ||
                    entidad.TipoLogico != tipo ||
                    entidad.Propietario != propietario)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(id) &&
                    entidad.IdLogico != id)
                {
                    continue;
                }

                if (!ignorarCoordenada &&
                    (entidad.X != x ||
                     entidad.Y != y))
                {
                    continue;
                }

                return entidad;
            }

            return null;
        }

        /// <summary>Devuelve el sprite según tipo y bando.</summary>
        private Sprite ObtenerSpriteUnidad(
            string tipo,
            bool humano)
        {
            switch (tipo)
            {
                case "Aldeano":
                    return humano
                        ? aldeanoHumano
                        : aldeanoMaquina;
                case "Guerrero":
                    return humano
                        ? guerreroHumano
                        : guerreroMaquina;
                case "Arquero":
                    return humano
                        ? arqueroHumano
                        : arqueroMaquina;
                default:
                    Debug.LogWarning(
                        $"Tipo de unidad desconocido: {tipo}",
                        this);
                    return null;
            }
        }

        /// <summary>Elimina los visuales temporales de obras.</summary>
        private void LimpiarObrasVisuales(
            Transform edificios)
        {
            if (edificios == null)
                return;

            for (int i = edificios.childCount - 1;
                 i >= 0;
                 i--)
            {
                GameObject objeto =
                    edificios.GetChild(i).gameObject;

                if (objeto == null ||
                    !objeto.name.StartsWith("Obra_"))
                {
                    continue;
                }

                objeto.SetActive(false);

                if (Application.isPlaying)
                    Destroy(objeto);
                else
                    DestroyImmediate(objeto);
            }
        }

        /// <summary>Destruye el contenido generado y reinicia la vista.</summary>
        private void Limpiar()
        {
            AntesDeLimpiarContenido?.Invoke();
            movimientosVisuales.Clear();
            anchoVisual = 0;
            altoVisual = 0;
            if (contenidoGenerado == null)
            {
                return;
            }

            // Destroy se completa al final del frame: ocultar antes evita superposición.
            contenidoGenerado.SetActive(false);
            if (Application.isPlaying)
            {
                Destroy(contenidoGenerado);
            }
            else
            {
                DestroyImmediate(contenidoGenerado);
            }
            contenidoGenerado = null;
        }

        /// <summary>Crea un contenedor hijo para una capa visual.</summary>
        private Transform CrearContenedor(string nombre)
        {
            var contenedor = new GameObject(nombre);
            contenedor.transform.SetParent(contenidoGenerado.transform, false);
            return contenedor.transform;
        }

        /// <summary>Dibuja las baldosas de suelo del mapa.</summary>
        private void RenderizarMapa(MapaEstadoDto mapa, Transform contenedor)
        {
            if (suelo == null)
            {
                Debug.LogWarning("No está configurado el sprite de suelo.", this);
                return;
            }

            // Cubrir la casilla completa mantiene el terreno continuo al variar la separación.
            Vector3 escalaSuelo = new Vector3(
                espacioCasilla * suelo.pixelsPerUnit / suelo.rect.width,
                espacioCasilla * suelo.pixelsPerUnit / suelo.rect.height, 1f);
            for (int x = 0; x < mapa.ancho; x++)
            {
                for (int y = 0; y < mapa.alto; y++)
                {
                    GameObject baldosa =
                        CrearSprite($"Suelo_{x}_{y}", suelo, x, y, 0, contenedor, escalaSuelo);

                    if (baldosa != null)
                    {
                        SpriteRenderer fondo =
                            baldosa.GetComponent<SpriteRenderer>();

                        if (fondo != null)
                        {
                            float brillo =
                                0.92f + ((x * 7 + y * 13) % 5) * 0.02f;

                            fondo.color =
                                new Color(brillo, brillo, brillo, 1f);
                        }
                    }
                }
            }
        }

        /// <summary>Dibuja los recursos iniciales del mapa.</summary>
        private void RenderizarRecursos(RecursoEstadoDto[] recursos, Transform contenedor)
        {
            if (recursos == null)
            {
                return;
            }

            foreach (RecursoEstadoDto recurso in recursos)
            {
                if (recurso == null || recurso.coordenada == null)
                {
                    Debug.LogWarning("Recurso sin datos o coordenada; se omite su representación.", this);
                    continue;
                }

                Sprite sprite;
                Color tinte = Color.white;
                switch (recurso.tipo)
                {
                    case "Oro": sprite = oro; break;
                    case "Madera": sprite = madera; break;
                    case "Comida": sprite = comida; break;
                    case "Piedra":
                        sprite = piedra != null ? piedra : oro;
                        tinte = new Color(0.72f, 0.72f, 0.78f, 1f);
                        break;
                    case "Hierro":
                        sprite = hierro != null ? hierro : oro;
                        tinte = new Color(0.55f, 0.42f, 0.34f, 1f);
                        break;
                    default:
                        Debug.LogWarning($"Tipo de recurso desconocido: {recurso.tipo}", this);
                        continue;
                }

                GameObject objeto = CrearSprite($"Recurso_{recurso.tipo}_{recurso.coordenada.x}_{recurso.coordenada.y}",
                    sprite, recurso.coordenada.x, recurso.coordenada.y, 10, contenedor,
                    Vector3.one * escalaRecursos);

                if (objeto != null && tinte != Color.white)
                {
                    SpriteRenderer dibujo =
                        objeto.GetComponent<SpriteRenderer>();

                    if (dibujo != null)
                        dibujo.color = tinte;
                }

                ConfigurarSeleccionable(objeto, CategoriaEntidadVisual.Recurso,
                    recurso.tipo, string.Empty, recurso.coordenada);
            }
        }

        /// <summary>Dibuja edificios y unidades de un jugador.</summary>
        private void RenderizarJugador(
            JugadorEstadoDto jugador, bool humano, Transform edificios, Transform unidades)
        {
            if (jugador == null)
            {
                Debug.LogWarning("Jugador sin datos; se omite su representación.", this);
                return;
            }

            string propietario = humano ? "Humano" : "Maquina";
            if (jugador.edificios != null)
            {
                foreach (EdificioEstadoDto edificio in jugador.edificios)
                {
                    if (edificio == null || edificio.coordenada == null)
                    {
                        Debug.LogWarning("Edificio sin datos o coordenada; se omite su representación.", this);
                        continue;
                    }
                    if (edificio.tipo != "CentroUrbano")
                    {
                        Debug.LogWarning($"Tipo de edificio desconocido: {edificio.tipo}", this);
                        continue;
                    }

                    GameObject objeto = CrearSprite($"Edificio_{propietario}_{edificio.tipo}_{edificio.coordenada.x}_{edificio.coordenada.y}",
                        humano ? centroHumano : centroMaquina,
                        edificio.coordenada.x, edificio.coordenada.y, 20, edificios,
                        Vector3.one * escalaEdificios);
                    ConfigurarSeleccionable(objeto, CategoriaEntidadVisual.Edificio,
                        edificio.tipo, propietario, edificio.coordenada, edificio.id);
                }
            }

            if (jugador.obrasConstruccion != null)
            {
                foreach (ObraConstruccionEstadoDto obra in jugador.obrasConstruccion)
                {
                    if (obra == null ||
                        obra.coordenada == null ||
                        obra.tipo != "CentroUrbano")
                    {
                        continue;
                    }

                    Sprite sprite =
                        humano
                            ? centroHumano
                            : centroMaquina;

                    float factor =
                        Mathf.Lerp(
                            0.35f,
                            0.85f,
                            Mathf.Clamp01(
                                obra.progreso / 100f));

                    GameObject objeto =
                        CrearSprite(
                            $"Obra_{propietario}_{obra.tipo}_{obra.coordenada.x}_{obra.coordenada.y}",
                            sprite,
                            obra.coordenada.x,
                            obra.coordenada.y,
                            18,
                            edificios,
                            Vector3.one *
                            escalaEdificios *
                            factor);

                    if (objeto != null)
                    {
                        SpriteRenderer renderer =
                            objeto.GetComponent<SpriteRenderer>();

                        if (renderer != null)
                        {
                            Color color =
                                renderer.color;

                            color.a = 0.55f;
                            renderer.color = color;
                        }
                    }
                }
            }

            if (jugador.unidades == null)
            {
                return;
            }

            foreach (UnidadEstadoDto unidad in jugador.unidades)
            {
                if (unidad == null || unidad.coordenada == null)
                {
                    Debug.LogWarning("Unidad sin datos o coordenada; se omite su representación.", this);
                    continue;
                }

                Sprite sprite;
                switch (unidad.tipo)
                {
                    case "Aldeano": sprite = humano ? aldeanoHumano : aldeanoMaquina; break;
                    case "Guerrero": sprite = humano ? guerreroHumano : guerreroMaquina; break;
                    case "Arquero": sprite = humano ? arqueroHumano : arqueroMaquina; break;
                    default:
                        Debug.LogWarning($"Tipo de unidad desconocido: {unidad.tipo}", this);
                        continue;
                }

                GameObject objeto = CrearSprite($"Unidad_{propietario}_{unidad.tipo}_{unidad.coordenada.x}_{unidad.coordenada.y}",
                    sprite, unidad.coordenada.x, unidad.coordenada.y, 30, unidades,
                    Vector3.one * escalaUnidades);
                ConfigurarSeleccionable(
                    objeto,
                    CategoriaEntidadVisual.Unidad,
                    unidad.tipo,
                    propietario,
                    unidad.coordenada,
                    unidad.id,
                    unidad.estado,
                    unidad.ordenActiva);
            }
        }

        /// <summary>Convierte una casilla lógica a posición de mundo.</summary>
        private Vector3 PosicionVisual(float x, float y)
        {
            return new Vector3(x * espacioCasilla, y * espacioCasilla, 0f);
        }

        /// <summary>Actualiza los datos lógicos y encola el desplazamiento visual.</summary>
        public bool ActualizarMovimientoUnidad(
            string unidadId,
            int x,
            int y,
            string estadoLogico,
            string ordenActiva)
        {
            if (string.IsNullOrWhiteSpace(unidadId))
            {
                return false;
            }

            EntidadSeleccionableVista[] entidades =
                GetComponentsInChildren<EntidadSeleccionableVista>(true);

            EntidadSeleccionableVista encontrada = null;

            foreach (EntidadSeleccionableVista entidad in entidades)
            {
                if (entidad != null &&
                    entidad.Categoria == CategoriaEntidadVisual.Unidad &&
                    entidad.IdLogico == unidadId)
                {
                    encontrada = entidad;
                    break;
                }
            }

            if (encontrada == null)
            {
                return false;
            }

            encontrada.ActualizarDatosLogicos(
                x,
                y,
                estadoLogico,
                ordenActiva);

            movimientosVisuales[unidadId] =
                new MovimientoVisualPendiente
                {
                    Entidad = encontrada,
                    Destino = PosicionVisual(x, y)
                };

            return true;
        }

        /// <summary>Avanza los desplazamientos visuales pendientes.</summary>
        private void Update()
        {
            if (movimientosVisuales.Count == 0)
            {
                return;
            }

            var completados =
                new List<string>();

            foreach (KeyValuePair<string, MovimientoVisualPendiente> par
                     in movimientosVisuales)
            {
                MovimientoVisualPendiente movimiento = par.Value;

                if (movimiento == null ||
                    movimiento.Entidad == null ||
                    !movimiento.Entidad.isActiveAndEnabled)
                {
                    completados.Add(par.Key);
                    continue;
                }

                Transform transformUnidad =
                    movimiento.Entidad.transform;

                float paso =
                    velocidadMovimientoVisual *
                    Time.unscaledDeltaTime;

                transformUnidad.position =
                    Vector3.MoveTowards(
                        transformUnidad.position,
                        movimiento.Destino,
                        paso);

                if ((transformUnidad.position - movimiento.Destino)
                    .sqrMagnitude <= 0.0001f)
                {
                    transformUnidad.position =
                        movimiento.Destino;

                    completados.Add(par.Key);
                }
            }

            foreach (string unidadId in completados)
            {
                movimientosVisuales.Remove(unidadId);
            }
        }

        /// <summary>Convierte una posición de mundo a casilla lógica.</summary>
        public bool TryObtenerCoordenadaLogica(Vector3 posicionMundo, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (anchoVisual <= 0 || altoVisual <= 0 || espacioCasilla <= 0f)
            {
                return false;
            }

            float columna = posicionMundo.x / espacioCasilla;
            float fila = posicionMundo.y / espacioCasilla;
            // Casillas centradas en enteros: borde inferior incluido, superior excluido.
            // Esta comparación también rechaza NaN e infinitos sin convertirlos a int.
            if (!(columna >= -0.5f && columna < anchoVisual - 0.5f &&
                  fila >= -0.5f && fila < altoVisual - 0.5f))
            {
                return false;
            }

            x = Mathf.FloorToInt(columna + 0.5f);
            y = Mathf.FloorToInt(fila + 0.5f);
            return true;
        }

        /// <summary>Añade selección y colisionador al objeto creado.</summary>
        private void ConfigurarSeleccionable(
            GameObject objeto,
            CategoriaEntidadVisual categoria,
            string tipo,
            string propietario,
            CoordenadaEstadoDto coordenada,
            string idLogico = "",
            string estadoLogico = "",
            string ordenActiva = "")
        {
            if (objeto == null)
            {
                return;
            }

            var entidad = objeto.AddComponent<EntidadSeleccionableVista>();

            entidad.Configurar(
                categoria,
                idLogico,
                tipo,
                propietario,
                coordenada.x,
                coordenada.y,
                estadoLogico,
                ordenActiva);

            var collider = objeto.AddComponent<BoxCollider2D>();

            collider.size = entidad.Renderer.sprite.bounds.size;
            collider.offset = entidad.Renderer.sprite.bounds.center;
        }

        /// <summary>Crea un objeto con sprite, posición y orden de dibujo.</summary>
        private GameObject CrearSprite(
            string nombre, Sprite sprite, int x, int y, int orden, Transform contenedor, Vector3 escala)
        {
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite no configurado para {nombre}; se omite.", this);
                return null;
            }

            var objeto = new GameObject(nombre);
            objeto.transform.SetParent(contenedor, false);
            objeto.transform.position = PosicionVisual(x, y);
            objeto.transform.localScale = escala;
            var renderer = objeto.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = orden;
            return objeto;
        }

        /// <summary>Encuadra la cámara al tamaño del mapa.</summary>
        private void AjustarCamara(MapaEstadoDto mapa)
        {
            if (camara == null)
            {
                Debug.LogWarning("No está configurada la cámara de VistaPartida.", this);
                return;
            }

            // Franja inferior reservada a la interfaz (botones, selección y
            // mensajes) para que ningún panel tape el mapa.
            const float reservaInferior = 0.13f;
            camara.rect = new Rect(0f, reservaInferior, 1f, 1f - reservaInferior);

            camara.orthographic = true;
            camara.transform.position = PosicionVisual((mapa.ancho - 1) / 2f, (mapa.alto - 1) / 2f)
                + new Vector3(0f, 0f, -10f);
            camara.transform.rotation = Quaternion.identity;
            float aspecto = Mathf.Max(camara.aspect, 0.01f);
            float altoVista = Mathf.Max(camara.rect.height, 0.01f);
            camara.orthographicSize = (Mathf.Max(
                mapa.alto * espacioCasilla / 2f,
                mapa.ancho * espacioCasilla / (2f * aspecto)) + espacioCasilla) / altoVista;
        }
    }
}
