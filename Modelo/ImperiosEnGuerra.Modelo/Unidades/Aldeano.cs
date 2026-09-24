using System;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa la especialización Aldeano con carga transportada separada
    /// del saldo económico del jugador.
    /// </summary>
    public class Aldeano : Unidad
    {
        /// <summary>
        /// Representa el campo capacidad carga predeterminada.
        /// </summary>
        public const int CapacidadCargaPredeterminada = 10;

        private readonly object sincronizacionCarga =
            new object();

        private int cargaActual;
        private TipoRecurso? tipoCarga;

        /// <summary>
        /// Obtiene capacidad carga.
        /// </summary>
        public int CapacidadCarga { get; }

        /// <summary>
        /// Obtiene carga actual.
        /// </summary>
        public int CargaActual
        {
            get
            {
                lock (sincronizacionCarga)
                {
                    return cargaActual;
                }
            }
        }

        /// <summary>
        /// Obtiene tipo carga.
        /// </summary>
        public TipoRecurso? TipoCarga
        {
            get
            {
                lock (sincronizacionCarga)
                {
                    return tipoCarga;
                }
            }
        }

        /// <summary>
        /// Obtiene capacidad disponible.
        /// </summary>
        public int CapacidadDisponible
        {
            get
            {
                lock (sincronizacionCarga)
                {
                    return CapacidadCarga - cargaActual;
                }
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia de Aldeano.
        /// </summary>
        /// <param name="coordenada">El valor de coordenada.</param>
        public Aldeano(
            Coordenada coordenada)
            : this(
                coordenada,
                CapacidadCargaPredeterminada)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de Aldeano.
        /// </summary>
        /// <param name="coordenada">El valor de coordenada.</param>
        /// <param name="capacidadCarga">El valor de capacidad carga.</param>
        public Aldeano(
            Coordenada coordenada,
            int capacidadCarga)
            : base(
                coordenada,
                1.00d,
                50,
                0,
                1)
        {
            if (capacidadCarga <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capacidadCarga),
                    "La capacidad de carga debe ser positiva.");
            }

            CapacidadCarga = capacidadCarga;
            cargaActual = 0;
            tipoCarga = null;
        }

        /// <summary>
        /// Extrae del nodo y añade a la carga del Aldeano de forma sincronizada.
        /// No mezcla tipos de recurso en una misma carga.
        /// </summary>
        public int RecolectarDesde(
            Recurso recurso,
            int cantidadSolicitada)
        {
            if (recurso == null)
            {
                throw new ArgumentNullException(
                    nameof(recurso));
            }

            if (cantidadSolicitada < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadSolicitada),
                    "La cantidad solicitada no puede ser negativa.");
            }

            if (cantidadSolicitada == 0)
            {
                return 0;
            }

            lock (sincronizacionCarga)
            {
                if (cargaActual > 0 &&
                    tipoCarga.HasValue &&
                    tipoCarga.Value != recurso.Tipo)
                {
                    return 0;
                }

                int capacidadDisponible =
                    CapacidadCarga - cargaActual;

                if (capacidadDisponible <= 0)
                {
                    return 0;
                }

                int cantidadAExtraer =
                    Math.Min(
                        cantidadSolicitada,
                        capacidadDisponible);

                int extraida =
                    recurso.Extraer(
                        cantidadAExtraer);

                if (extraida > 0)
                {
                    tipoCarga = recurso.Tipo;
                    cargaActual += extraida;
                }

                return extraida;
            }
        }

        /// <summary>
        /// Vacía la carga para que una operación de depósito pueda transferirla
        /// al saldo del jugador.
        /// </summary>
        public int VaciarCarga(
            out TipoRecurso? tipo)
        {
            lock (sincronizacionCarga)
            {
                tipo = tipoCarga;

                int cantidad =
                    cargaActual;

                cargaActual = 0;
                tipoCarga = null;

                return cantidad;
            }
        }
    }
}
