using System.Linq;
using ModeloContratos = ImperiosEnGuerra.Modelo.Contratos;
using UnityDto = ImperiosEnGuerra.Controladores.Red.Contratos;

namespace ImperiosEnGuerra.Controladores.ApiInterna
{
    /// <summary>
    /// Traduce la respuesta del Modelo a DTOs de Unity (campos públicos).
    /// Solo mapeo, sin reglas ni hilos.
    /// </summary>
    public static class AdaptadorEstadoPartida
    {
        public static UnityDto.EstadoPartidaDto Convertir(ModeloContratos.EstadoPartidaResponse respuesta)
        {
            return new UnityDto.EstadoPartidaDto
            {
                estado = respuesta.Estado,
                mapa = ConvertirMapa(respuesta.Mapa),
                jugadorHumano = ConvertirJugador(respuesta.JugadorHumano),
                jugadorMaquina = ConvertirJugador(respuesta.JugadorMaquina),
                economia = ConvertirEconomia(respuesta.Economia)
            };
        }

        private static UnityDto.MapaEstadoDto ConvertirMapa(ModeloContratos.MapaEstadoResponse mapa)
        {
            return new UnityDto.MapaEstadoDto
            {
                ancho = mapa.Ancho,
                alto = mapa.Alto,
                recursos = mapa.Recursos.Select(r => new UnityDto.RecursoEstadoDto
                {
                    tipo = r.Tipo,
                    coordenada = new UnityDto.CoordenadaEstadoDto { x = r.Coordenada.X, y = r.Coordenada.Y },
                    cantidadRestante = r.CantidadRestante
                }).ToArray()
            };
        }

        private static UnityDto.JugadorEstadoDto ConvertirJugador(ModeloContratos.JugadorEstadoResponse jugador)
        {
            return new UnityDto.JugadorEstadoDto
            {
                nombre = jugador.Nombre,
                tipo = jugador.Tipo,
                recursos = new UnityDto.RecursosJugadorEstadoDto
                {
                    oro = jugador.Recursos.Oro,
                    madera = jugador.Recursos.Madera,
                    comida = jugador.Recursos.Comida
                },
                edificios = jugador.Edificios.Select(e => new UnityDto.EdificioEstadoDto
                {
                    id = e.Id,
                    tipo = e.Tipo,
                    coordenada = new UnityDto.CoordenadaEstadoDto { x = e.Coordenada.X, y = e.Coordenada.Y },
                    colaEntrenamiento = e.ColaEntrenamiento.Select(p => new UnityDto.EntrenamientoEstadoDto
                    {
                        id = p.Id,
                        tipoUnidad = p.TipoUnidad,
                        progreso = p.Progreso
                    }).ToArray()
                }).ToArray(),
                obrasConstruccion = jugador.ObrasConstruccion.Select(o => new UnityDto.ObraConstruccionEstadoDto
                {
                    id = o.Id,
                    tipo = o.Tipo,
                    coordenada = new UnityDto.CoordenadaEstadoDto { x = o.Coordenada.X, y = o.Coordenada.Y },
                    progreso = o.Progreso
                }).ToArray(),
                unidades = jugador.Unidades.Select(u => new UnityDto.UnidadEstadoDto
                {
                    id = u.Id,
                    tipo = u.Tipo,
                    coordenada = u.Coordenada == null
                        ? null
                        : new UnityDto.CoordenadaEstadoDto { x = u.Coordenada.X, y = u.Coordenada.Y },
                    disponible = u.Disponible,
                    estado = u.Estado,
                    ordenActiva = u.OrdenActiva,
                    capacidadCarga = u.CapacidadCarga,
                    cargaActual = u.CargaActual,
                    tipoCarga = u.TipoCarga
                }).ToArray()
            };
        }

        private static UnityDto.EconomiaEstadoDto ConvertirEconomia(ModeloContratos.EconomiaEstadoResponse economia)
        {
            return new UnityDto.EconomiaEstadoDto
            {
                centroUrbano = ConvertirCosto(economia.CentroUrbano),
                aldeano = ConvertirCosto(economia.Aldeano),
                guerrero = ConvertirCosto(economia.Guerrero),
                lancero = ConvertirCosto(economia.Lancero),
                arquero = ConvertirCosto(economia.Arquero),
                monje = ConvertirCosto(economia.Monje)
            };
        }

        private static UnityDto.CostoEstadoDto ConvertirCosto(ModeloContratos.CostoEstadoResponse costo)
        {
            return new UnityDto.CostoEstadoDto
            {
                oro = costo.Oro,
                madera = costo.Madera,
                comida = costo.Comida
            };
        }
    }
}
