# Pruebas de escritorio — Imperios en Guerra

Verificación manual del flujo y el comportamiento de las secciones críticas, con trazas paso a paso basadas en el código real y respaldadas por la suite NUnit (313/313 verdes). Valores del prototipo: Soldado 120 vida / 25 daño / alcance 1; Arquero 90 vida / 15 daño / alcance 4; Aldeano 50 vida / capacidad 10 (5 en estas pruebas); tasa de recolección 5; retardos 0,5 s (movimiento), 1 s (recolección y ataque), 7 s (construcción), 5 s × factor (entrenamiento); IA cada 2 s; polling de Vista 0,1 s; `VigilarIA` 0,5 s.

---

## T1. Concurrencia — dos aldeanos, un mismo nodo (sin duplicar saldo)

**Respaldo:** `CicloRecoleccionTests.DosAldeanos_MismoNodo_NoDuplicanSaldo` (verde).
**Precondiciones:** nodo Oro con 19 en (4,1); aldeanos A y B con capacidad 5; Castillo en (1,1).

| Paso | Hilo / actor | Acción | Estado del nodo | Saldo Oro |
|---|---|---|---|---|
| 1 | Worker A | `IntentarIniciarOrden(Recolectar)` → true; ocupa A | 19 | 0 |
| 2 | Worker B | `IntentarIniciarOrden(Recolectar)` → true (otra unidad); ocupa B | 19 | 0 |
| 3 | A | Extrae tasa 5 (`Recurso.Extraer` bajo lock) → carga 5/5 | 14 | 0 |
| 4 | B | Extrae tasa 5 → carga 5/5 | 9 | 0 |
| 5 | A | Va al Castillo, `DepositarCarga` (+5) | 9 | 5 |
| 6 | B | Va al Castillo, `DepositarCarga` (+5) | 9 | 10 |
| 7 | A | Extrae 5 → carga 5/5 | 4 | 10 |
| 8 | B | Extrae 4 (`Min(5,4)`) → carga 4/5, nodo **agotado** | 0 | 10 |
| 9 | Servicio | `RecolectarPaso` ve `RecursoAgotado` → `Mapa.RetirarRecurso((4,1))`: el nodo desaparece y la casilla queda libre | — (retirado) | 10 |
| 10 | A | Deposita +5 | — | 15 |
| 11 | B | Deposita +4 | — | **19** |

**Esperado:** saldo 19 (exacto, sin duplicar), cargas en 0, `ObtenerRecursoEn((4,1)) == null`, `PuedeColocar((4,1)) == true`.
**Obtenido:** el test lo confirma (verde). El orden exacto entre A y B puede variar; el resultado final es determinista gracias a los locks de `Recurso`, `Aldeano` y `RecursosJugador`.

---

## T2. Ataque por rondas y victoria humana

**Respaldo:** `AtaqueTests.*`, `OperacionAtaque` (`Modelo/.../Acciones/OperacionAtaque.cs:26-150`).
**Precondiciones:** Soldado humano junto (distancia 1) al último Soldado máquina (120 vida); la máquina ya perdió su Castillo.

| Ronda | Acción del worker `ATACAR` | Vida enemigo | Resultado parcial |
|---|---|---|---|
| 1 | Distancia 1 ≤ alcance 1 → espera 1 s → `Atacar()` 25 daño | 95 | `Impacto: 25 de daño a Soldado (vida 95).` |
| 2 | Golpea | 70 | Impacto (vida 70) |
| 3 | Golpea | 45 | Impacto (vida 45) |
| 4 | Golpea | 20 | Impacto (vida 20) |
| 5 | Golpea 25 → `RecibirDano` devuelve destruido → `EliminarUnidad` + `Liberar()` casilla | 0 (eliminado) | `EsVictoriaHumana` = sin Castillo **y** sin unidades → true |

**Esperado:** `Soldado enemigo destruido. ¡Victoria! La máquina perdió su Castillo y todas sus unidades.` + anuncio en HUD + `resultado_final.txt` con `Ganador=Griegos`.
**Obtenido:** flujo verificado en tests de ataque (verdes). Si quedaran unidades enemigas, el mensaje sería `…destruido. Quedan N enemigos.` (sin victoria: la condición exige castillo Y ejército).

---

## T3. Comunicación en red — orden REST concurrente (modo externo)

