# Clases del proyecto — explicación general y por clase

Cómo leer este documento: primero la explicación general del código (§1), luego cada clase con su ruta, su responsabilidad y sus miembros principales con línea aproximada (§2–§5). Base: suite 313/313 + Unity 0 errores.

---

## 1. Explicación general de cómo funciona el código

El juego es un RTS Humano vs Máquina con **una sola regla de arquitectura**: todo lo que decide el juego vive en el **Modelo** (C# puro, sin Unity), todo lo que se ve vive en la **Vista** (Unity), y el **Controlador** traduce entre ambos sin hilos propios.

1. **Arranque.** `ApiInternaJuego` crea `InicializadorPartida`, que valida y construye el mapa 15×15 (castillos en (13,7) y (1,7), 16 nodos por bando, aldeanos iniciales, saldos) y lo guarda en `configuracion.txt`. Lanza el worker continuo de la IA.
2. **Una orden.** El jugador hace clic (Vista) → el Controlador valida con `ReglasAcciones` → `MotorAcciones` abre un worker en el ThreadPool (`TareasJuego`) → el worker ejecuta la operación paso a paso con retardos, todo bajo `lock` → publica el resultado en una cola thread-safe → el Controlador lo recoge cada 0,1 s y refresca la Vista en el hilo principal.
3. **El rival.** Cada 2 s la IA mueve sus tropas: huyen los aldeanos en peligro (`DefensaHumana`), cazan militares en radio 7, se retiran los heridos y los demás custodian el Castillo.
4. **El final.** Cada destrucción revisa la victoria (castillo Y ejército). El ganador se anuncia en el HUD, se escribe en la bitácora (`log_partida.txt`) y en `resultado_final.txt`. Con F5/F9 se guarda y carga el progreso (`progreso.txt`).

---

## 2. Modelo — dominio (`Core`, `Map`, `Unidades`, `Edificios`, `Recursos`)

### `Modelo/.../Core/Partida.cs`
Agrupa a los dos jugadores validando tipos. `JugadorHumano` (~L13), `JugadorMaquina` (~L17), constructor (~L26).

### `Modelo/.../Core/Jugador.cs`
Datos del participante y sus listas (`List<Unidad>`, `List<Edificio>`, obras). `Nombre/Tipo/Mapa/Recursos` (~L22-34), vistas readonly `Unidades/Edificios/ObrasConstruccion` (~L39-55), `Agregar*/Eliminar*` (~L107-182). Sin lock propio: lo protege el servicio.

### `Modelo/.../Core/TipoJugador.cs`
Enum `Humano/Maquina` (~L11-15).

### `Modelo/.../Core/InicializadorPartida.cs`
Construye la partida inicial válida. `AnchoMapa/AltoMapa = 15` (~L16-18), `CentroHumano (13,7)` (~L21), `CentroMaquina (1,7)` (~L23), `RecursosHumano/Maquina` (16 nodos por bando, ~L26-71), `GuarnicionMaquina` (2 posiciones, ~L74), `Crear(...)` (~L109: valida, planifica aldeanos sin traslapes, configura mapas y saldos). Sin traslapes ni salidas del mapa por construcción.

### `Modelo/.../Core/ConfiguracionInicioPartida.cs`
Balance inicial: 2 aldeanos, saldos oro 0 / madera 20 / comida 30 / piedra 0 / hierro 0 (~L15-60); `AplicarSaldoInicial` (~L104).

### `Modelo/.../Map/Coordenada.cs`
Posición lógica inmutable `X/Y` (~L11-22).

### `Modelo/.../Map/Casilla.cs`
Celda con tránsito y ocupación independientes. `Ocupar()` (~L46, solo si libre), `Liberar()` (~L60), `CambiarTransitabilidad` (~L38).

### `Modelo/.../Map/Mapa.cs`
Cuadrícula `Casilla[,]` + lista de recursos físicos. `ObtenerCasilla` (~L74), `EstaDentroDeLimites` (~L89), `PuedeColocar` (~L105: libre y sin recurso), `ObtenerRecursoEn` (~L122), `ColocarRecurso` (~L146), **`RetirarRecurso`** (~L169: quita el nodo agotado y deja la casilla libre para mover, construir o entrenar).

### `Modelo/.../Unidades/Unidad.cs` (abstracta)
Base de toda unidad: `Id` (~L15), `Vida/VidaMaxima/PuntosAtaque/AlcanceAtaque` (~L47-62), `Estado/OrdenActiva/Disponible` (~L26-42). `RecibirDano()` (~L141, bajo lock, avisa si destruye), `EstaViva` (~L163), `IntentarIniciarOrden` (~L177: una sola orden a la vez), `IntentarReemplazarOrden` (~L196), `Cancelar/CompletarOrden` (~L211-219), `EstablecerDestino` (~L241).

### `Modelo/.../Unidades/Aldeano.cs`
Civil (50 vida, sin ataque) con carga monorecurso. `CapacidadCarga` 10 (~L16-27), `RecolectarDesde(recurso, tasa)` (~L114: no mezcla tipos, respeta capacidad), `VaciarCarga` (~L176). Carga bajo lock.

### `Modelo/.../Unidades/UnidadMilitar.cs` (abstracta)
Marca a Soldado/Arquero para batalla e IA. Solo constructores (~L16-26).

### `Modelo/.../Unidades/Soldado.cs` — infantería 120 vida, 25 daño, alcance 1 (~L15).
### `Modelo/.../Unidades/Arquero.cs` — a distancia 90 vida, 15 daño, alcance 4 (~L14).
### `Modelo/.../Unidades/EstadoUnidad.cs` — `Idle/Moviendo/Recolectando/Construyendo/Atacando` (~L11-27).

### `Modelo/.../Edificios/Edificio.cs` (abstracta)
Base de edificios: `Id/Coordenada/Vida/VidaMaxima` (~L18-33, Castillo = 500), `RecibirDano` (~L85, bajo lock), `EstaDestruido` (~L107).

### `Modelo/.../Edificios/Castillo.cs`
Centro urbano con **cola FIFO** de entrenamientos (copia bajo lock, ~L53). `EncolarEntrenamiento` (~L81), `EsPrimero` (~L104), `AvanzarEntrenamiento` (~L121, solo el frente), `CompletarEntrenamiento(id)` (~L149), `CancelarEntrenamiento` (~L171). Todo bajo `lock`.

### `Modelo/.../Edificios/ObraConstruccion.cs`
Construcción en curso 0–100 (`Avanzar`, ~L74; `Terminada` ≥ 100, ~L35). Guarda aldeano ejecutor, tipo y coordenada (~L14-26).

### `Modelo/.../Edificios/EntrenamientoPendiente.cs`
Entrenamiento en cola: tipo, punto de reunión y progreso 0–100 (`Avanzar`, ~L55). Protegido por el lock del Castillo.

### `Modelo/.../Edificios/BuscadorCasillaSpawn.cs`
Busca la casilla libre más cercana al edificio por anillos Manhattan, evitando tránsito bloqueado, recursos y entidades de ambos bandos (`Buscar`, ~L20; null si no hay).

### `Modelo/.../Edificios/ConfiguracionEntrenamiento.cs`
Factores de tiempo: Aldeano 1,0 / Soldado 1,2 / Arquero 1,3 (~L17-27); `IntentarObtenerFactor` (~L36).

### `Modelo/.../Recursos/Recurso.cs`
Nodo físico con cantidad sincronizada (100 inicial, ~L14). `CantidadRestante/Agotado` bajo lock (~L34-57), `Extraer` (~L107: entrega `Min(pedido, restante)`, nunca negativo).

### `Modelo/.../Recursos/RecursosJugador.cs`
Saldos del jugador (los 5 tipos en 0 al inicio, ~L17). `ObtenerCantidad` (~L35), `Agregar` (~L50), `IntentarGastar` simple (~L93) y de costo completo **bajo un solo lock** (~L114: imposible gastar dos veces lo mismo), `Reintegrar` (~L146: reembolsos al cancelar).

### `Modelo/.../Recursos/CostoRecursos.cs`
Costo inmutable oro/madera/comida/piedra/hierro (~L13-39), `EsCero` (~L67).

### `Modelo/.../Recursos/ConfiguracionEconomia.cs`
Precios del prototipo (~L22-46): Castillo 20/50/0/20, Aldeano 0/0/10, Soldado 5/0/15/0/5, Arquero 10/0/10/0/8. `IntentarObtenerCostoEdificio/Unidad` (~L55-89).

### `Modelo/.../Recursos/ConfiguracionRecoleccion.cs`
Tasa por ciclo de cada recurso (5 por defecto, ~L21); `ObtenerTasa` (~L64).

### `Modelo/.../Recursos/TipoRecurso.cs` — `Oro/Madera/Comida/Piedra/Hierro`.

---

## 3. Modelo — reglas, movimiento, acciones y resultados

### `Modelo/.../Reglas/ReglasAcciones.cs` (estática)
Puerta de validación que usa el Controlador: `PermiteMover` (~L36), `PermiteRecolectar/Construir` (aldeano humano, ~L53-70), `PermiteEntrenar` (Castillo humano, ~L80), `PermiteAtacar` (militar propia, ~L95), `EsObjetivoAtaqueValido` (entidad de la Máquina con id, ~L109).

### `Modelo/.../Movimiento/RutaAStar.cs`
A* en 4 direcciones con heurística Manhattan; respeta transitabilidad, ocupación, recursos y bloqueos (`Buscar`, ~L28-48, estructuras locales).

### `Modelo/.../Movimiento/RutaMovimiento.cs`
Valida la intención de mover (unidad propia disponible, destino dentro/libre/sin recurso ni entidad) y prepara la ruta **sin mover** (`Preparar`, ~L45).

### `Modelo/.../Movimiento/ResultadoRuta.cs` / `ResultadoPlanMovimiento.cs`
Resultados inmutables del A* (`Exitosa/Imposible`) y de la validación (`Exitoso/Fallido` con mensaje y pasos).

### Operaciones (`Modelo/.../Acciones/Operacion*.cs`) — cada una valida y ejecuta una sola cosa
- **`OperacionAtaque`** (~L26): atacante militar propia, objetivo enemigo en alcance Manhattan; aplica daño, elimina y `Liberar()` la casilla si destruye; informa restantes o `¡Victoria!`. `EsVictoriaHumana` (~L157: máquina sin Castillo Y sin unidades), `EsVictoriaMaquina` (~L177: humano sin unidades).
- **`OperacionMovimiento`** (~L19) / **`OperacionPasoMovimiento`** (~L21): plan completo / avance de exactamente 1 paso ortogonal válido.
- **`OperacionRecoleccion`** (~L21): valida la intención (aldeano libre, nodo existente no agotado).
- **`OperacionPasoRecoleccion`** (~L25): exige orden `Recolectar` y distancia 1; extrae **un ciclo a la carga** (no al saldo).
- **`OperacionDepositoRecoleccion`** (~L24): junto al Castillo, vacía la carga al saldo.
- **`OperacionConstruccion`** (~L21) / **`OperacionEntrenamiento`** (~L21): versiones inmediatas legadas (solo Castillo; solo tipos permitidos); el juego real usa el flujo concurrente del servicio.
- **`FabricaUnidades`** (`Crear`, ~L18: Aldeano/Soldado/Arquero por nombre), **`TipoAccionJuego`** (órdenes Mover/Recolectar/Construir/Entrenar/Atacar), **`ResultadoAccion`** (`Exitoso/Fallido` + mensaje), **`Solicitud*`** (intenciones con ids/coordenadas: `SolicitudAtaque`, `SolicitudMovimiento`, `SolicitudRecoleccion`, `SolicitudConstruccion`, `SolicitudEntrenamiento`, base `SolicitudAccion`).

### Aproximaciones y resultados (`Recoleccion/`, `Edificios/Resultado*`)
`AproximacionRecurso`, `AproximacionDeposito` y `AproximacionConstruccion` calculan la **casilla libre adyacente + ruta más corta** al nodo, Castillo u obra (`Preparar`, ~L56; si ya está al lado, pasos vacíos; si no hay ruta, fallo reintentable). Los `Resultado*` (`ResultadoPasoRecoleccion` con `CantidadExtraida/CargaActual/RecursoAgotado`, `ResultadoDepositoRecoleccion`, `ResultadoAproximacion*`, `ResultadoProgresoConstruccion/Entrenamiento`, `ResultadoSpawnEntrenamiento`) transportan cada paso del worker de forma inmutable.

---

## 4. Modelo — servicios, IA, concurrencia, persistencia y contratos

### `Modelo/.../Servicios/EstadoPartidaService.cs`
**La única puerta al estado**: cada método público corre bajo `lock (sincronizacion)`. Delegación por tema: movimiento (`MoverUnidad` :51, `PrepararMovimientoProgresivo` :91, `AvanzarMovimiento` :208, órdenes :137-185), recolección (`PrepararAproximacionRecurso/Deposito` :232-284, `RecolectarPaso` :383 — **retira el nodo agotado**, :404-409; `DepositarCarga` :310; `RecursoExiste/Disponible` :335-373), construcción (costos :460, obra :559-768, `Construir` :803), entrenamiento (cola :876-1168, **spawn** :1008-1092 — respeta la casilla clicada si está libre, si no la más cercana, con rollback), ataque (`Atacar` :1240 — victoria humana → evento + archivo), estado (`ObtenerEstado` :1295, `EstablecerPartida` :1309), progreso F5/F9 (:1323-1354), máquina (`EjecutarTurnoMaquina` :1427 — victoria máquina → evento + archivo) y bitácora (`RegistrarResultado` :1512, `RegistrarEventoGuia` :1546 con formato `Turno/Accion/Resultado`, `NombreAccion` :1529).

### `Modelo/.../Servicios/MotorAcciones.cs`
**Todos los workers del juego** sobre `TareasJuego`, con retardos configurables ( ctor :43-53: movimiento 0,5 s/casilla, recolección 1 s, construcción 7 s, entrenamiento 5 s, ataque 1 s; ctor de pruebas con un solo retardo, :65-78). `IniciarMovimiento` (:151: plan + pasos + replan), `IniciarRecoleccion` (:296: bucle extraer→depositar→repetir hasta agotar), `IniciarConstruccion` (:668: reserva costo, obra, 10 avances, rollback con reembolso), `IniciarEntrenamiento` (:844: reserva, cola, espera turno, avances, spawn, rollback), `IniciarAtaque` (:977: 40 rondas golpear/acercar), `IniciarIA` (:1134: turno cada 2 s hasta victoria), `IniciarBatalla` (:1189: un worker por militar + clase `ProcesoBatalla`), `Cancelar/CancelarTodos` (:1276-1287), consultas de resultado (:1301-1327).

### `Modelo/.../IA/InteligenciaMaquina.cs`
Rival del juego, sin hilos propios. `EjecutarTurno` (:32): 1) `DefensaHumana` (aldeanos en peligro huyen); 2) cada militar viva actúa (`ActuarConUnidad` :87): herida (≤ ¼ vida, `DebeRetirarse` :546) o sin objetivo → `RegresarAGuardia` (radio 2); con objetivo en radio 7 (`BuscarObjetivoCercano` :136 — **militares primero**, luego resto) → `Golpear` (:245: daño, eliminar, liberar, bitácora) o `Acercarse` (:319: A* + 1 paso); 3) si el humano queda sin unidades → `¡Victoria!`.

