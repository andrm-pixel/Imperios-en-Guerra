# Informe formal del proyecto — Imperios en Guerra

**Asignatura:** Programación Avanzada — **Docente:** Nancy Yaneth Gelvez García — **Monitor:** Brayan Javier Ramírez Mendoza (20222020139).
**Proyecto:** videojuego de estrategia en tiempo real (RTS) inspirado en Age of Empires, en C# y Unity (build de escritorio).
**Modalidad entregada:** Humano vs Máquina. **Repositorio:** `andrm-pixel/Imperios-en-Guerra`, ramas `main` (estable) y `develop` (integración).
**Fecha del informe:** 27 de septiembre de 2026. **Suite de pruebas:** 313/313 verdes (NUnit) + compilación de scripts Unity sin errores.

> Dónde está cada cosa: `Modelo/` (lógica + hilos), `Assets/Scripts/Vistas/` (solo presentación), `Assets/Scripts/Controladores/` (puente sin hilos), `Controlador/` (servidor REST externo opcional), `tests/` (NUnit), `docs/` (esta documentación).

---

## 1. Arquitectura MVC

### 1.1. Reparto de responsabilidades

| Capa | Carpeta / namespace | Contenido | Regla que cumple |
|---|---|---|---|
| **Modelo** | `Modelo/ImperiosEnGuerra.Modelo` (78 clases) | `Core` (Partida, Jugador, InicializadorPartida), `Map`, `Unidades`, `Edificios`, `Recursos`, `Acciones` (Operacion*), `Servicios` (EstadoPartidaService, MotorAcciones), `IA`, `Movimiento`, `Reglas`, `Concurrencia`, `Persistencia`, `Contratos`, `Mapeadores` | Clases C# planas, **cero referencias a `UnityEngine`**. Toda la lógica del juego y todos los hilos viven aquí |
| **Vista** | `Assets/Scripts/Vistas/` (`ImperiosEnGuerra.Vistas`) | `VistaPartida`, `VistaHud`, `HudDisposicion`, `EntidadSeleccionableVista`, `BarraVidaVista` + escenas y sprites Tiny Swords | Solo lee el estado y dibuja. **Nunca modifica el Modelo**: no llama a operaciones, solo presenta DTOs y emite eventos de UI |
| **Controlador** | `Assets/Scripts/Controladores/` (`ImperiosEnGuerra.Controladores[.Red/.ApiInterna]`) y `Controlador/` | `ControladorSeleccion`, `ControladorAcciones`, `ControladorConexionApi`, `ApiInternaJuego`, `AdaptadorEstadoPartida`, DTOs, servidor REST `Program.cs` | Recibe clics/eventos, valida con `ReglasAcciones` del Modelo, lanza workers y devuelve resultados a la Vista. **No crea `Thread`/`Task` propios**: la concurrencia está encapsulada en el Modelo |

### 1.2. Flujo de una orden (ejemplo: mover)

1. La Vista (`ControladorSeleccion`) captura el clic y lo entrega como casilla/entidad, sin saber qué significa.
2. El Controlador (`ControladorAcciones.PrepararAccion`, `Assets/Scripts/Controladores/ControladorAcciones.cs:508`) valida la opción con `ReglasAcciones` del Modelo y pide la orden a `ControladorConexionApi`.
3. El Modelo ejecuta en un worker (`MotorAcciones.IniciarMovimiento`, `Modelo/.../Servicios/MotorAcciones.cs:151`) y publica el resultado en una cola thread-safe.
4. El Controlador sondea el resultado cada 0,1 s (`EsperarProcesoInterno`, `Assets/Scripts/Controladores/Red/ControladorConexionApi.cs:646`) y sincroniza la Vista **en el hilo principal de Unity**.

Sin el Modelo el juego no funciona: los controladores exigen la API interna (`ExigirApiInterna`) y la Vista solo sabe dibujar DTOs.

### 1.3. Diagrama y justificación de partes fundamentales

Ver `docs/DIAGRAMAS.md` (arquitectura, clases y flujos en Mermaid). Las piezas sin las cuales el programa no funciona son:

