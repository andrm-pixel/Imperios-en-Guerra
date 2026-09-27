using ImperiosEnGuerra.Vistas;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ImperiosEnGuerra.Controladores
{
    /// <summary>Coordina la seleccion local; no envia ordenes de gameplay.</summary>
    public class ControladorSeleccion : MonoBehaviour
    {
        /// <summary>Camara usada para convertir el clic a mundo.</summary>
        [SerializeField] private Camera camara;
        /// <summary>Vista consultada para coordenadas y entidades.</summary>
        [SerializeField] private VistaPartida vistaPartida;

        /// <summary>Entidad actualmente seleccionada.</summary>
        public EntidadSeleccionableVista SeleccionActual { get; private set; }
        /// <summary>Identificador de la unidad seleccionada, si la hay.</summary>
        public string IdUnidadSeleccionada =>
            SeleccionActual != null && SeleccionActual.Categoria == CategoriaEntidadVisual.Unidad
                ? SeleccionActual.IdLogico
                : string.Empty;
        /// <summary>Avisa cuando cambia la entidad seleccionada.</summary>
        public event System.Action<EntidadSeleccionableVista> SeleccionCambio;
        /// <summary>Indica si se espera una casilla destino.</summary>
        public bool CapturandoDestino { get; private set; }
        /// <summary>Indica si el destino debe ser un recurso.</summary>
        public bool CapturandoRecurso { get; private set; }
        /// <summary>Indica si se espera una entidad objetivo.</summary>
        public bool CapturandoObjetivoEntidad { get; private set; }
        /// <summary>Se emite al elegir una casilla destino valida.</summary>
        public event System.Action<int, int> DestinoSeleccionado;
        /// <summary>Se emite al elegir una entidad objetivo.</summary>
        public event System.Action<EntidadSeleccionableVista> ObjetivoEntidadSeleccionado;
        /// <summary>Se emite al cancelar la captura en curso.</summary>
        public event System.Action CapturaCancelada;
        /// <summary>Vista suscrita para limpiar la seleccion.</summary>
        private VistaPartida vistaSuscrita;

        /// <summary>Inicia la espera de una casilla destino.</summary>
        public void IniciarCapturaDestino()
        {
            CapturandoObjetivoEntidad = false;
            CapturandoRecurso = false;
            CapturandoDestino = true;
        }

        /// <summary>Inicia la espera de un recurso como destino.</summary>
        public void IniciarCapturaRecurso()
        {
            CapturandoObjetivoEntidad = false;
            CapturandoRecurso = true;
            CapturandoDestino = true;
        }

        /// <summary>Termina la espera de casilla destino.</summary>
        public void FinalizarCapturaDestino()
        {
            CapturandoDestino = false;
            CapturandoRecurso = false;
        }

        /// <summary>Inicia la espera de una entidad objetivo.</summary>
        public void IniciarCapturaObjetivoEntidad()
        {
            CapturandoDestino = false;
            CapturandoRecurso = false;
            CapturandoObjetivoEntidad = true;
        }

        /// <summary>Termina la espera de entidad objetivo.</summary>
        public void FinalizarCapturaObjetivoEntidad() =>
            CapturandoObjetivoEntidad = false;

        /// <summary>Cancela la captura y notifica la cancelacion.</summary>
        private void CancelarCapturaDestino()
        {
            CapturandoDestino = false;
            CapturandoRecurso = false;
            CapturandoObjetivoEntidad = false;
            CapturaCancelada?.Invoke();
        }

        /// <summary>Suscribe la limpieza de seleccion a la vista.</summary>
        private void OnEnable()
        {
            vistaSuscrita = vistaPartida;
            if (vistaSuscrita != null)
            {
                vistaSuscrita.AntesDeLimpiarContenido += LimpiarSeleccion;
            }
            else
            {
                Debug.LogError("ControladorSeleccion necesita una VistaPartida configurada.", this);
            }
        }

        /// <summary>Cancela suscripciones y limpia la seleccion.</summary>
        private void OnDisable()
        {
            if (vistaSuscrita != null)
            {
                vistaSuscrita.AntesDeLimpiarContenido -= LimpiarSeleccion;
            }
            vistaSuscrita = null;
            LimpiarSeleccion();
            FinalizarCapturaDestino();
            FinalizarCapturaObjetivoEntidad();
        }

        /// <summary>Gestiona Escape y clics de seleccion o destino.</summary>
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (CapturandoDestino || CapturandoObjetivoEntidad)
                    CancelarCapturaDestino();
                else
                    LimpiarSeleccion();
                return;
            }

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (camara == null || vistaPartida == null)
            {
                Debug.LogError("Configure cámara y VistaPartida en ControladorSeleccion.", this);
                return;
            }

            Vector2 pantalla = Mouse.current.position.ReadValue();
            if (EventSystem.current != null)
            {
                var puntero = new PointerEventData(EventSystem.current) { position = pantalla };
                var resultados = new List<RaycastResult>();
                EventSystem.current.RaycastAll(puntero, resultados);
                if (resultados.Exists(resultado => resultado.module is GraphicRaycaster))
                {
                    return;
                }
            }
            if (!camara.pixelRect.Contains(pantalla))
            {
                if (!CapturandoDestino) LimpiarSeleccion();
                return;
            }

            // VistaPartida representa las entidades en el plano mundial z=0.
            Vector3 mundo = camara.ScreenToWorldPoint(
                new Vector3(pantalla.x, pantalla.y, -camara.transform.position.z));
            if (CapturandoDestino)
            {
                if (CapturandoRecurso)
                {
                    EntidadSeleccionableVista recurso =
                        ObtenerRecursoEn(mundo);

                    if (recurso != null)
                    {
                        DestinoSeleccionado?.Invoke(
                            recurso.X,
                            recurso.Y);
                    }

                    // En recoleccion solo una entidad Recurso valida puede
                    // convertirse en objetivo logico.
                    return;
                }

                if (vistaPartida.TryObtenerCoordenadaLogica(mundo, out int x, out int y))
                    DestinoSeleccionado?.Invoke(x, y);
                // El clic de destino se consume incluso si queda fuera del mapa.
                return;
            }

            EntidadSeleccionableVista candidata = ObtenerEntidadEn(mundo);

            if (CapturandoObjetivoEntidad)
            {
                ObjetivoEntidadSeleccionado?.Invoke(candidata);
                return;
            }

            Seleccionar(candidata);
        }

        /// <summary>Busca el recurso mas cercano al punto indicado.</summary>
        private EntidadSeleccionableVista ObtenerRecursoEn(
            Vector3 mundo)
        {
            Physics2D.SyncTransforms();

            EntidadSeleccionableVista candidata = null;
            float mejorDistancia = float.PositiveInfinity;

            foreach (Collider2D collider in Physics2D.OverlapPointAll(mundo))
            {
                var entidad =
                    collider.GetComponent<EntidadSeleccionableVista>();

                if (entidad == null ||
                    !entidad.isActiveAndEnabled ||
                    entidad.Categoria != CategoriaEntidadVisual.Recurso ||
                    !entidad.transform.IsChildOf(vistaPartida.transform) ||
                    entidad.Renderer == null ||
                    !entidad.Renderer.enabled)
                {
                    continue;
                }

                Vector2 centroVisual =
                    entidad.Renderer.bounds.center;

                float distancia =
                    ((Vector2)mundo - centroVisual).sqrMagnitude;

                if (candidata == null ||
                    distancia < mejorDistancia)
                {
                    candidata = entidad;
                    mejorDistancia = distancia;
                }
            }

            return candidata;
        }

        /// <summary>Busca la entidad visible bajo el punto indicado.</summary>
        private EntidadSeleccionableVista ObtenerEntidadEn(Vector3 mundo)
        {
            Physics2D.SyncTransforms();
            EntidadSeleccionableVista candidata = null;

            foreach (Collider2D collider in Physics2D.OverlapPointAll(mundo))
            {
                var entidad = collider.GetComponent<EntidadSeleccionableVista>();
                if (entidad == null || !entidad.isActiveAndEnabled ||
                    !entidad.transform.IsChildOf(vistaPartida.transform) ||
                    entidad.Renderer == null || !entidad.Renderer.enabled)
                {
                    continue;
                }

                if (candidata == null ||
                    entidad.Renderer.sortingOrder > candidata.Renderer.sortingOrder ||
                    (entidad.Renderer.sortingOrder == candidata.Renderer.sortingOrder &&
                     entidad.transform.GetSiblingIndex() < candidata.transform.GetSiblingIndex()))
                {
                    candidata = entidad;
                }
            }

            return candidata;
        }

        /// <summary>Aplica la nueva seleccion y notifica el cambio.</summary>
        private void Seleccionar(EntidadSeleccionableVista entidad)
        {
            if (SeleccionActual == entidad)
            {
                return;
            }

            LimpiarSeleccion();
            SeleccionActual = entidad;
            if (SeleccionActual != null)
            {
                SeleccionActual.MostrarSeleccion();
                string id = string.IsNullOrEmpty(entidad.IdLogico) ? string.Empty : $" id={entidad.IdLogico}";
                Debug.Log($"Seleccionado: {entidad.Categoria} {entidad.TipoLogico} {entidad.Propietario}{id} ({entidad.X},{entidad.Y})", entidad);
            }
            SeleccionCambio?.Invoke(SeleccionActual);
        }

        /// <summary>Quita el resaltado y deja la seleccion vacia.</summary>
        public void LimpiarSeleccion()
        {
            if (SeleccionActual != null)
            {
                SeleccionActual.OcultarSeleccion();
                Debug.Log("Selección limpiada", this);
            }
            SeleccionActual = null;
            SeleccionCambio?.Invoke(null);
        }
    }
}