### `Modelo/.../IA/DefensaHumana.cs`
Huida automática: cada Aldeano humano libre con una militar enemiga a ≤ 3 (`RadioPanicoAldeano` :25) da **1 paso** a la casilla vecina más segura (`Ejecutar` :33, `IntentarHuir` :77). Nunca interrumpe una orden del jugador.

### `Modelo/.../Concurrencia/TareasJuego.cs` (`IDisposable`)
**El único lugar que crea hilos** (`Task.Run`, :33). Guarda cancelaciones en `ConcurrentDictionary`, resultados en `ConcurrentQueue` + diccionario por id (:16-22); `Cancelar/CancelarTodos` (:111-127), `IntentarObtenerResultado` por id o FIFO (:142-156), `Dispose` (:200). Estado de cierre con `Volatile/Interlocked`, sin `lock`. `ProcesoConcurrente` (handle Id/Nombre/Task) y `ResultadoProcesoConcurrente` (`Completado/Cancelado/Fallido` + resultado o error técnico) completan el mecanismo.

### `Modelo/.../Persistencia/ServicioArchivos.cs`
Toda la IO del juego: `GuardarConfiguracionInicial` (:69: dump determinista ordenado), `RegistrarEvento` (:166: append a `log_partida.txt`), `GuardarProgreso/LeerProgreso` (:181-194: `progreso.txt` F5/F9), `GuardarResultadoFinal` (:216). Crea la carpeta base y traduce errores de disco en excepciones claras.

