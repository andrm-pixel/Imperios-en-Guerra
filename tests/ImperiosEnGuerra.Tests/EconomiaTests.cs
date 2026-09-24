using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Tests;

/// <summary>Pruebas de Economia: verifica economia.</summary>
public class EconomiaTests
{
    // Caso Gasto Compuesto: verifica es atomico.
    [Test]
    public void GastoCompuesto_EsAtomico()
    {
        var recursos = new RecursosJugador();
        recursos.Agregar(TipoRecurso.Oro, 20);
        recursos.Agregar(TipoRecurso.Madera, 30);
        recursos.Agregar(TipoRecurso.Comida, 10);

        var costo =
            new CostoRecursos(20, 30, 10);

        Assert.That(
            recursos.IntentarGastar(costo),
            Is.True);

        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.Zero);
        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Madera),
            Is.Zero);
        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Comida),
            Is.Zero);
    }

    // Caso Gasto Compuesto: verifica insuficiente - no descuenta nada.
    [Test]
    public void GastoCompuesto_Insuficiente_NoDescuentaNada()
    {
        var recursos = new RecursosJugador();
        recursos.Agregar(TipoRecurso.Oro, 20);
        recursos.Agregar(TipoRecurso.Madera, 29);
        recursos.Agregar(TipoRecurso.Comida, 10);

        Assert.That(
            recursos.IntentarGastar(
                new CostoRecursos(20, 30, 10)),
            Is.False);

        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.EqualTo(20));
        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Madera),
            Is.EqualTo(29));
        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Comida),
            Is.EqualTo(10));
    }

    // Caso Dos Gastos Concurrentes: verifica con saldo para uno - solo uno gana.
    [Test]
    public async Task DosGastosConcurrentes_ConSaldoParaUno_SoloUnoGana()
    {
        var recursos = new RecursosJugador();
        recursos.Agregar(TipoRecurso.Oro, 20);
        recursos.Agregar(TipoRecurso.Madera, 30);

        var costo =
            new CostoRecursos(20, 30, 0);

        bool[] resultados =
            await Task.WhenAll(
                Task.Run(() => recursos.IntentarGastar(costo)),
                Task.Run(() => recursos.IntentarGastar(costo)));

        Assert.That(
            resultados.Count(x => x),
            Is.EqualTo(1));

        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Oro),
            Is.Zero);

        Assert.That(
            recursos.ObtenerCantidad(TipoRecurso.Madera),
            Is.Zero);
    }

    // Caso Configuracion Economia: verifica usa madera solo para construccion.
    [Test]
    public void ConfiguracionEconomia_UsaMaderaSoloParaConstruccion()
    {
        var config =
            new ConfiguracionEconomia();

        Assert.That(
            config.IntentarObtenerCostoEdificio(
                "CentroUrbano",
                out CostoRecursos edificio),
            Is.True);

        Assert.That(edificio.Oro, Is.EqualTo(20));
        Assert.That(edificio.Madera, Is.EqualTo(50));
        Assert.That(edificio.Comida, Is.Zero);
        Assert.That(edificio.Piedra, Is.EqualTo(20));
        Assert.That(edificio.Hierro, Is.Zero);

        var esperados = new Dictionary<string, (int Oro, int Comida, int Piedra, int Hierro)>
        {
            { "Aldeano", (0, 10, 0, 0) },
            { "Soldado", (5, 15, 0, 5) },
            { "Arquero", (10, 10, 0, 8) }
        };

        foreach (var esperado in esperados)
        {
            Assert.That(
                config.IntentarObtenerCostoUnidad(
                    esperado.Key,
                    out CostoRecursos costo),
                Is.True);

            Assert.That(costo.Madera, Is.Zero, esperado.Key);
            Assert.That(costo.Oro, Is.EqualTo(esperado.Value.Oro), esperado.Key);
            Assert.That(costo.Comida, Is.EqualTo(esperado.Value.Comida), esperado.Key);
            Assert.That(costo.Piedra, Is.EqualTo(esperado.Value.Piedra), esperado.Key);
            Assert.That(costo.Hierro, Is.EqualTo(esperado.Value.Hierro), esperado.Key);
        }
    }
}
