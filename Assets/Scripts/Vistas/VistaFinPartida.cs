using ImperiosEnGuerra.Controladores.Red;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>
    /// Pantalla de fin de partida: Victoria o Derrota con detalle,
    /// boton de revancha y boton para ver el mapa final.
    /// Se arma por codigo y nace oculta; no toca la escena.
    /// </summary>
    public class VistaFinPartida : MonoBehaviour
    {
        /// <summary>Panel raiz del fin de partida.</summary>
        private GameObject panel;
        /// <summary>Titulo grande (victoria o derrota).</summary>
        private Text titulo;
        /// <summary>Detalle del resultado.</summary>
        private Text detalle;

        /// <summary>Arma el panel oculto al arrancar.</summary>
        private void Awake()
        {
            Transform padre = transform.parent != null ? transform.parent : transform;

            panel = PanelSimple.CrearPantalla(padre, "PanelFin");
            titulo = PanelSimple.CrearTexto(panel.transform, "TituloFin", "", 56, Color.white, 120f);
            detalle = PanelSimple.CrearTexto(panel.transform, "DetalleFin", "", 18, Color.white, 10f, 140f);
            PanelSimple.CrearBoton(panel.transform, "BotonRevancha", "Jugar de nuevo", -130f, PedirRevancha);
            PanelSimple.CrearBoton(panel.transform, "BotonVerMapa", "Ver mapa", -210f, Ocultar);
            panel.SetActive(false);
        }

        /// <summary>Muestra la victoria con su detalle.</summary>
        public void MostrarVictoria(string mensaje)
        {
            Mostrar("¡VICTORIA!", new Color(1f, 0.85f, 0.2f), mensaje);
        }

        /// <summary>Muestra la derrota con su detalle.</summary>
        public void MostrarDerrota(string mensaje)
        {
            Mostrar("DERROTA", new Color(1f, 0.4f, 0.4f), mensaje);
        }

        /// <summary>Esconde la pantalla de fin.</summary>
        public void Ocultar()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        private void Mostrar(string tituloTexto, Color color, string mensaje)
        {
            if (panel == null)
                return;

            titulo.text = tituloTexto;
            titulo.color = color;
            detalle.text = string.IsNullOrEmpty(mensaje) ? "" : mensaje;
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
        }

        /// <summary>Pide una partida nueva y esconde el panel.</summary>
        private void PedirRevancha()
        {
            ControladorConexionApi conexion =
                FindAnyObjectByType<ControladorConexionApi>();

            if (conexion != null && conexion.isActiveAndEnabled)
                conexion.ReiniciarPartida();

            Ocultar();
        }
    }
}
