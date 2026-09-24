using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Servicios;

/// <summary>
/// Representa servicio ordenes unidad dentro del modelo del juego.
/// </summary>
public sealed class ServicioOrdenesUnidad
{
    /// <summary>
    /// Inicia el elemento solicitado.
    /// </summary>
    /// <param name="unidad">El valor de unidad.</param>
    /// <param name="tipo">El valor de tipo.</param>
    /// <returns>true si la operación tuvo éxito; false en caso contrario.</returns>
    public bool Iniciar(
        Unidad unidad,
        TipoAccionJuego tipo)
        {
            if(unidad == null)
            return false;
            
            return unidad.IntentarIniciarOrden(tipo);
        }

        /// <summary>
        /// Completa el elemento solicitado.
        /// </summary>
        /// <param name="unidad">El valor de unidad.</param>
        public void Completar(
            Unidad unidad)
    {
        if (unidad == null)
        return;

        unidad.CompletarOrden();
    }

    /// <summary>
    /// Cancela el elemento solicitado.
    /// </summary>
    /// <param name="unidad">El valor de unidad.</param>
    public void Cancelar(
        Unidad unidad)
    {
        if(unidad == null)
        return;

        unidad.CancelarOrden();
    }
    }