**Respaldo:** `Program.cs` (endpoints) + `ControladorConexionApi.EsperarResultadoMovimiento` (polling 0,1 s).
**Precondiciones:** servidor en `localhost:5086`, `usarApiExterna = true`.

| Paso | Emisor → Receptor | Mensaje | Respuesta |
|---|---|---|---|
| 1 | Unity → servidor | `POST /api/partida/mover-concurrente` `{"unidadId":"…","destino":{"x":4,"y":5}}` | `202 {"procesoId":"…","nombre":"MOVER","estado":"EnCurso"}` |
| 2 | Unity → servidor | `GET /api/procesos/{id}/resultado` (cada 0,1 s, sin bloquear el hilo principal) | `204` vacío (sigue en curso; la Vista refresca el snapshot intermedio) |
| 3 | Worker Modelo | Avanza la unidad paso a paso bajo `lock` | — |
| 4 | Unity → servidor | `GET /api/procesos/{id}/resultado` | `200 {"estado":"Completado","exito":true,"mensaje":"Movimiento realizado.","hiloTrabajoId":5}` |
| 5 | Unity | `GET /api/partida` → `Sincronizar` Vista + mensaje | Mapa actualizado en pantalla |

**Casos de error verificados:** timeout HTTP → aviso `HTTP {código}` en HUD y reintento a 0,5 s; `procesoId` ajeno → error y sincronización; worker `Fallido` → se muestra `errorTecnico`; funciones solo internas (batalla, cancelar, F5/F9) → mensaje que lo indica. Nada tumba el juego (todo con `try-catch`).

---

## T4. Condición de victoria de la máquina y anuncio de derrota

**Respaldo:** `InteligenciaMaquinaTests.Turno_EliminaUltimoHumano_DeclaraVictoriaMaquina` (verde) + `VigilarIA`.
**Precondiciones:** último Soldado humano (20 vida) junto a un Soldado máquina; turno de IA.

| Paso | Actor | Acción |
|---|---|---|
| 1 | `IniciarIA` (cada 2 s) | `EjecutarTurnoMaquina` → `InteligenciaMaquina.EjecutarTurno` |
| 2 | IA | `DefensaHumana` (sin aldeanos que mover) → `BuscarObjetivoCercano`: militar humano en radio 7 → `Golpear` 25 daño → 20 − 25 ≤ 0 → `EliminarUnidad` + `Liberar()` |
| 3 | IA | `EsVictoriaMaquina` (`Humano.Unidades.Count == 0`) → true → `¡Victoria! La máquina destruyó…` |
| 4 | Servicio | `GuardarResultadoFinalSeguro` → `resultado_final.txt`: `Ganador=Troya`, `Perdedor=Griegos`, `UnidadesRestantesMaquina=N` |
| 5 | `VigilarIA` (cada 0,5 s) | `IntentarResultadoIA` listo, mensaje con `¡Victoria!` → `derrotaAnunciada = true` → HUD: `Derrota: …` + estado sincronizado (una sola vez) |

**Esperado/obtenido:** derrota anunciada una vez, archivo final escrito, test verde.

---

## T5. Condición de carrera evitada — doble orden sobre la misma unidad

**Respaldo:** `Unidad.IntentarIniciarOrden` + `MotorAcciones` (rollback y `Cancelar`).
**Precondiciones:** un Aldeano libre; dos jugadores lógicos (o UI + IA) intentan ocuparlo a la vez.

| Paso | Hilo | Acción | Resultado |
|---|---|---|---|
| 1 | Worker 1 | `IntentarIniciarOrden(Mover)` | true — la unidad queda ocupada |
| 2 | Worker 2 | `IntentarIniciarOrden(Recolectar)` | **false** — la unidad no está disponible |
| 3 | Worker 2 | Retorna `Fallido("El Aldeano no está disponible.")` sin tocar mapa, carga ni saldos | Sin corrupción |
| 4 | Worker 1 | Completa y `CompletarOrden()` → unidad libre | Estado consistente |

**Esperado/obtenido:** la segunda orden se rechaza con mensaje claro; `CancelarOrdenesDe` + `SustituirOrden` permiten reasignar la unidad de forma ordenada (cancela, espera a lo sumo 3 s y lanza la nueva). Suite de cancelación y sustitución en verde.
