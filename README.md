# Imperios en Guerra

Videojuego de estrategia en tiempo real (RTS) en C# y Unity, inspirado en Age of Empires. Proyecto académico de Programación Avanzada (docente Nancy Yaneth Gelvez García).

---

## Qué es el juego (en palabras simples)

Tú comandas a los **Griegos** (castillo al este) contra **Troya**, la máquina (castillo al oeste), en un mapa de **15×15**.

1. Tus **aldeanos** recogen oro, madera, comida, piedra y hierro de los nodos del mapa.
2. Con esos recursos **invocas** más aldeanos, **soldados** (fuertes de cerca) y **arqueros** (atacan de lejos) desde tu castillo.
3. Mueves tus tropas con clics, atacas al enemigo y cuidas tu castillo.
4. **Ganas** si destruyes el castillo enemigo **y** todas sus unidades. **Pierdes** si te quedas sin unidades.
5. Puedes **guardar** con F5 y **cargar** con F9. Cada partida deja archivos: configuración inicial, bitácora de acciones y resultado final (ver `docs/INFORME_PROYECTO.md` §4).

La máquina no es tonta: sus tropas cazan a las tuyas, cuidan su castillo, huyen tus aldeanos del peligro y sus heridos se retiran. Además ves **barras de vida** sobre cada unidad y edificio.

---

## Cómo está organizado (MVC en palabras simples)

- **Modelo** (`Modelo/`): el cerebro. Sabe las reglas, mueve las piezas y usa hilos para que varias cosas pasen a la vez (recolectar, construir, entrenar, atacar, la IA). No sabe nada de pantallas.
- **Vista** (`Assets/Scripts/Vistas/` + escenas y dibujos): los ojos. Solo muestra el mapa, los botones y los mensajes. No decide nada.
- **Controlador** (`Assets/Scripts/Controladores/` y `Controlador/`): las manos. Recibe tus clics, pregunta al Modelo si se puede hacer, lanza el trabajo y devuelve el resultado a la pantalla. No crea hilos.

```text
Modelo/        Lógica del juego y concurrencia (Thread/Task/lock solo aquí)
  ImperiosEnGuerra.Modelo/
Assets/        Proyecto Unity (la raíz del repo): escenas, sprites y scripts
  Scripts/Controladores/  puente (corutinas, sin Thread/Task)
  Scripts/Vistas/         solo lectura del estado
Controlador/   Servidor REST externo opcional (sin hilos propios)
  ImperiosEnGuerra.Controlador/
tests/         Pruebas .NET (NUnit, 313 verdes)
docs/          Documentación (informe, diagramas, pruebas de escritorio, clases)
scripts/       Instalador de sprites Tiny Swords
```

---

## Abrir y jugar

1. Instala los sprites de Tiny Swords (no se versionan): ver `docs/INSTALACION_TINY_SWORDS.md` o ejecuta `powershell -ExecutionPolicy Bypass -File scripts\instalar_tinyswords.ps1 -Origen "<ruta del pack>"`.
2. Abre en **Unity Hub la raíz del repositorio** (donde están `Assets/`, `Packages/` y `ProjectSettings/`). Usa **Unity 6000.6.0f1**.
3. Abre `Assets/Scenes/SampleScene.unity` y dale **Play**. Con una sola ventana de Unity abierta.
4. Controles: clic para seleccionar, botones Mover / Recolectar / Construir / Entrenar / Atacar / ¡Batalla!; F5 guarda, F9 carga.
5. (Opcional) Servidor REST externo: `dotnet run --project Controlador/ImperiosEnGuerra.Controlador` y activa `usarApiExterna` en el inspector. Por defecto el juego usa la API interna en memoria.

---

## Qué cambió (historial corto)

- **Base MVC + combate real + IA enemiga + guardado TXT + HUD** (mapa 15×15, Tiny Swords, victoria total).
- **Concurrencia real**: un worker por orden (movimiento, recolección, construcción, entrenamiento, ataque, batalla) + worker continuo de IA; `lock`, `ConcurrentDictionary/Queue`, cancelación y polling 0,1 s sin congelar Unity.
- **Nodo agotado libera su casilla**; la unidad nace en la casilla clicada; la IA prioriza tropas y defiende su castillo; la derrota se anuncia una vez en el HUD.
- **Modelo de combate del compañero**: aldeanos que huyen del peligro, máquinas heridas que se retiran, **barras de vida**, proyecto Unity movido a la raíz yTests nuevos.
- **Documentación formal**: `docs/INFORME_PROYECTO.md` (concurrencia, MVC, red, archivos), `docs/DIAGRAMAS.md` (arquitectura, clases y flujos), `docs/PRUEBA_ESCRITORIO.md` (trazas de recolección, ataque, red y victoria), `docs/CLASES.md` (todas las clases explicadas) y `docs/CUMPLIMIENTO_GUIA.md` (guía vs implementación).

**Nota honesta sobre red:** el rival es la máquina, no otro humano. La red implementada es REST + JSON entre Unity y el Modelo (modo interno en memoria por defecto, servidor externo alterno en `localhost:5086`).

---

## Tecnologías

C#, Unity (UGUI), .NET Tasks/ThreadPool, `lock`, colecciones concurrentes, REST + JSON (`UnityWebRequest`), `System.IO`, NUnit.

## Ramas

`main` (estable) y `develop` (integración). Flujo: Issue → Branch → Desarrollo → Pruebas → Commit → Pull Request → Develop → Main.

## Estado

Concurrencia, sincronización y cancelación integradas y probadas: mapa y recursos, selección por clic y HUD, movimiento, recolección con depósito, construcción y entrenamiento con costos, ataque con validaciones, batalla total, IA completa, guardado/carga, bitácora y resultado final, 313 pruebas verdes y Unity sin errores.

## Referencias

- Microsoft. (s/f). *Managed Threading (C#)*. Learn.microsoft.com. Recuperado el 1 de septiembre de 2026, de https://learn.microsoft.com/dotnet/standard/threading/managed-threading-basics
- Microsoft. (s/f). *Sockets (System.Net.Sockets)*. Learn.microsoft.com. Recuperado el 1 de septiembre de 2026, de https://learn.microsoft.com/dotnet/api/system.net.sockets
- Unity Technologies. (s/f). *Unity Manual: Multithreading and native plug-ins*. Docs.unity3d.com. Recuperado el 1 de septiembre de 2026, de https://docs.unity3d.com/Manual/UnderstandingPerformanceThreading.html
