using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Aplica daño real de combate en el Modelo con estadísticas por tipo.
    /// Valores del prototipo: Guerrero 25/alc.1, Lancero 20/alc.2,
    /// Arquero 15/alc.4, Monje 10/alc.1. La unidad destruida se retira
    /// y libera su casilla.
    /// </summary>
    public sealed class OperacionAtaque
    {
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
                    "No existe la unidad atacante humana indicada.");

            if (!atacante.Disponible)
                return ResultadoAccion.Fallido(
                    "La unidad atacante no está disponible.");

            if (!EsUnidadMilitar(atacante))
                return ResultadoAccion.Fallido(
                    "La unidad atacante no es una unidad militar permitida.");

            var objetivoPropio = partida.JugadorHumano.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.ObjetivoId);

            if (objetivoPropio != null)
                return ResultadoAccion.Fallido(
                    "El objetivo pertenece al jugador humano.");

            var objetivo = partida.JugadorMaquina.Unidades
                .FirstOrDefault(unidad => unidad.Id == solicitud.ObjetivoId);

            if (objetivo == null)
                return ResultadoAccion.Fallido(
                    "No existe la unidad enemiga objetivo indicada.");

            if (atacante.Coordenada == null ||
                objetivo.Coordenada == null)
                return ResultadoAccion.Fallido(
                    "Atacante u objetivo sin posición válida.");

            int distancia =
                Math.Abs(atacante.Coordenada.X - objetivo.Coordenada.X) +
                Math.Abs(atacante.Coordenada.Y - objetivo.Coordenada.Y);

            if (distancia > atacante.AlcanceAtaque)
                return ResultadoAccion.Fallido(
                    $"Objetivo fuera de alcance ({distancia} > {atacante.AlcanceAtaque}). Mueve la unidad para acercarla.");

            bool destruido = objetivo.RecibirDano(atacante.PuntosAtaque);

            if (!destruido)
            {
                return ResultadoAccion.Exitoso(
                    $"Impacto: {atacante.PuntosAtaque} de daño a {objetivo.GetType().Name} (vida {objetivo.Vida}).");
            }

            partida.JugadorMaquina.EliminarUnidad(objetivo);

            Mapa mapaMaquina = partida.JugadorMaquina.Mapa;
            mapaMaquina.ObtenerCasilla(
                objetivo.Coordenada.X,
                objetivo.Coordenada.Y)?.Liberar();

            if (partida.JugadorMaquina.Unidades.Count == 0)
            {
                return ResultadoAccion.Exitoso(
                    $"Unidad enemiga destruida. ¡Victoria! Todas las unidades de la máquina fueron eliminadas.");
            }

            return ResultadoAccion.Exitoso(
                "Unidad enemiga destruida.");
        }

        private static bool EsUnidadMilitar(Unidad unidad)
        {
            return unidad is Soldado || unidad is Monje;
        }
    }
}
