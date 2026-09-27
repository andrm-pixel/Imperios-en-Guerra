using UnityEngine;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>
    /// Barra de vida flotante de una unidad o edificio. Solo presentacion:
    /// lee la vida que llega en el DTO y ajusta un relleno; no consulta el
    /// Modelo ni altera sus reglas. Se dibuja con dos sprites generados en
    /// codigo para no depender de ningun asset importado.
    /// </summary>
    public class BarraVidaVista : MonoBehaviour
    {
        /// <summary>Ancho de la barra en unidades locales de la barra.</summary>
        private const float AnchoBarra = 1f;
        /// <summary>Alto de la barra en unidades locales de la barra.</summary>
        private const float AltoBarra = 0.16f;
        /// <summary>Proporcion del relleno respecto al alto del fondo.</summary>
        private const float RellenoAlto = 0.72f;
        /// <summary>Tamano de la barra en unidades de mundo.</summary>
        private const float EscalaMundo = 0.65f;
        /// <summary>
        /// Separacion en unidades de mundo entre la parte alta del sprite y
        /// el centro de la barra, para que nunca lo tape.
        /// </summary>
        private const float SeparacionSobreSprite = 0.15f;

        /// <summary>Sprite blanco 1x1 compartido por todas las barras.</summary>
        private static Sprite spriteBlanco;

        /// <summary>Fondo oscuro de la barra.</summary>
        private SpriteRenderer fondo;
        /// <summary>Relleno de color que representa la vida actual.</summary>
        private SpriteRenderer relleno;

        /// <summary>
        /// Prepara la barra como hija de la entidad y la dibuja sobre ella.
        /// </summary>
        /// <param name="entidad">Sprite de la unidad o edificio.</param>
        /// <param name="vida">Vida actual.</param>
        /// <param name="vidaMaxima">Vida maxima de la entidad.</param>
        /// <param name="escalaPadre">Escala uniforme aplicada a la entidad.</param>
        /// <param name="ordenPadre">sortingOrder de la entidad.</param>
        public void Configurar(
            SpriteRenderer entidad,
            int vida,
            int vidaMaxima,
            float escalaPadre,
            int ordenPadre)
        {
            if (entidad == null)
            {
                return;
            }

            // Se contraescala para que la barra conserve un tamano legible
            // aunque la entidad se dibuje con escala 0.65 (unidad) o 0.58.
            float escalaSeguro = Mathf.Max(escalaPadre, 0.0001f);
            float factor = EscalaMundo / escalaSeguro;

            transform.localScale = Vector3.one * factor;

            // La altura se calcula desde el sprite real, no con un valor fijo:
            // asi la barra queda justo encima y nunca encima del sprite,
            // tanto en unidades pequenas como en edificios grandes.
            float medioSprite = entidad.sprite == null
                ? 0f
                : entidad.sprite.bounds.extents.y * escalaSeguro;

            // localPosition esta en espacio local de la entidad, que ya
            // tiene escala escalaSeguro: se divide por ella para Translate
            // el desplazamiento a unidades de mundo.
            transform.localPosition = new Vector3(
                0f,
                (medioSprite + SeparacionSobreSprite) / escalaSeguro,
                0f);

            if (fondo == null)
            {
                fondo = CrearCapa("Fondo", ordenPadre + 1, new Color(0f, 0f, 0f, 0.75f));
            }

            if (relleno == null)
            {
                relleno = CrearCapa("Relleno", ordenPadre + 2, Color.green);
            }

            Actualizar(vida, vidaMaxima);
        }

        /// <summary>
        /// Ajusta el relleno y su color segun la vida recibida.
        /// </summary>
        /// <param name="vida">Vida actual.</param>
        /// <param name="vidaMaxima">Vida maxima de la entidad.</param>
        public void Actualizar(int vida, int vidaMaxima)
        {
            if (fondo == null || relleno == null)
            {
                return;
            }

            if (vidaMaxima <= 0)
            {
                // Sin vida maxima conocida (por ejemplo una entidad sin
                // combate) no se dibuja barra para no ensuciar el mapa.
                gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            float proporcion = Mathf.Clamp01((float)vida / vidaMaxima);

            // El relleno se ancla a la izquierda: su borde izquierdo queda
            // fijo y solo crece hacia la derecha al recuperar vida.
            relleno.transform.localScale = new Vector3(
                Mathf.Max(proporcion, 0.0001f),
                AltoBarra * RellenoAlto,
                1f);
            relleno.transform.localPosition = new Vector3(
                -AnchoBarra / 2f + (AnchoBarra * proporcion) / 2f,
                0f,
                0f);
            relleno.color = ColorPorProporcion(proporcion);
        }

        /// <summary>Devuelve un color legible segun la proporcion de vida.</summary>
        private static Color ColorPorProporcion(float proporcion)
        {
            if (proporcion > 0.5f)
            {
                return new Color(0.25f, 0.85f, 0.25f);
            }

            if (proporcion > 0.25f)
            {
                return new Color(0.95f, 0.80f, 0.20f);
            }

            return new Color(0.90f, 0.25f, 0.20f);
        }

        /// <summary>Crea una capa de la barra con el sprite blanco compartido.</summary>
        private SpriteRenderer CrearCapa(string nombre, int orden, Color color)
        {
            var capa = new GameObject(nombre);
            capa.transform.SetParent(transform, false);
            capa.transform.localScale = Vector3.one;
            capa.transform.localPosition = Vector3.zero;

            var renderer = capa.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteBlanco();
            renderer.sortingOrder = orden;
            renderer.color = color;
            return renderer;
        }

        /// <summary>Crea una vez el sprite blanco 1x1 usado por las barras.</summary>
        private static Sprite SpriteBlanco()
        {
            if (spriteBlanco != null)
            {
                return spriteBlanco;
            }

            var textura = new Texture2D(1, 1);
            textura.name = "BarraVida_Blanco";
            textura.filterMode = FilterMode.Point;
            textura.SetPixel(0, 0, Color.white);
            textura.Apply();
            textura.hideFlags = HideFlags.DontSave;

            spriteBlanco = Sprite.Create(
                textura,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            spriteBlanco.name = "BarraVida_Blanco";
            return spriteBlanco;
        }
    }
}
