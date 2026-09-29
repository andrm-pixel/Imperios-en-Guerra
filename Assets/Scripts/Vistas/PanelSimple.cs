using UnityEngine;
using UnityEngine.UI;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>
    /// Ayuda para armar paneles por codigo sin editar la escena.
    /// </summary>
    internal static class PanelSimple
    {
        /// <summary>Crea una pantalla completa con fondo oscuro.</summary>
        internal static GameObject CrearPantalla(Transform padre, string nombre)
        {
            var raiz = new GameObject(nombre);
            raiz.transform.SetParent(padre, false);

            var rect = raiz.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var fondo = raiz.AddComponent<Image>();
            fondo.color = new Color(0f, 0f, 0f, 0.88f);

            raiz.transform.SetAsLastSibling();
            return raiz;
        }

        /// <summary>Crea un texto centrado a una altura dada.</summary>
        internal static Text CrearTexto(Transform padre, string nombre, string contenido, int tamano, Color color, float y, float alto = 60f)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(700f, alto);

            var texto = go.AddComponent<Text>();
            texto.text = contenido;
            texto.font = Fuente(padre);
            texto.fontSize = tamano;
            texto.color = color;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.horizontalOverflow = HorizontalWrapMode.Wrap;
            texto.verticalOverflow = VerticalWrapMode.Overflow;
            return texto;
        }

        /// <summary>Crea un boton centrado a una altura dada.</summary>
        internal static Button CrearBoton(Transform padre, string nombre, string etiqueta, float y, UnityEngine.Events.UnityAction clic)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(260f, 64f);

            var fondo = go.AddComponent<Image>();
            fondo.color = new Color(0.85f, 0.75f, 0.35f);

            var boton = go.AddComponent<Button>();
            CrearTexto(go.transform, nombre + "Texto", etiqueta, 24, Color.black, 0f, 60f);
            if (clic != null)
                boton.onClick.AddListener(clic);
            return boton;
        }

        private static Font Fuente(Transform raiz)
        {
            Text existente = Object.FindAnyObjectByType<Text>();
            if (existente != null && existente.font != null)
                return existente.font;

            Font builtin = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (builtin != null)
                return builtin;

            return Font.CreateDynamicFontFromOSFont("Arial", 16);
        }
    }
}