### `Modelo/.../Persistencia/ProgresoPartida.cs` (estática)
Guarda/carga en texto plano versionado `PROGRESO_V1`: mapa, saldos, edificios/unidades (con vida y carga) y recursos restantes (`Serializar` :28, `Deserializar` :126 con rechazo estricto de lo desconocido).

### `Modelo/.../Contratos/` y `Modelo/.../Mapeadores/`
Peticiones (`Atacar/Mover/Recolectar/Construir/Entrenar/IniciarPartidaRequest` con ids en texto y coordenadas) y `EstadoPartidaResponse` (mapa, jugadores con recursos/edificios/obras/unidades **con vida**, economía con costos). `PartidaEstadoMapper.Convertir` (dominio → respuesta) y `PartidaRequestMapper` (respuesta → dominio, con validación estricta).

---

## 5. Vista y Controlador (`Assets/Scripts`, `Controlador/`)

### Vistas — solo presentan (`ImperiosEnGuerra.Vistas`)
- **`VistaPartida`** (`Vistas/VistaPartida.cs`): dibuja y sincroniza mapa y entidades desde el DTO (`Renderizar` :79 total, `Sincronizar` :115 incremental que conserva la selección); mueve sprites con interpolación (`ActualizarMovimientoUnidad` :869); convierte clics a casillas (`TryObtenerCoordenadaLogica` :972); crea/actualiza **barras de vida** (:292-501).
- **`VistaHud`** (`Vistas/VistaHud.cs`): textos y botones; **emite eventos, no llama a la API**. `MostrarRecursos` (:144), `MostrarSeleccion` (:154), `MostrarSelectorEntrenamiento` (:202), `MostrarOpciones` (:209: solo botones válidos), `MostrarMensaje` (:220: trunca a 110), eventos `AccionSolicitada` (:38) y `TipoUnidadSolicitado` (:40).
- **`HudDisposicion`** (`Vistas/HudDisposicion.cs`): fija el HUD por código al arrancar (`Aplicar` :13) para que ninguna escena vieja muestre paneles cruzados.
- **`EntidadSeleccionableVista`** (`Vistas/EntidadSeleccionableVista.cs`): datos lógicos del sprite (categoría, id, tipo, propietario, x/y, estado, orden, ~L21-38) + resaltado magenta (`Mostrar/OcultarSeleccion`, :103-117).
- **`BarraVidaVista`** (`Vistas/BarraVidaVista.cs`): barra flotante generada por código (`Configurar` :43, `Actualizar` :95: verde/amarillo/rojo según %; se oculta sin vida máxima).

