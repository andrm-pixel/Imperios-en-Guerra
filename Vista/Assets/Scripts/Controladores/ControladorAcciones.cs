using ImperiosEnGuerra.Vistas;
using ImperiosEnGuerra.Controladores.Red;
using ImperiosEnGuerra.Modelo.Reglas;
using UnityEngine;

namespace ImperiosEnGuerra.Controladores
{
    /// <summary>
    /// Coordina opciones de interfaz.
    /// Preparar una intención no autoriza ni ejecuta gameplay.
    /// </summary>
    public class ControladorAcciones : MonoBehaviour
    {
        [SerializeField] private ControladorSeleccion controladorSeleccion;
        [SerializeField] private VistaHud vistaHud;
        [SerializeField] private ControladorConexionApi conexionApi;

        private string unidadIdPendiente;
        private EntidadSeleccionableVista unidadPendiente;
        private string accionPendiente;
        private EntidadSeleccionableVista edificioPendiente;
        private string tipoUnidadPendiente;

        private string ultimaEntidadMostrada;
        private string ultimoEstadoMostrado;
        private string ultimaOrdenMostrada;

        private bool EsperandoObjetivo =>
            !string.IsNullOrEmpty(accionPendiente);

        private void OnEnable()
        {
            if (controladorSeleccion != null)
            {
                controladorSeleccion.SeleccionCambio += ActualizarSeleccion;
                controladorSeleccion.DestinoSeleccionado += EnviarObjetivo;
                controladorSeleccion.ObjetivoEntidadSeleccionado += EnviarObjetivoAtaque;
                controladorSeleccion.CapturaCancelada += CancelarCaptura;
            }

            if (vistaHud != null)
            {
                vistaHud.AccionSolicitada += PrepararAccion;
                vistaHud.TipoUnidadSolicitado += SeleccionarTipoUnidad;
            }

            ActualizarSeleccion(
                controladorSeleccion == null
                    ? null
                    : controladorSeleccion.SeleccionActual);
        }

        private void OnDisable()
        {
            LimpiarCaptura();

            if (controladorSeleccion != null)
            {
                controladorSeleccion.SeleccionCambio -= ActualizarSeleccion;
                controladorSeleccion.DestinoSeleccionado -= EnviarObjetivo;
                controladorSeleccion.ObjetivoEntidadSeleccionado -= EnviarObjetivoAtaque;
                controladorSeleccion.CapturaCancelada -= CancelarCaptura;
            }

            if (vistaHud != null)
            {
                vistaHud.AccionSolicitada -= PrepararAccion;
                vistaHud.TipoUnidadSolicitado -= SeleccionarTipoUnidad;
            }
        }

        private void ActualizarSeleccion(EntidadSeleccionableVista entidad)
        {
            bool cancelar = EsperandoObjetivo;
            string accionCancelada = accionPendiente;

            if (cancelar)
                LimpiarCaptura();

            if (vistaHud == null)
                return;

            vistaHud.MostrarSeleccion(entidad);

            ActualizarOpcionesHud(
                entidad);

            RegistrarEstadoMostrado(
                entidad);

            vistaHud.MostrarMensaje(
                cancelar
                    ? ObtenerMensajeCancelacion(accionCancelada)
                    : "");
        }

        private void Update()
        {
            if (EsperandoObjetivo &&
                !ConservaSeleccion())
            {
                CancelarCaptura();
                return;
            }

            if (!EsperandoObjetivo)
            {
                RefrescarSeleccionSiCambioEstado();
            }
        }

        private void RefrescarSeleccionSiCambioEstado()
        {
            if (vistaHud == null ||
                controladorSeleccion == null)
            {
                return;
            }

            EntidadSeleccionableVista entidad =
                controladorSeleccion.SeleccionActual;

            if (entidad == null)
            {
                if (!string.IsNullOrEmpty(
                        ultimaEntidadMostrada))
                {
                    vistaHud.MostrarSeleccion(null);
                    ActualizarOpcionesHud(null);
                    RegistrarEstadoMostrado(null);
                }

                return;
            }

            string identidad =
                !string.IsNullOrWhiteSpace(
                    entidad.IdLogico)
                    ? entidad.IdLogico
                    : $"{entidad.Categoria}:{entidad.Propietario}:{entidad.TipoLogico}:{entidad.X}:{entidad.Y}";

            if (identidad == ultimaEntidadMostrada &&
                entidad.EstadoLogico == ultimoEstadoMostrado &&
                entidad.OrdenActiva == ultimaOrdenMostrada)
            {
                return;
            }

            vistaHud.MostrarSeleccion(
                entidad);

            ActualizarOpcionesHud(
                entidad);

            RegistrarEstadoMostrado(
                entidad);
        }

