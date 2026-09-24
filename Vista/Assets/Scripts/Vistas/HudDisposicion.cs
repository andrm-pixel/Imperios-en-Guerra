using UnityEngine;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>
    /// Fija la posicion del HUD por codigo para que ninguna escena vieja
    /// en memoria muestre paneles cruzados. Lo llama VistaHud.Awake.
    /// Medidas en pixeles de referencia (1280x720).
    /// </summary>
    public static class HudDisposicion
    {
        /// <summary>Aplica la disposicion canonica a los hijos del panel.</summary>
        public static void Aplicar(Component hud)
        {
            if (hud == null || hud.transform == null || hud.transform.parent == null)
                return;

            Transform raiz = hud.transform.parent;

            Colocar(raiz, "Recursos", 0f, 1f, 1f, 1f, 0f, -8f, -16f, 36f, 0.5f, 1f);
            Colocar(raiz, "Seleccion", 0f, 1f, 1f, 1f, 0f, -48f, -32f, 65f, 0.5f, 1f);
            Colocar(raiz, "Mensaje", 0f, 1f, 1f, 1f, 0f, -118f, -32f, 40f, 0.5f, 1f);
            Colocar(raiz, "Mover", 0f, 0f, 0f, 0f, 16f, 16f, 72f, 46f, 0f, 0f);
            Colocar(raiz, "Recolectar", 0f, 0f, 0f, 0f, 95f, 16f, 72f, 46f, 0f, 0f);
            Colocar(raiz, "Construir", 0f, 0f, 0f, 0f, 174f, 16f, 72f, 46f, 0f, 0f);
            Colocar(raiz, "Entrenar", 0f, 0f, 0f, 0f, 253f, 16f, 72f, 46f, 0f, 0f);
            Colocar(raiz, "Atacar", 0f, 0f, 0f, 0f, 332f, 16f, 72f, 46f, 0f, 0f);
            Colocar(raiz, "Batalla", 0f, 0f, 0f, 0f, 411f, 16f, 72f, 46f, 0f, 0f);
            Colocar(raiz, "SelectorEntrenamiento", 0f, 0f, 0f, 0f, 16f, 64f, 240f, 46f, 0f, 0f);
        }

        private static void Colocar(
            Transform raiz,
            string nombre,
            float minX,
            float minY,
            float maxX,
            float maxY,
            float posX,
            float posY,
            float ancho,
            float alto,
            float pivoteX,
            float pivoteY)
        {
            Transform hijo = Buscar(raiz, nombre);

            if (hijo == null)
                return;

            if (!(hijo is RectTransform rect))
                return;

            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.anchoredPosition = new Vector2(posX, posY);
            rect.sizeDelta = new Vector2(ancho, alto);
            rect.pivot = new Vector2(pivoteX, pivoteY);
        }

        private static Transform Buscar(Transform raiz, string nombre)
        {
            foreach (Transform hijo in raiz)
            {
                if (hijo.name == nombre)
                    return hijo;

                Transform nieto = Buscar(hijo, nombre);

                if (nieto != null)
                    return nieto;
            }

            return null;
        }
    }
}
