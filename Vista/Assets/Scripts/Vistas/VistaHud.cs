using System;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiosEnGuerra.Vistas
{
    /// <summary>Muestra datos y opciones de interfaz; no consulta la API ni ejecuta acciones.</summary>
    public class VistaHud : MonoBehaviour
    {
        /// <summary>Texto superior con los recursos del jugador.</summary>
        [SerializeField] private Text recursos;
        /// <summary>Texto del panel con la entidad seleccionada.</summary>
        [SerializeField] private Text seleccion;
        /// <summary>Texto inferior para mensajes y errores.</summary>
        [SerializeField] private Text mensaje;
        /// <summary>Botón que solicita la acción Mover.</summary>
        [SerializeField] private Button mover;
        /// <summary>Botón que solicita la acción Recolectar.</summary>
        [SerializeField] private Button recolectar;
        /// <summary>Botón que solicita la acción Construir.</summary>
        [SerializeField] private Button construir;
        /// <summary>Botón que abre el selector de entrenamiento.</summary>
        [SerializeField] private Button entrenar;
        /// <summary>Botón que solicita la acción Atacar.</summary>
        [SerializeField] private Button atacar;
        /// <summary>Botón fijo que ordena a todo el ejército atacar.</summary>
        [SerializeField] private Button batalla;
        /// <summary>Panel con los tipos de unidad entrenables.</summary>
        [SerializeField] private GameObject selectorEntrenamiento;
        /// <summary>Botón para entrenar un Aldeano.</summary>
        [SerializeField] private Button entrenarAldeano;
        /// <summary>Botón para entrenar un Guerrero.</summary>
        [SerializeField] private Button entrenarGuerrero;
        /// <summary>Botón para entrenar un Lancero.</summary>
        [SerializeField] private Button entrenarLancero;
        /// <summary>Botón para entrenar un Arquero.</summary>
        [SerializeField] private Button entrenarArquero;

        /// <summary>Se emite cuando se pulsa un botón de acción.</summary>
        public event Action<string> AccionSolicitada;
        /// <summary>Se emite al elegir un tipo de unidad a entrenar.</summary>
        public event Action<string> TipoUnidadSolicitado;

        /// <summary>Suscribe los botones a sus solicitudes de acción.</summary>
        private void OnEnable()
        {
            if (mover != null) mover.onClick.AddListener(SolicitarMover);
            if (recolectar != null) recolectar.onClick.AddListener(SolicitarRecolectar);
            if (construir != null) construir.onClick.AddListener(SolicitarConstruir);
            if (entrenar != null) entrenar.onClick.AddListener(SolicitarEntrenar);
            if (atacar != null) atacar.onClick.AddListener(SolicitarAtacar);
            if (batalla != null) batalla.onClick.AddListener(SolicitarBatalla);

            if (entrenarAldeano != null)
                entrenarAldeano.onClick.AddListener(SolicitarEntrenarAldeano);
            if (entrenarGuerrero != null)
                entrenarGuerrero.onClick.AddListener(SolicitarEntrenarGuerrero);

            if (entrenarLancero != null)
                entrenarLancero.onClick.AddListener(SolicitarEntrenarLancero);

            if (entrenarArquero != null)
                entrenarArquero.onClick.AddListener(SolicitarEntrenarArquero);
        }

        /// <summary>Cancela las suscripciones de los botones del HUD.</summary>
        private void OnDisable()
        {
            if (mover != null) mover.onClick.RemoveListener(SolicitarMover);
            if (recolectar != null) recolectar.onClick.RemoveListener(SolicitarRecolectar);
            if (construir != null) construir.onClick.RemoveListener(SolicitarConstruir);
            if (entrenar != null) entrenar.onClick.RemoveListener(SolicitarEntrenar);
            if (atacar != null) atacar.onClick.RemoveListener(SolicitarAtacar);
            if (batalla != null) batalla.onClick.RemoveListener(SolicitarBatalla);

            if (entrenarAldeano != null)
                entrenarAldeano.onClick.RemoveListener(SolicitarEntrenarAldeano);

            if (entrenarGuerrero != null)
                entrenarGuerrero.onClick.RemoveListener(SolicitarEntrenarGuerrero);

            if (entrenarLancero != null)
                entrenarLancero.onClick.RemoveListener(SolicitarEntrenarLancero);

            if (entrenarArquero != null)
                entrenarArquero.onClick.RemoveListener(SolicitarEntrenarArquero);
        }

        /// <summary>Notifica la intención de mover la unidad.</summary>
        private void SolicitarMover() => AccionSolicitada?.Invoke("Mover");
        /// <summary>Notifica la intención de recolectar un recurso.</summary>
        private void SolicitarRecolectar() => AccionSolicitada?.Invoke("Recolectar");
        /// <summary>Notifica la intención de construir un edificio.</summary>
        private void SolicitarConstruir() => AccionSolicitada?.Invoke("Construir");
        /// <summary>Notifica la intención de entrenar una unidad.</summary>
        private void SolicitarEntrenar() => AccionSolicitada?.Invoke("Entrenar");
        /// <summary>Notifica la intención de atacar un objetivo.</summary>
        private void SolicitarAtacar() => AccionSolicitada?.Invoke("Atacar");
        /// <summary>Notifica la orden de batalla total del ejército.</summary>
        private void SolicitarBatalla() => AccionSolicitada?.Invoke("Batalla");

        /// <summary>Notifica la elección del tipo Aldeano.</summary>
        private void SolicitarEntrenarAldeano() =>
            TipoUnidadSolicitado?.Invoke("Aldeano");
        /// <summary>Notifica la elección del tipo Guerrero.</summary>
        private void SolicitarEntrenarGuerrero() =>
            TipoUnidadSolicitado?.Invoke("Guerrero");
        /// <summary>Notifica la elección del tipo Lancero.</summary>
        private void SolicitarEntrenarLancero() =>
            TipoUnidadSolicitado?.Invoke("Lancero");
        /// <summary>Notifica la elección del tipo Arquero.</summary>
        private void SolicitarEntrenarArquero() =>
            TipoUnidadSolicitado?.Invoke("Arquero");
        /// <summary>Muestra los recursos y la carga en camino en el HUD.</summary>
        public void MostrarRecursos(int oro, int madera, int comida, int piedra = 0, int hierro = 0, string tipoCarga = null, int cargaActual = 0)
        {
            if (recursos == null)
                return;

            string texto =
                $"Oro: {oro} | Madera: {madera} | Comida: {comida} | Piedra: {piedra} | Hierro: {hierro}";

            if (!string.IsNullOrWhiteSpace(tipoCarga) && cargaActual > 0)
                texto += $" (+{cargaActual} {tipoCarga} en camino)";

            recursos.text = texto;
        }

        /// <summary>Muestra los datos de la entidad seleccionada en una línea.</summary>
        public void MostrarSeleccion(EntidadSeleccionableVista entidad)
        {
            if (seleccion == null)
                return;

            if (entidad == null)
            {
                seleccion.text = "Sin selección";
                return;
            }

            string texto =
                $"{entidad.TipoLogico} {entidad.Propietario} ({entidad.X},{entidad.Y})";

            if (entidad.Categoria == CategoriaEntidadVisual.Unidad)
            {
                string estado =
                    string.IsNullOrWhiteSpace(entidad.EstadoLogico)
                        ? "Desconocido"
                        : entidad.EstadoLogico;

                string orden =
                    string.IsNullOrWhiteSpace(entidad.OrdenActiva)
                        ? "Ninguna"
                        : entidad.OrdenActiva;

                texto += $" · {estado}/{orden}";
            }

            if (entidad.Propietario == "Maquina")
                texto += " · Enemigo";

            seleccion.text = texto;
        }

        /// <summary>Muestra u oculta el selector de tipo de unidad.</summary>
        public void MostrarSelectorEntrenamiento(bool mostrar)
        {
            if (selectorEntrenamiento != null)
                selectorEntrenamiento.SetActive(mostrar);
        }

        /// <summary>Activa solo los botones válidos para la selección.</summary>
        public void MostrarOpciones(bool puedeMover, bool puedeRecolectar, bool puedeConstruir,
            bool puedeEntrenar, bool puedeAtacar)
        {
            if (mover != null) mover.gameObject.SetActive(puedeMover);
            if (recolectar != null) recolectar.gameObject.SetActive(puedeRecolectar);
            if (construir != null) construir.gameObject.SetActive(puedeConstruir);
            if (entrenar != null) entrenar.gameObject.SetActive(puedeEntrenar);
            if (atacar != null) atacar.gameObject.SetActive(puedeAtacar);
        }

        /// <summary>Muestra un mensaje informativo o de error.</summary>
        public void MostrarMensaje(string texto, bool error = false)
        {
            if (mensaje == null) return;
            mensaje.text = texto;
            mensaje.color = error ? new Color(1f, 0.55f, 0.55f) : Color.white;
        }
    }
}