        private void ActualizarOpcionesHud(
            EntidadSeleccionableVista entidad)
        {
            if (vistaHud == null)
                return;

            vistaHud.MostrarOpciones(
                PermiteOpcion(entidad, "Mover"),
                PermiteOpcion(entidad, "Recolectar"),
                PermiteOpcion(entidad, "Construir"),
                PermiteOpcion(entidad, "Entrenar"),
                PermiteOpcion(entidad, "Atacar"));
        }

        private void RegistrarEstadoMostrado(
            EntidadSeleccionableVista entidad)
        {
            if (entidad == null)
            {
                ultimaEntidadMostrada =
                    string.Empty;

                ultimoEstadoMostrado =
                    string.Empty;

                ultimaOrdenMostrada =
                    string.Empty;

                return;
            }

            ultimaEntidadMostrada =
                !string.IsNullOrWhiteSpace(
                    entidad.IdLogico)
                    ? entidad.IdLogico
                    : $"{entidad.Categoria}:{entidad.Propietario}:{entidad.TipoLogico}:{entidad.X}:{entidad.Y}";

            ultimoEstadoMostrado =
                entidad.EstadoLogico ?? string.Empty;

            ultimaOrdenMostrada =
                entidad.OrdenActiva ?? string.Empty;
        }

        private bool ConservaSeleccion()
        {
            if (controladorSeleccion == null ||
                !controladorSeleccion.isActiveAndEnabled)
            {
                return false;
            }

            if (accionPendiente == "Entrenar")
            {
                return edificioPendiente != null &&
                    edificioPendiente.isActiveAndEnabled &&
                    controladorSeleccion.SeleccionActual == edificioPendiente &&
                    PermiteOpcion(edificioPendiente, accionPendiente);
            }

            return unidadPendiente != null &&
                unidadPendiente.isActiveAndEnabled &&
                controladorSeleccion.SeleccionActual == unidadPendiente &&
                unidadPendiente.IdLogico == unidadIdPendiente &&
                PermiteOpcion(unidadPendiente, accionPendiente);
        }

        private void LimpiarCaptura()
        {
            unidadIdPendiente = null;
            unidadPendiente = null;
            accionPendiente = null;
            edificioPendiente = null;
            tipoUnidadPendiente = null;

            if (controladorSeleccion != null)
            {
                controladorSeleccion.FinalizarCapturaDestino();
                controladorSeleccion.FinalizarCapturaObjetivoEntidad();
            }

            if (vistaHud != null)
                vistaHud.MostrarSelectorEntrenamiento(false);
        }

