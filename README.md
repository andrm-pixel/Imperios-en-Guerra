# Imperios en Guerra

Videojuego de estrategia en tiempo real (RTS) en C# y Unity, inspirado en Age of Empires. Proyecto académico de Programación Avanzada (docente Nancy Yaneth Gelvez García).

---

## Qué es el juego

Tú comandas a los **Griegos** (castillo a la derecha) contra **Troya**, la máquina (castillo a la izquierda), en un mapa de **15×15**.

1. Tus **aldeanos** recogen oro, madera, comida, piedra y hierro de los nodos del mapa.
2. Con esos recursos **invocas** más aldeanos, **soldados** (fuertes de cerca) y **arqueros** (atacan de lejos) desde tu castillo.
3. Mueves tus tropas con clics, atacas al enemigo y cuidas tu castillo.
4. **Ganas** si destruyes el castillo enemigo **y** todas sus unidades. **Pierdes** si te quedas sin unidades.
5. Puedes **guardar** con F5 y **cargar** con F9. Cada partida deja archivos: configuración inicial, bitácora de acciones y resultado final (ver `docs/INFORME_PROYECTO.md` §4).

La máquina no es tonta: sus tropas cazan a las tuyas, cuidan su castillo, huyen tus aldeanos del peligro y sus heridos se retiran. Además ves **barras de vida** sobre cada unidad y edificio.

---

## Cómo está organizado (MVC)

- **Modelo** (`Modelo/`): el cerebro. Sabe las reglas, mueve las piezas y usa hilos para que varias cosas pasen a la vez (recolectar, construir, entrenar, atacar, la IA). No sabe nada de pantallas.
- **Vista** (`Assets/Scripts/Vistas/` + escenas y dibujos): los ojos. Solo muestra el mapa, los botones y los mensajes. No decide nada.
- **Controlador** (`Assets/Scripts/Controladores/` y `Controlador/`): las manos. Recibe tus clics, pregunta al Modelo si se puede hacer, lanza el trabajo y devuelve el resultado a la pantalla. No crea hilos.

```text
Modelo/        Lógica del juego y concurrencia (Thread/Task/lock solo aquí)
  ImperiosEnGuerra.Modelo/
Assets/        Proyecto Unity (la raíz del repo): escenas, sprites y scripts
  Scripts/Controladores/  puente (corutinas, sin Thread/Task)
  Scripts/Vistas/         solo lectura del estado
Controlador/   (sin hilos propios)
  ImperiosEnGuerra.Controlador/
tests/         Pruebas .NET (NUnit, 313 verdes)
docs/          Documentación (informe, diagramas, pruebas de escritorio, clases)
scripts/       Instalador de sprites Tiny Swords
```

---

## Abrir y jugar

1. Instala los sprites con `scripts\instalar_tinyswords.ps1` (ver `docs/INSTALACION_TINY_SWORDS.md`).
2. Abre la raíz del repo en Unity Hub (Unity 6000.6.0f1), abre `Assets/Scenes/SampleScene.unity` y dale Play.
3. Clic para seleccionar; botones Mover / Recolectar / Construir / Entrenar / Atacar / ¡Batalla!; F5 guarda, F9 carga.

---

## Tecnologías

C#, Unity (UGUI), .NET Tasks/ThreadPool, `lock`, colecciones concurrentes, REST + JSON (`UnityWebRequest`), `System.IO`, NUnit.

## Estado

Concurrencia, sincronización y cancelación integradas y probadas: mapa y recursos, selección por clic y HUD, movimiento, recolección con depósito, construcción y entrenamiento con costos, ataque con validaciones, batalla total, IA completa, guardado/carga, bitácora y resultado final, 313 pruebas verdes y Unity sin errores.

## Referencias

- Microsoft. (s/f). *Managed Threading (C#)*. Learn.microsoft.com. Recuperado el 1 de septiembre de 2026, de https://learn.microsoft.com/dotnet/standard/threading/managed-threading-basics
- Microsoft. (s/f). *Sockets (System.Net.Sockets)*. Learn.microsoft.com. Recuperado el 1 de septiembre de 2026, de https://learn.microsoft.com/dotnet/api/system.net.sockets
- Unity Technologies. (s/f). *Unity Manual: Multithreading and native plug-ins*. Docs.unity3d.com. Recuperado el 1 de septiembre de 2026, de https://docs.unity3d.com/Manual/UnderstandingPerformanceThreading.html
