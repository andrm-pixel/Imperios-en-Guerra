using UnityEngine;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>
    /// Pantalla de inicio: titulo, ayuda corta y boton Jugar.
    /// Se arma por codigo al despertar; no toca la escena.
    /// </summary>
    public class VistaInicio : MonoBehaviour
    {
        /// <summary>Panel raiz de la pantalla de inicio.</summary>
        private GameObject panel;

        /// <summary>Arma el panel visible al arrancar.</summary>
        private void Awake()
        {
            Transform padre = transform.parent != null ? transform.parent : transform;

            panel = PanelSimple.CrearPantalla(padre, "PanelInicio");
            PanelSimple.CrearTexto(panel.transform, "TituloInicio", "IMPERIOS EN GUERRA", 48, Color.white, 130f);
            PanelSimple.CrearTexto(panel.transform, "SubtituloInicio", "Griegos (tu) vs Troya (maquina)", 24, Color.yellow, 50f);
            PanelSimple.CrearTexto(
                panel.transform,
                "AyudaInicio",
                "Selecciona con clic.\nMover / Recolectar / Construir / Entrenar / Atacar / Batalla.\nF5 guarda - F9 carga.\nDestruye su castillo Y su ejercito.",
                18, Color.white, -60f, 150f);
            PanelSimple.CrearBoton(panel.transform, "BotonJugar", "JUGAR", -190f, Ocultar);
            Debug.Log("PanelInicio creado: " + panel.transform.childCount + " hijos.", panel);
        }
        }

        /// <summary>Esconde la pantalla de inicio.</summary>
        private void Ocultar()
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }
}