        private void CancelarCaptura()
        {
            string accionCancelada = accionPendiente;

            LimpiarCaptura();

            // Sin captura en curso, Esc saca a la unidad seleccionada
            // de su orden activa (recolección continua, movimiento...).
            if (string.IsNullOrEmpty(accionCancelada))
            {
                var seleccionada =
                    controladorSeleccion == null
                        ? null
                        : controladorSeleccion.SeleccionActual;

                if (seleccionada != null &&
                    !string.IsNullOrWhiteSpace(seleccionada.IdLogico) &&
                    !string.IsNullOrWhiteSpace(seleccionada.OrdenActiva) &&
                    conexionApi != null &&
                    conexionApi.isActiveAndEnabled)
                {
                    conexionApi.CancelarOrdenesDe(seleccionada.IdLogico);
                    return;
                }
            }

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    ObtenerMensajeCancelacion(accionCancelada));
            }
        }

        private void EnviarObjetivo(int x, int y)
        {
            if (!EsperandoObjetivo)
                return;

            if (!ConservaSeleccion())
            {
                CancelarCaptura();
                return;
            }

            string id = unidadIdPendiente;
            string accion = accionPendiente;
            string tipoUnidad = tipoUnidadPendiente;

            int edificioX =
                edificioPendiente != null
                    ? edificioPendiente.X
                    : 0;

            int edificioY =
                edificioPendiente != null
                    ? edificioPendiente.Y
                    : 0;

            LimpiarCaptura();

            if (conexionApi == null ||
                !conexionApi.isActiveAndEnabled)
            {
                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "La conexión con la API no está disponible.",
                        true);
                }

                return;
            }

            if (accion == "Entrenar")
            {
                conexionApi.Entrenar(
                    edificioX,
                    edificioY,
                    tipoUnidad,
                    x,
                    y);

                return;
            }

            if (accion == "Mover")
            {
                conexionApi.MoverUnidad(id, x, y);
                return;
            }

            if (accion == "Recolectar")
            {
                conexionApi.IniciarRecoleccion(id, x, y);
                return;
            }

            if (accion == "Construir")
            {
                conexionApi.Construir(
                    id,
                    ReglasAcciones.TipoCentroUrbano,
                    x,
                    y);

                return;
            }

            if (vistaHud != null)
            {
                vistaHud.MostrarMensaje(
                    "La acción preparada no reconoce un objetivo válido.",
                    true);
            }
        }

        private void EnviarObjetivoAtaque(EntidadSeleccionableVista objetivo)
        {
            if (accionPendiente != "Atacar")
                return;

            if (!ConservaSeleccion())
            {
                CancelarCaptura();
                return;
            }

            if (!EsObjetivoAtaqueValido(objetivo))
            {
                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "Selecciona una unidad enemiga válida como objetivo.",
                        true);
                }

                return;
            }

            string atacanteId = unidadIdPendiente;
            string objetivoId = objetivo.IdLogico;

            LimpiarCaptura();

            if (conexionApi == null ||
                !conexionApi.isActiveAndEnabled)
            {
                if (vistaHud != null)
                {
                    vistaHud.MostrarMensaje(
                        "La conexión con la API no está disponible.",
                        true);
                }

                return;
            }

            conexionApi.Atacar(
                atacanteId,
                objetivoId);
        }

        private static bool EsObjetivoAtaqueValido(
            EntidadSeleccionableVista objetivo)
        {
            // Puente: la regla vive en el Modelo.
            if (objetivo == null || !objetivo.isActiveAndEnabled)
                return false;
            return ReglasAcciones.EsObjetivoAtaqueValido(
                objetivo.Categoria.ToString(),
                objetivo.Propietario,
                objetivo.IdLogico);
        }

        private bool PuedeIniciarAccion(string accion)
        {
            if (conexionApi == null ||
                !conexionApi.isActiveAndEnabled)
            {
                return false;
            }

            if (accion == "Mover")
                return conexionApi.PuedeIniciarMovimiento;

            if (accion == "Recolectar")
                return conexionApi.PuedeIniciarRecoleccion;

            if (accion == "Construir")
                return conexionApi.PuedeIniciarConstruccion;

            if (accion == "Entrenar")
                return conexionApi.PuedeIniciarEntrenamiento;

            if (accion == "Atacar")
                return conexionApi.PuedeIniciarAtaque;

            return false;
        }

        private static bool PermiteOpcion(
            EntidadSeleccionableVista entidad,
            string accion)
        {
            // Puente: las reglas viven en el Modelo (ReglasAcciones).
            if (entidad == null || !entidad.isActiveAndEnabled)
                return false;
            if (accion == "Entrenar")
                return ReglasAcciones.PermiteEntrenar(entidad.Propietario, entidad.Categoria.ToString(), entidad.TipoLogico);
            if (accion == "Mover")
                return ReglasAcciones.PermiteMover(entidad.Propietario, entidad.Categoria.ToString(), entidad.OrdenActiva);
            if (accion == "Recolectar" || accion == "Construir")
                return ReglasAcciones.PermiteRecolectar(entidad.Propietario, entidad.Categoria.ToString(), entidad.TipoLogico, entidad.OrdenActiva);
            if (accion == "Atacar")
                return ReglasAcciones.PermiteAtacar(entidad.Propietario, entidad.Categoria.ToString(), entidad.TipoLogico, entidad.OrdenActiva);
            return false;
        }

        private void PrepararAccion(string accion)
        {
            if (vistaHud == null)
                return;

            LimpiarCaptura();

            var entidad =
                controladorSeleccion == null
                    ? null
                    : controladorSeleccion.SeleccionActual;

            if (!PermiteOpcion(entidad, accion))
            {
                vistaHud.MostrarMensaje(
                    "Selecciona una entidad humana apropiada para esta opción.",
                    true);

                return;
            }

            if (accion == "Entrenar")
            {
                if (!PuedeIniciarAccion(accion))
                {
                    vistaHud.MostrarMensaje(
                        "La conexión con la API no está disponible.",
                        true);

                    return;
                }

                edificioPendiente = entidad;
                accionPendiente = accion;

                vistaHud.MostrarSelectorEntrenamiento(true);

                vistaHud.MostrarMensaje(
                    "Selecciona el tipo de unidad a entrenar.");

                return;
            }

            if (accion == "Atacar")
            {
                if (string.IsNullOrWhiteSpace(entidad.IdLogico))
                {
                    vistaHud.MostrarMensaje(
                        "La unidad atacante no tiene identidad disponible.",
                        true);

                    return;
                }

                if (!PuedeIniciarAccion(accion))
                {
                    vistaHud.MostrarMensaje(
                        "La conexión con la API no está disponible.",
                        true);

                    return;
                }

                unidadPendiente = entidad;
                unidadIdPendiente = entidad.IdLogico;
                accionPendiente = accion;

                controladorSeleccion.IniciarCapturaObjetivoEntidad();

                vistaHud.MostrarMensaje(
                    "Selecciona una unidad enemiga como objetivo.");

                return;
            }

            if (accion == "Mover" ||
                accion == "Recolectar" ||
                accion == "Construir")
            {
                if (string.IsNullOrWhiteSpace(entidad.IdLogico))
                {
                    vistaHud.MostrarMensaje(
                        "La unidad seleccionada no tiene identidad disponible.",
                        true);

                    return;
                }

                if (!PuedeIniciarAccion(accion))
                {
                    vistaHud.MostrarMensaje(
                        "La conexión con la API no está disponible.",
                        true);

                    return;
                }

                unidadPendiente = entidad;
                unidadIdPendiente = entidad.IdLogico;
                accionPendiente = accion;

                if (accion == "Recolectar")
                {
                    controladorSeleccion.IniciarCapturaRecurso();
                }
                else
                {
                    controladorSeleccion.IniciarCapturaDestino();
                }

                if (accion == "Mover")
                {
                    vistaHud.MostrarMensaje(
                        "Selecciona una casilla destino.");
                }
                else if (accion == "Recolectar")
                {
                    vistaHud.MostrarMensaje(
                        "Selecciona un recurso.");
                }
                else
                {
                    string costo =
                        conexionApi == null
                            ? string.Empty
                            : conexionApi.DescribirCostoConstruccion();

                    string mensajeConstruccion =
                        "Selecciona una casilla para construir el Centro Urbano.";

                    if (!string.IsNullOrWhiteSpace(costo))
                    {
                        mensajeConstruccion +=
                            " " + costo;
                    }

                    vistaHud.MostrarMensaje(
                        mensajeConstruccion);
                }

                return;
            }

            vistaHud.MostrarMensaje(
                $"Intención {accion} preparada. Ejecución pendiente de una fase posterior.");
        }

        private void SeleccionarTipoUnidad(string tipoUnidad)
        {
            if (accionPendiente != "Entrenar" ||
                edificioPendiente == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(tipoUnidad))
                return;

            tipoUnidadPendiente = tipoUnidad;

            vistaHud.MostrarSelectorEntrenamiento(false);

            controladorSeleccion.IniciarCapturaDestino();

            string costo =
                conexionApi == null
                    ? string.Empty
                    : conexionApi.DescribirCostoUnidad(
                        tipoUnidad);

            string mensajeEntrenamiento =
                $"Selecciona una casilla de referencia para {tipoUnidad}.";

            if (!string.IsNullOrWhiteSpace(costo))
            {
                mensajeEntrenamiento +=
                    " " + costo;
            }

            vistaHud.MostrarMensaje(
                mensajeEntrenamiento);
        }

        private static string ObtenerMensajeCancelacion(string accion)
        {
            if (accion == "Recolectar")
                return "Recolección cancelada.";

            if (accion == "Construir")
                return "Construcción cancelada.";

            if (accion == "Entrenar")
                return "Entrenamiento cancelado.";

            if (accion == "Atacar")
                return "Ataque cancelado.";

            return "Movimiento cancelado.";
        }
    }
}