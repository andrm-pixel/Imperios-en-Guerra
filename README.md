# Imperios en Guerra

Videojuego de estrategia en tiempo real (RTS) en C# y Unity, inspirado en Age of Empires. Proyecto académico de Programación Avanzada (docente Nancy Yaneth Gelvez García).

---

## Qué es el juego

Tú comandas a los **Griegos** (castillo a la derecha) contra **Troya**, la máquina (castillo a la izquierda), en un mapa de **15×15**.

**Lo que ves al abrirlo:** tu castillo, tus aldeanos, árboles de madera, minas de oro, arbustos de comida, piedras y hierros regados por el mapa, y arriba tus monedas.

**Cada pieza sirve para algo:**

1. El **aldeano** es tu trabajador: recoge recursos y construye. Sin aldeanos no hay nada.
2. El **soldado** pega fuerte de cerca (25 de daño, alcance 1) y aguanta mucho (120 de vida).
3. El **arquero** pega de lejos (alcance 4) pero es más débil (90 de vida, 15 de daño).
4. El **castillo** es tu casa principal: ahí nacen tus tropas y ahí guardas lo recogido. Si lo pierdes y te quedas sin tropas, pierdes.

**Cómo se juega:**

1. Manda aldeanos a recoger oro, madera, comida, piedra y hierro.
2. Con esos recursos puedes **invocas** más aldeanos, soldados y arqueros desde tu castillo a cambio de cierta cantidad de recursos especificos.
3. Mueves tus tropas con clics, atacas al enemigo y cuidas tu castillo.
4. **Ganas** si destruyes el castillo enemigo **y** todas sus unidades. **Pierdes** si te quedas sin unidades.
5. **F5 guarda** tu progreso e el juego y **F9 carga** ese progreso que guardaste. Cada partida deja archivos: configuración inicial, bitácora de acciones y resultado final.

La máquina no es tonta, sus tropas cazan a las tuyas, cuidan su castillo, tus aldeanos huyen del peligro y sus heridos se retiran. Además ves **barras de vida** sobre cada unidad y edificio.

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
Controlador/   Servidor REST externo opcional (sin hilos propios)
  ImperiosEnGuerra.Controlador/
tests/         Pruebas .NET (NUnit, 313 verdes)
docs/          Documentación (guía, combate, instalación)
scripts/       Instalador de sprites Tiny Swords
```

**Qué es cada parte (en simple):**

- **`Modelo/`** — el cerebro. Aquí están las reglas, las piezas (aldeanos, soldados, arqueros, castillos, recursos, mapa) y los trabajadores (hilos) que hacen todo a la vez. Si quieres saber por qué el juego hace algo, la respuesta está aquí.
- **`Assets/`** — lo que ves y tocas. `Scenes/` trae el mapa jugable (`SampleScene`); `Scripts/Vistas/` dibuja todo; `Scripts/Controladores/` lleva tus clics al cerebro; `Art/` trae los dibujos (se instalan aparte); `Plugins/` trae el cerebro compilado para que Unity lo use.
- **`Controlador/`** — un programa aparte que atiende por internet (REST). Solo se usa en modo externo; el juego normal no lo necesita.
- **`tests/`** — 313 pruebas automáticas que revisan que nada se rompa (movimiento, recolección, ataques, victoria, guardado).
- **`docs/`** — papeles de ayuda: cómo cumple la guía, cómo se pelea y cómo instalar los dibujos.
- **`scripts/`** — instala los dibujos Tiny Swords en su lugar con un doble clic.
- **`Packages/` y `ProjectSettings/`** — configuración interna de Unity (versión, paquetes, calidad). No se tocan.

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
