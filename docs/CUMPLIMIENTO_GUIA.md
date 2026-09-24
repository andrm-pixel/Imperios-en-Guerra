# Cumplimiento de la guia - Imperios en Guerra

Modalidad: Humano vs Maquina (sin multijugador, por decision del curso).

## Estructura del programa

| Punto guia | Estado | Evidencia |
|---|---|---|
| Mapa 15x15 por jugador y render Unity | OK | `Modelo/Core/DisposicionInicial.cs` (fusionado en `InicializadorPartida`), `VistaPartida.Renderizar`, test `MapaInicialTests` |
| Recursos oro/madera/comida (+piedra/hierro) y castillo inicial | OK | `InicializadorPartida.Crear`, validacion anti-traslape |
| Guardar configuracion inicial | OK | `configuracion.txt` via `ServicioArchivos` |
| Recoleccion y construccion con hilos sin bloquear UI | OK | workers `MotorAcciones` (Task), UI pregunta cada 0.1 s |
| Reflejo en tiempo real + sincronizacion al hilo principal | OK | `ControladorConexionApi.EsperarProcesoInterno`, cola `TareasJuego` |
| Sincronizacion anti carreras | OK | `lock` en `EstadoPartidaService`, `RecursosJugador`, `Castillo`, `Aldeano`, `Recurso`, `ConcurrentDictionary/Queue` |
| Red entre jugadores | PARCIAL | REST con JSON entre Unity y servicios (`ControladorConexionApi`, `Program.cs`); el rival es la IA, no otro humano |
| Acciones construir/entrenar/mover/atacar desde UI | OK | botones + clics con raycast, `ControladorAcciones` |
| Validaciones (recursos, limites, disponibilidad) | OK | `ReglasAcciones`, planificadores, `Operacion*` + tests |
| Verificacion de ganador | OK | `OperacionAtaque.EsVictoriaHumana/Maquina` (castillo Y ejercito), anuncio HUD, `resultado_final.txt` |

## Detalle por requisito tecnico

- C# + Unity 6000 desktop UGUI: OK.
- MVC en carpetas y namespaces (`Modelo/`, `Vista/`, `Controlador/`): OK.
- Hilos Task + sincronizacion: OK (tabla de hilos en el informe del chat).
- Red REST: OK (modo externo) + modo interno en memoria.
- Archivos: `configuracion.txt`, `log_partida.txt` (formato Turno/Accion/Resultado), `resultado_final.txt`, `progreso.txt` (F5/F9): OK.
- Colecciones `List/Dictionary/Concurrent`: OK.
- Validacion de entradas y try-catch en endpoints, IO y workers: OK.

## Brechas conocidas

1. Sin multijugador humano-humano (fuera de alcance: es contra la maquina).
2. Sin diagrama UML en repo (generar desde `Modelo/` con herramienta externa).
3. Red externa probada solo en modo interno por defecto.
