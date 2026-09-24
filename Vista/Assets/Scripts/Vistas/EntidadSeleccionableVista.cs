using UnityEngine;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>Categoría visual de una entidad seleccionable.</summary>
    public enum CategoriaEntidadVisual
    {
        /// <summary>Unidad militar o aldeano.</summary>
        Unidad,
        /// <summary>Edificio construido u obra.</summary>
        Edificio,
        /// <summary>Recurso recolectable del mapa.</summary>
        Recurso
    }

    /// <summary>Metadatos y resaltado de una representación, sin reglas del juego.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class EntidadSeleccionableVista : MonoBehaviour
    {
        /// <summary>Categoría de la entidad representada.</summary>
        public CategoriaEntidadVisual Categoria { get; private set; }
        /// <summary>Identificador lógico del Modelo.</summary>
        public string IdLogico { get; private set; }
        /// <summary>Tipo lógico (Aldeano, Oro, CentroUrbano...).</summary>
        public string TipoLogico { get; private set; }
        /// <summary>Propietario (Humano, Maquina o vacío).</summary>
        public string Propietario { get; private set; }
        /// <summary>Columna lógica de la entidad.</summary>
        public int X { get; private set; }
        /// <summary>Fila lógica de la entidad.</summary>
        public int Y { get; private set; }
        /// <summary>Estado lógico de la unidad.</summary>
        public string EstadoLogico { get; private set; }
        /// <summary>Orden activa de la unidad.</summary>
        public string OrdenActiva { get; private set; }

        /// <summary>Renderizador asociado a la entidad.</summary>
        public SpriteRenderer Renderer { get; private set; }

        /// <summary>Color original antes del resaltado de selección.</summary>
        private Color colorOriginal;
        /// <summary>Indica si la entidad muestra el resaltado.</summary>
        private bool seleccionada;

        /// <summary>Configura una entidad de recurso o edificio sin identidad.</summary>
        public void Configurar(
            CategoriaEntidadVisual categoria,
            string tipo,
            string propietario,
            int x,
            int y)
        {
            Configurar(
                categoria,
                string.Empty,
                tipo,
                propietario,
                x,
                y);
        }

        public void Configurar(
            CategoriaEntidadVisual categoria,
            string idLogico,
            string tipo,
            string propietario,
            int x,
            int y,
            /// <summary>Configura todos los datos lógicos de la entidad.</summary>
            string estadoLogico = "",
            string ordenActiva = "")
        {
            OcultarSeleccion();

            Categoria = categoria;
            IdLogico = idLogico ?? string.Empty;
            TipoLogico = tipo;
            Propietario = propietario;
            X = x;
            Y = y;

            EstadoLogico = estadoLogico ?? string.Empty;
            OrdenActiva = ordenActiva ?? string.Empty;

            Renderer = GetComponent<SpriteRenderer>();
            colorOriginal = Renderer.color;
        }

        /// <summary>Actualiza posición, estado y orden sin mover el sprite.</summary>
        public void ActualizarDatosLogicos(
            int x,
            int y,
            string estadoLogico,
            string ordenActiva)
        {
            X = x;
            Y = y;
            EstadoLogico = estadoLogico ?? string.Empty;
            OrdenActiva = ordenActiva ?? string.Empty;
        }

        /// <summary>Aplica el tinte de resaltado a la entidad.</summary>
        public void MostrarSeleccion()
        {
            if (Renderer == null || seleccionada)
            {
                return;
            }

            colorOriginal = Renderer.color;
            // Atenuar verde produce un tinte visible conservando los canales rojo y azul.
            Renderer.color = colorOriginal * new Color(1f, 0.45f, 1f, 1f);
            seleccionada = true;
        }

        /// <summary>Restaura el color original de la entidad.</summary>
        public void OcultarSeleccion()
        {
            if (seleccionada && Renderer != null)
            {
                Renderer.color = colorOriginal;
            }
            seleccionada = false;
        }

        /// <summary>Limpia el resaltado al desactivar el objeto.</summary>
        private void OnDisable()
        {
            OcultarSeleccion();
        }
    }
}