1. **`EstadoPartidaService`** — única puerta thread-safe al estado (`lock (sincronizacion)` en todos sus métodos públicos) y única fuente de la bitácora.
2. **`MotorAcciones` + `TareasJuego`** — todos los workers del juego; sin ellos no hay recolección, construcción, entrenamiento, ataque ni IA.
3. **`OperacionAtaque` + `InteligenciaMaquina`** — daño real y rival; contienen la condición de victoria.
4. **`ControladorConexionApi` + `ApiInternaJuego`** — puente que traduce DTOs y devuelve resultados al hilo principal; sin él la Vista queda ciega.
5. **`InicializadorPartida` + `Mapa`** — mapa 15×15 válido (sin traslapes ni salidas); todo el gameplay presupone esa validez.

---

## 2. Concurrencia e hilos

### 2.1. Dónde se crean los hilos

Todos los hilos nacen en **un solo punto**: `TareasJuego.Iniciar` (`Modelo/.../Concurrencia/TareasJuego.cs:33`), que usa `Task.Run` (ThreadPool de .NET). Nadie más en el proyecto crea hilos. La Vista y el Controlador usan **corutinas** de Unity (hilo principal), nunca `Thread`/`Task`.

### 2.2. Tabla de workers: propósito, sincronización y finalización

| Worker (nombre de proceso) | Quién lo lanza | Propósito | Sincronización | Finalización |
|---|---|---|---|---|
| `MOVER` | `MotorAcciones.IniciarMovimiento` (:151) | Llevar una unidad paso a paso (0,5 s/casilla ÷ velocidad) con replan hasta 12 intentos | `IntentarIniciarOrden` (una orden por unidad); cada paso pasa por `EstadoPartidaService` bajo `lock` | `CompletarOrden` en `finally`; `Cancelar(Guid)` / `CancelarTodos()` |
| `RECOLECTAR` | `IniciarRecoleccion` (:296) | Bucle aldeano: ir al nodo → extraer por tasa cada 1 s → ir al Castillo → depositar → repetir hasta agotar nodo | Carga/nodo/saldo bajo locks (`Aldeano`, `Recurso`, `RecursosJugador`); nodo agotado se **retira del mapa** (`RecolectarPaso`, `EstadoPartidaService.cs:404-409`) dejando la casilla libre | Mensaje `Nodo agotado… En espera de órdenes`; cancelable |
| `CONSTRUIR` | `IniciarConstruccion` (:668) | Reservar costo → crear obra → 10 avances de +10 % cada 0,7 s (7 s total) | Costo con descuento atómico (`IntentarGastar` 5 recursos bajo un solo lock); obra en `Jugador` accedida bajo `lock` del servicio | `AvanzarObra` crea el `Castillo`; rollback `CancelarObra` + `ReembolsarCosto` |
| `ENTRENAR` | `IniciarEntrenamiento` (:844) | Cola FIFO del Castillo; espera su turno (sondeo 10 ms); 5 s × factor (Aldeano 1,0 / Soldado 1,2 / Arquero 1,3); nace en la casilla clicada si está libre, si no en la más cercana | Cola bajo `lock` de `Castillo`; spawn con `Ocupar()` + rollback (`EliminarUnidad` + `Liberar`) | `CompletarEntrenamientoConSpawn`; rollback cancela cola y reembolsa |
| `ATACAR` | `IniciarAtaque` (:977) | Hasta 40 rondas: si hay alcance golpea cada 1 s; si no, se acerca (A*) | Daño bajo locks de vida (`Unidad`/`Edificio`); casilla liberada al destruir | Termina al destruir el objetivo o agotar rondas; 1 worker por unidad en `IniciarBatalla` (:1189) |
| `IA_MAQUINA` | `IniciarIA` (:1134) | Turno de la máquina cada 2 s: huida de aldeanos (`DefensaHumana`), caza militar en radio 7, retirada de heridos (vida ≤ ¼), guardia del Castillo | Todo el turno corre dentro de `EjecutarTurnoMaquina` bajo `lock`; sin hilos propios | Termina solo con `¡Victoria!`; se cancela y relanza al cargar progreso (F9) |

### 2.3. Cómo se evita que Unity se congele y cómo se evitan carreras

