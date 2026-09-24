using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>Confirmación de un proceso concurrente recién iniciado.</summary>
    [Serializable]
    public class ProcesoIniciadoDto
    {
        /// <summary>Identificador del proceso asignado por la API.</summary>
        public string procesoId;
        /// <summary>Nombre de la acción (MOVER, RECOLECTAR, etc.).</summary>
        public string nombre;
        /// <summary>Estado inicial del proceso.</summary>
        public string estado;
    }

    /// <summary>Resultado final de un proceso ejecutado por un worker.</summary>
    [Serializable]
    public class ResultadoProcesoDto
    {
        /// <summary>Identificador del proceso consultado.</summary>
        public string procesoId;
        /// <summary>Nombre de la acción ejecutada.</summary>
        public string nombre;
        /// <summary>Estado final (Completado, Fallido, Cancelado).</summary>
        public string estado;
        /// <summary>Identificador del hilo de trabajo que lo ejecutó.</summary>
        public int hiloTrabajoId;
        /// <summary>Indica si la regla del Modelo aceptó la acción.</summary>
        public bool exito;
        /// <summary>Mensaje del Modelo para mostrar en el HUD.</summary>
        public string mensaje;
        /// <summary>Detalle técnico cuando el worker falla.</summary>
        public string errorTecnico;
    }
}
