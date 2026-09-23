using System;

namespace ImperiosEnGuerra.Servicios
{
    /// <summary>
    /// Compatibilidad: la persistencia vive en el Modelo.
    /// Usar <see cref="Modelo.Persistencia.ServicioArchivos"/>.
    /// </summary>
    [Obsolete("Usar ImperiosEnGuerra.Modelo.Persistencia.ServicioArchivos. La persistencia es parte del Modelo.")]
    public class ServicioArchivos : Modelo.Persistencia.ServicioArchivos
    {
        public ServicioArchivos(string directorioBase)
            : base(directorioBase)
        {
        }
    }
}
