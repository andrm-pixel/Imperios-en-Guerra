using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Aplica dano real de combate en el Modelo con estadisticas por tipo.
    /// El objetivo puede ser una unidad enemiga o un edificio enemigo.
    /// Unidades del prototipo: Aldeano, Soldado y Arquero.
    /// Valores del prototipo: Soldado 25/alc.1,
    /// Arquero 15/alc.4. Lo destruido se retira y libera
    /// su casilla. Victoria: sin Castillo o sin unidades enemigas.
    /// </summary>
    public sealed class OperacionAtaque
    {
        /// <summary>
        /// Ejecuta el elemento solicitado.
        /// </summary>
        /// <param name="partida">El valor de partida.</param>
        /// <param name="solicitud">El valor de solicitud.</param>
        /// <returns>Resultado de la operacion.</returns>
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudAtaque solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");

            if (solicitud == null)
                return ResultadoAccion.Fallido("La solicitud de ataque es obligatoria.");

            var atacanteMaquina = partida.JugadorMaquina.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.AtacanteId);

            if (atacanteMaquina != null)
                return ResultadoAccion.Fallido(
                    "La unidad atacante pertenece a la máquina y no puede controlarse.");

            var atacante = partida.JugadorHumano.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.AtacanteId);

            if (atacante == null)
                return ResultadoAccion.Fallido(
                    "La unidad atacante no existe o fue destruida.");

            if (!atacante.Disponible &&
                atacante.OrdenActiva != TipoAccionJuego.Atacar)
                return ResultadoAccion.Fallido(
                    "La unidad atacante no está disponible.");

            if (!EsUnidadMilitar(atacante))
                return ResultadoAccion.Fallido(
                    "La unidad atacante no es una unidad militar permitida.");

            if (atacante.Coordenada == null)
                return ResultadoAccion.Fallido(
                    "La unidad atacante no tiene posición válida.");

            var objetivoPropioUnidad = partida.JugadorHumano.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.ObjetivoId);

            var objetivoPropioEdificio = partida.JugadorHumano.Edificios
                .FirstOrDefault(edificio => edificio.Id == solicitud.ObjetivoId);

            if (objetivoPropioUnidad != null ||
                objetivoPropioEdificio != null)
                return ResultadoAccion.Fallido(
                    "El objetivo pertenece al jugador humano.");

            var objetivoUnidad = partida.JugadorMaquina.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.ObjetivoId);

            var objetivoEdificio = objetivoUnidad == null
                ? partida.JugadorMaquina.Edificios
                    .FirstOrDefault(edificio => edificio.Id == solicitud.ObjetivoId)
                : null;

            if (objetivoUnidad == null &&
                objetivoEdificio == null)
                return ResultadoAccion.Fallido(
                    "No existe el objetivo enemigo indicado.");

            Coordenada posicionObjetivo =
                objetivoUnidad != null
                    ? objetivoUnidad.Coordenada
                    : objetivoEdificio.Coordenada;

            string nombreObjetivo =
                objetivoUnidad != null
                    ? objetivoUnidad.GetType().Name
                    : objetivoEdificio.GetType().Name;

            if (posicionObjetivo == null)
                return ResultadoAccion.Fallido(
                    "El objetivo no tiene posición válida.");

            int distancia =
                Math.Abs(atacante.Coordenada.X - posicionObjetivo.X) +
                Math.Abs(atacante.Coordenada.Y - posicionObjetivo.Y);

            if (distancia > atacante.AlcanceAtaque)
                return ResultadoAccion.Fallido(
                    $"Objetivo fuera de alcance ({distancia} > {atacante.AlcanceAtaque}). Mueve la unidad para acercarla.");

            bool destruido =
                objetivoUnidad != null
                    ? objetivoUnidad.RecibirDano(atacante.PuntosAtaque)
                    : objetivoEdificio.RecibirDano(atacante.PuntosAtaque);

            int vidaRestante =
                objetivoUnidad != null
                    ? objetivoUnidad.Vida
                    : objetivoEdificio.Vida;

            if (!destruido)
            {
                return ResultadoAccion.Exitoso(
                    $"Impacto: {atacante.PuntosAtaque} de daño a {nombreObjetivo} (vida {vidaRestante}).");
            }

            if (objetivoUnidad != null)
            {
                partida.JugadorMaquina.EliminarUnidad(objetivoUnidad);
            }
            else
            {
                partida.JugadorMaquina.EliminarEdificio(objetivoEdificio);
            }

            partida.JugadorMaquina.Mapa.ObtenerCasilla(
                posicionObjetivo.X,
                posicionObjetivo.Y)?.Liberar();

            if (EsVictoriaHumana(partida))
            {
                return ResultadoAccion.Exitoso(
                    $"{nombreObjetivo} enemigo destruido. ¡Victoria! La máquina perdió su Castillo y todas sus unidades.");
            }

            int restantesMaquina =
                partida.JugadorMaquina.Unidades.Count +
                partida.JugadorMaquina.Edificios.Count;

            return ResultadoAccion.Exitoso(
                $"{nombreObjetivo} enemigo destruido. Quedan {restantesMaquina} enemigos.");
        }

        /// <summary>
        /// Verifica la victoria humana: Castillo Y todas las unidades enemigas destruidas.
        /// </summary>
        /// <param name="partida">Partida que se evalua.</param>
        /// <returns>true si la maquina perdio su centro y sus unidades.</returns>
        public static bool EsVictoriaHumana(Partida partida)
        {
            if (partida == null)
                return false;

            bool sinCentro = !partida.JugadorMaquina.Edificios
                .OfType<Castillo>()
                .Any();

            bool sinUnidades =
                partida.JugadorMaquina.Unidades.Count == 0;

            return sinCentro && sinUnidades;
        }

        /// <summary>
        /// Ejecuta la operacion es victoria maquina.
        /// </summary>
        /// <param name="partida">El valor de partida.</param>
        /// <returns>true si la operacion tuvo exito; false en caso contrario.</returns>
        public static bool EsVictoriaMaquina(Partida partida)
        {
            if (partida == null)
                return false;

            // El humano puede reconstruir su centro con un aldeano si
            // sobrevive alguien; la maquina solo gana arrasando todo.
            return partida.JugadorHumano.Unidades.Count == 0;
        }

        private static bool EsUnidadMilitar(Unidad unidad)
        {
            return unidad is UnidadMilitar;
        }
    }
}