### Controladores — puente sin hilos (`ImperiosEnGuerra.Controladores*`)
- **`ControladorSeleccion`** (`Controladores/ControladorSeleccion.cs`): selección por clic y captura de destino/recurso/entidad objetivo (eventos :26-38, `Iniciar/FinalizarCaptura*` :43-74, `LimpiarSeleccion` :281). No ejecuta gameplay.
- **`ControladorAcciones`** (`Controladores/ControladorAcciones.cs`): convierte selección + HUD en intenciones (`PrepararAccion` :508: Mover/Recolectar/Construir/Entrenar/Atacar/Batalla; `EnviarObjetivo` :318, `EnviarObjetivoAtaque` :402; validación vía `ReglasAcciones` :489). F5 guarda, F9 carga (`Update` :114).
- **`ControladorConexionApi`** (`Controladores/Red/ControladorConexionApi.cs`): el puente. Flags de workers (:40-48), `MoverUnidad/IniciarRecoleccion/Construir/Entrenar/Atacar/IniciarBatalla` (:273-417), espera con **polling 0,1 s** sin bloquear (`EsperarProcesoInterno` :646 + snapshots :705), sincronización (`SincronizarEstadoInterno` :734), costos (`DescribirCosto*` :1899-1906), `VigilarIA` (:2020: derrota cada 0,5 s, una vez) y arranque (`Start` :1998). Modo externo: HTTP contra `localhost:5086` (movimiento/recolección 0,1 s, entrenamiento 0,25 s, construcción 0,5 s); batalla/cancelar/F5-F9 solo internos.
- **`ApiInternaJuego`** (`Controladores/ApiInterna/ApiInternaJuego.cs`): API en memoria (singleton `DontDestroyOnLoad`, :27-41) que expone el Modelo sin terminal: `ObtenerEstado` (:130), `IniciarMovimiento/Recoleccion/Construccion/Entrenamiento/Ataque` (:140-196), `IniciarBatalla` (:201), `IntentarObtenerResultado / IntentarResultadoIA` (:209-258), `CancelarProceso` (:229), `Guardar/CargarProgreso` (:265-287, la carga relanza la IA), reglas delegadas (:289-325).
- **`AdaptadorEstadoPartida`** (`Controladores/ApiInterna/AdaptadorEstadoPartida.cs`): solo mapea la respuesta del Modelo a DTOs Unity (`Convertir` :14, con `vida/vidaMaxima` :63-89).
- **DTOs** (`Controladores/Red/Contratos/*.cs`): `EstadoPartidaDto` (estado, mapa, jugadores, economía; unidades y edificios con vida), `MoverUnidadDto`, `RecolectarDto`, `ConstruirDto`, `EntrenarDto`, `AtaqueDto`, `IniciarPartidaDto`, `ProcesoConcurrenteDto` (`ProcesoIniciadoDto`, `ResultadoProcesoDto` con `hiloTrabajoId/exito/mensaje/errorTecnico`).
- **Servidor REST** (`Controlador/ImperiosEnGuerra.Controlador/Program.cs`): modo externo opcional — `GET /api/estado`, `GET /api/modelo/prueba`, `POST /api/partida/iniciar`, `GET /api/partida`, `POST /api/partida/{mover,recolectar,construir,entrenar,atacar}[-concurrente]`, `GET /api/procesos/{id}/resultado`, `POST /api/procesos/{id}/cancelar`. Todo con `try-catch` y JSON.
