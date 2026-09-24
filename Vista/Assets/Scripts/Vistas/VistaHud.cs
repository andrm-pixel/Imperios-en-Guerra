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
        /// <summary>Boton que solicita la accion Mover.</summary>
        [SerializeField] private Button mover;
        /// <summary>Boton que solicita la accion Recolectar.</summary>
        [SerializeField] private Button recolectar;
        /// <summary>Boton que solicita la accion Construir.</summary>
        [SerializeField] private Button construir;
        /// <summary>Boton que abre el selector de entrenamiento.</summary>
        [SerializeField] private Button entrenar;
        /// <summary>Boton que solicita la accion Atacar.</summary>
        [SerializeField] private Button atacar;
        /// <summary>Boton fijo que ordena a todo el ejercito atacar.</summary>
        [SerializeField] private Button batalla;
        /// <summary>Panel con los tipos de unidad entrenables.</summary>
        [SerializeField] private GameObject selectorEntrenamiento;
        /// <summary>Boton para entrenar un Aldeano.</summary>
        [SerializeField] private Button entrenarAldeano;
        /// <summary>Boton para entrenar un Soldado.</summary>
        [SerializeField] private Button entrenarSoldado;
        /// <summary>Boton para entrenar un Arquero.</summary>
        [SerializeField] private Button entrenarArquero;

        /// <summary>Se emite cuando se pulsa un boton de accion.</summary>
        public event Action<string> AccionSolicitada;
        /// <summary>Se emite al elegir un tipo de unidad a entrenar.</summary>
        public event Action<string> TipoUnidadSolicitado;

        /// <summary>Suscribe los botones a sus solicitudes de accion.</summary>
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
            if (entrenarSoldado != null)
                entrenarSoldado.onClick.AddListener(SolicitarEntrenarSoldado);

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

            if (entrenarSoldado != null)
                entrenarSoldado.onClick.RemoveListener(SolicitarEntrenarSoldado);

            if (entrenarArquero != null)
                entrenarArquero.onClick.RemoveListener(SolicitarEntrenarArquero);
        }

        /// <summary>Notifica la intencion de mover la unidad.</summary>
        private void SolicitarMover() => AccionSolicitada?.Invoke("Mover");
        /// <summary>Notifica la intencion de recolectar un recurso.</summary>
        private void SolicitarRecolectar() => AccionSolicitada?.Invoke("Recolectar");
        /// <summary>Notifica la intencion de construir un edificio.</summary>
        private void SolicitarConstruir() => AccionSolicitada?.Invoke("Construir");
        /// <summary>Notifica la intencion de entrenar una unidad.</summary>
        private void SolicitarEntrenar() => AccionSolicitada?.Invoke("Entrenar");
        /// <summary>Notifica la intencion de atacar un objetivo.</summary>
        private void SolicitarAtacar() => AccionSolicitada?.Invoke("Atacar");
        /// <summary>Notifica la orden de batalla total del ejercito.</summary>
        private void SolicitarBatalla() => AccionSolicitada?.Invoke("Batalla");

        /// <summary>Notifica la eleccion del tipo Aldeano.</summary>
        private void SolicitarEntrenarAldeano() =>
            TipoUnidadSolicitado?.Invoke("Aldeano");
        /// <summary>Notifica la eleccion del tipo Soldado.</summary>
        private void SolicitarEntrenarSoldado() =>
            TipoUnidadSolicitado?.Invoke("Soldado");
        /// <summary>Notifica la eleccion del tipo Arquero.</summary>
        private void SolicitarEntrenarArquero() =>
            TipoUnidadSolicitado?.Invoke("Arquero");
        /// <summary>Muestra los recursos y la carga en camino en el HUD.</summary>
        public void MostrarRecursos(int oro, int madera, int comida, int piedra = 0, int hierro = 0)
        {
            if (recursos == null)
                return;

            recursos.text =
                $"Oro: {oro} | Madera: {madera} | Comida: {comida} | Piedra: {piedra} | Hierro: {hierro}";
        }

        /// <summary>Muestra los datos de la entidad seleccionada en una linea.</summary>
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
                $"{entidad.TipoLogico} {NombreReino(entidad.Propietario)} ({entidad.X},{entidad.Y})";

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

        /// <summary>Nombre del reino para mostrar (Griegos vs Troya).</summary>
        private static string NombreReino(string propietario)
        {
            if (propietario == "Humano")
                return "Griego";

            if (propietario == "Maquina")
                return "Troya";

            return propietario;
        }

        /// <summary>Muestra u oculta el selector de tipo de unidad.</summary>
        public void MostrarSelectorEntrenamiento(bool mostrar)
        {
            if (selectorEntrenamiento != null)
                selectorEntrenamiento.SetActive(mostrar);
        }

        /// <summary>Activa solo los botones validos para la seleccion.</summary>
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
