# Guía de combate por fases — Imperios en Guerra

Objetivo: batallas estilo Age of Empires entre reinos, con botón de
batalla, guardado de progreso y victoria total.

## Estado actual del motor (ya implementado)

- Unidades: **Aldeano** (50 vida), **Soldado** (120 vida, 25 daño, alcance 1),
  **Arquero** (90 vida, 15 daño, alcance 4).
- Cualquier militar ataca cualquier objetivo enemigo: unidades (incluidos
  aldeanos) y edificios (Centro Urbano, 500 vida).
- El worker `ATACAR` camina solo hasta el alcance y golpea cada 1 s hasta
  destruir. El botón **¡Batalla!** ordena a todo el ejército a la vez, cada
  unidad en su propio hilo; entrenar y recolectar siguen en simultáneo.
- La **máquina** patrulla con 2 soldados y caza humanos en radio 7.
- Victoria total: destruir el Centro Urbano **Y** todas las unidades enemigas.
  Se guarda en `resultado_final.txt`.
- Progreso: **F5** guarda y **F9** carga `progreso.txt` (mapa, saldos, edificios,
  unidades con vida y carga, recursos restantes).

## Fase 1 — Batalla jugable (HECHA)

- [x] Botón ¡Batalla! con workers por unidad.
- [x] Ataque mutuo soldados↔soldados, soldados↔arqueros, arqueros↔castillo.
- [x] Victoria total (centro + ejército).
- [x] Guardado/carga en TXT.

## Fase 2 — Cinco reinos (SIGUIENTE)

La guía del curso menciona varios reinos enfrentados. Plan sin romper nada:

1. Nuevo `enum Reino { Griegos, Romanos, Egipcios, Vikingos, Persas }`
   en `Modelo/Core`, guardado en `Jugador.Reino`.
2. `ConfiguracionReino`: modificadores por reino (ej. Griegos +10 % ataque
   infantería, Egipcios +10 % recolección). Se aplican al crear unidades
   y al calcular tasas, sin tocar firmas existentes.
3. Selector de reino en el HUD antes de iniciar (1 botón + 5 opciones,
   igual que el selector de entrenamiento).
4. El humano juega con **Griegos**; la máquina rota de reino rival.
5. `progreso.txt` guarda el reino (`Reino=Griegos`).

## Fase 3 — Guerra prolongada (DESPUÉS)

1. Oleadas: la máquina entrena refuerzos cada N segundos si puede pagarlos
   (usar `EncolarEntrenamiento` + worker de espera ya existentes).
2. Asedio: ariete/balista como edificio móvil (nuevo `Edificio` + `Operacion`).
3. Contador de bajas y tiempo de partida en `resultado_final.txt`.
4. Mapa 20×20 con agua (casillas no transitables) usando `Tilemap_color2`.

## Reglas de validación por fase

- Cada fase cierra con `dotnet test` en verde y simulación de batalla
  (mover + atacar + victoria) antes de tocar la Vista.
- La Vista solo lee el Modelo; los hilos solo viven en
  `Modelo/Concurrencia` y `Modelo/Servicios`.