- **Nunca se toca `UnityEngine` desde un worker.** Los workers solo mutan el Modelo y publican `ResultadoProcesoConcurrente` (inmutable) en `ConcurrentQueue` + `ConcurrentDictionary` (`TareasJuego.cs:16-22`).
- **El hilo principal sondea, no espera.** `EsperarProcesoInterno` consulta cada 0,1 s y entre consultas refresca la Vista con snapshots (`ControladorConexionApi.cs:646-731`); la ventana jamás se bloquea.
- **Un solo `lock` maestro.** `EstadoPartidaService` serializa todo acceso al estado (`lock (sincronizacion)`), y los datos compartidos tienen su propio lock fino: vida (`Unidad`, `Edificio`), carga (`Aldeano`), cantidad (`Recurso`), saldos (`RecursosJugador`), cola de entrenamiento (`Castillo`). Sin doble gasto, sin doble ocupación de casilla, sin lecturas a medias.
- **Cancelación cooperativa.** Cada worker recibe `CancellationToken`; `SustituirOrden` cancela la orden anterior de la unidad y espera (máx. 3 s) a que quede libre antes de lanzar la nueva.

---

## 3. Comunicación en red

### 3.1. Alcance real (honestidad técnica)

El rival del juego es la **IA** (`InteligenciaMaquina`), no otro humano: no hay multijugador humano-humano. La comunicación en red implementada es **REST + JSON** entre la app Unity y el Modelo, en dos modos:

| Modo | Cómo viaja la orden | Cuándo se usa |
|---|---|---|
| **Interna (por defecto)** | Llamada en-proceso a `ApiInternaJuego` → `MotorAcciones`; sin sockets ni terminal | Juego normal Humano vs Máquina |
| **Externa (alterna)** | HTTP a `http://localhost:5086` + `UnityWebRequest`; `POST /api/partida/*-concurrente` → `202` con `procesoId` → `GET /api/procesos/{id}/resultado` hasta `Completado` | Servidor `Controlador/ImperiosEnGuerra.Controlador/Program.cs` en ejecución (`dotnet run --project Controlador/...`) |

### 3.2. Endpoints del servidor (`Controlador/ImperiosEnGuerra.Controlador/Program.cs`)

`GET /api/estado`, `GET /api/modelo/prueba`, `POST /api/partida/iniciar`, `GET /api/partida`, `POST /api/partida/mover[-concurrente]`, `POST /api/partida/recolectar[-concurrente]`, `POST /api/partida/construir[-concurrente]`, `POST /api/partida/entrenar[-concurrente]`, `POST /api/partida/atacar[-concurrente]`, `GET /api/procesos/{id}/resultado`, `GET /api/procesos/resultado`, `POST /api/procesos/{id}/cancelar`.

### 3.3. Formato de mensajes (JSON) y escucha sin bloqueo

- **Peticiones:** `{ unidadId, destino:{x,y} }`, `{ aldeanoId, objetivo:{x,y} }`, `{ atacanteId, objetivoId }`, etc. (`Assets/Scripts/Controladores/Red/Contratos/*.cs`).
- **Respuestas de proceso:** `{ procesoId, nombre, estado, hiloTrabajoId, exito, mensaje, errorTecnico }` (`ProcesoConcurrenteDto.cs`).
- **Escucha sin bloqueo:** no hay hilo de escucha dedicado; las corutinas sondean (`mover/recolectar` cada 0,1 s con reintento a 0,5 s ante fallo HTTP; `entrenar` cada 0,25 s; `construir` refresca cada 0,5 s). El hilo principal nunca se bloquea.
- **Errores de comunicación:** timeouts (5–15 s), aviso `HTTP {código}` en HUD, reintentos de consulta, y mensajes claros cuando una función solo existe en modo interno (batalla total, cancelar, F5/F9). Todo endpoint y worker está envuelto en `try-catch`: un fallo de red o de IO muestra mensaje y sincroniza estado, jamás tumba el juego.

---

## 4. Archivos generados por el programa

Todos los escribe `ServicioArchivos` (`Modelo/.../Persistencia/ServicioArchivos.cs`) en la carpeta de datos persistentes:

- **Windows:** `%USERPROFILE%\AppData\LocalLow\DefaultCompany\ImperiosEnGuerraUnity\DatosPartida\`
- **Linux/Mac:** `Application.persistentDataPath/DatosPartida/`

| Archivo | Cuándo se escribe | Formato / ejemplo real |
|---|---|---|
| `configuracion.txt` | Al iniciar la partida (`GuardarConfiguracionInicial`) | `PARTIDA`, `[JUGADOR_HUMANO]` / `[JUGADOR_MAQUINA]` con `Nombre=`, `Tipo=`, `Mapa=15x15`, saldos `Oro=/Madera=/Comida=/Piedra=/Hierro=`, listas ordenadas `Edificios:` (`Castillo=(13,7)`), `Unidades:` (`Aldeano=(12,7)`), `RecursosMapa:` (`Oro=(10,6)`); números con cultura invariable, saltos LF |
| `log_partida.txt` | Cada acción y evento (`RegistrarEvento`, append) | Tres líneas por evento (formato de la guía): `Turno: Griegos` / `Accion: Ataque` / `Resultado: Impacto: 25 de daño a Castillo (vida 175).` Acciones: `Partida, Movimiento, Recoleccion, Construccion, Entrenamiento, Ataque, Turno maquina, Progreso, Victoria`. Si el disco falla, se escribe en consola y la partida continúa |
| `resultado_final.txt` | Al haber ganador (`GuardarResultadoFinalSeguro`) | `Ganador=Troya` / `Perdedor=Griegos` / `UnidadesRestantesMaquina=4` (ejemplo real de partida) |
| `progreso.txt` | Tecla **F5** guarda / **F9** carga (`ProgresoPartida` serializa `PROGRESO_V1`: mapa, saldos, edificios/unidades con vida y carga, recursos restantes) | Texto plano versionado con secciones `[JUGADOR_*]`; `Deserializar` rechaza con `FormatException` versiones o tipos desconocidos |

---

## 5. Acciones, validaciones y victoria

- **Acciones desde la UI:** Mover, Recolectar, Construir (Castillo), Entrenar/Invocar (Aldeano, Soldado, Arquero), Atacar y ¡Batalla! (todo el ejército ataca sin frenar obras). Selección directa sobre el mapa con raycast (`ControladorSeleccion`, `VistaPartida.TryObtenerCoordenadaLogica`).
- **Validaciones (antes de gastar nada):** recursos suficientes (`ConfiguracionEconomia`: Castillo 20/50/0/20, Soldado 5/0/15/0/5, Arquero 10/0/10/0/8), coordenadas dentro del 15×15, casilla libre y sin traslapes (`Mapa.PuedeColocar`), unidad disponible y del tipo correcto (`ReglasAcciones`), alcance de ataque (Soldado 1, Arquero 4). Cada rechazo explica el motivo en el HUD.
- **Victoria total (castillo Y ejército):** humana si la máquina pierde su Castillo **y** todas sus unidades (`OperacionAtaque.EsVictoriaHumana`); la máquina gana solo arrasando todas las unidades humanas (`EsVictoriaMaquina`, porque el humano puede reconstruir su Castillo con un aldeano). Se anuncia en el HUD (victoria con el mensaje del worker; derrota con `VigilarIA` cada 0,5 s, una sola vez) y se guarda en `resultado_final.txt`.

---

## 6. Referencias

1. Microsoft. (s/f). *Managed Threading (C#)*. Learn.microsoft.com. Recuperado el 1 de septiembre de 2026, de https://learn.microsoft.com/dotnet/standard/threading/managed-threading-basics
2. Microsoft. (s/f). *Sockets (System.Net.Sockets)*. Learn.microsoft.com. Recuperado el 1 de septiembre de 2026, de https://learn.microsoft.com/dotnet/api/system.net.sockets
3. Unity Technologies. (s/f). *Unity Manual: Multithreading and native plug-ins*. Docs.unity3d.com. Recuperado el 1 de septiembre de 2026, de https://docs.unity3d.com/Manual/UnderstandingPerformanceThreading.html
4. Documentos del repositorio: `docs/CUMPLIMIENTO_GUIA.md` (matriz guía vs implementación), `docs/DIAGRAMAS.md`, `docs/PRUEBA_ESCRITORIO.md`, `docs/CLASES.md`, `docs/GUIA_COMBATE.md`, `docs/INSTALACION_TINY_SWORDS.md`.
