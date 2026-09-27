# Diagramas — Imperios en Guerra

Diagramas en Mermaid (se ven en GitHub): arquitectura MVC, clases por capa y flujos de las secciones críticas.

---

## 1. Arquitectura MVC y comunicación

```mermaid
flowchart LR
    subgraph VISTA["VISTA (hilo principal Unity)"]
        VP[VistaPartida<br/>dibuja mapa y entidades]
        VH[VistaHud<br/>recursos, botones, mensajes]
        EV[EntidadSeleccionableVista<br/>datos lógicos del sprite]
        BV[BarraVidaVista<br/>vida flotante]
    end
    subgraph CONTROLADOR["CONTROLADOR (corutinas, sin hilos)"]
        CS[ControladorSeleccion<br/>clics y captura]
        CA[ControladorAcciones<br/>intenciones + F5/F9]
        CC[ControladorConexionApi<br/>puente + polling 0.1s]
        AI[ApiInternaJuego<br/>API en memoria]
        AD[AdaptadorEstadoPartida<br/>Modelo a DTO Unity]
    end
    subgraph MODELO["MODELO (lógica + todos los hilos)"]
        ES[EstadoPartidaService<br/>puerta thread-safe + bitácora]
        MO[MotorAcciones<br/>workers]
        TJ[TareasJuego<br/>Task.Run + colas]
        OP[Operacion* / Aproximacion*<br/>reglas de cada acción]
        IA[InteligenciaMaquina<br/>turno cada 2s]
        PE[(configuracion<br/>log_partida<br/>resultado_final<br/>progreso)]
    end
    subgraph RED["RED REST externa (alterna)"]
        SRV[Program.cs<br/>localhost:5086]
    end
    VP <--> CC
    VH <--> CA
    CS --> CA
    CA --> CC
    CC <--> AI
    AI <--> ES
    ES <--> MO
    MO <--> TJ
    MO --> OP
    MO --> IA
    ES --> PE
    CC <--> SRV
    AD -. mapea .-> CC
```

**Lectura:** la Vista solo habla con el Controlador; el Controlador solo habla con el Modelo (o el servidor REST); el Modelo nunca llama a la Vista. Los hilos existen únicamente dentro del Modelo.

---

## 2. Diagrama de clases — núcleo del dominio

```mermaid
classDiagram
    class Partida {
        +JugadorHumano
        +JugadorMaquina
    }
    class Jugador {
        +Nombre
        +Tipo
        +Mapa
        +Recursos
        +Unidades : List
        +Edificios : List
        +AgregarUnidad()
        +EliminarUnidad()
        +AgregarEdificio()
    }
    class Mapa {
        +Ancho = 15
        +Alto = 15
        +PuedeColocar()
        +ObtenerRecursoEn()
        +ColocarRecurso()
        +RetirarRecurso()
    }
    class Casilla {
        +EsTransitable
        +EstaOcupada
        +Ocupar()
        +Liberar()
    }
    class Coordenada {
        +X
        +Y
    }
    class Unidad {
        <<abstract>>
        +Id
        +Vida / VidaMaxima
        +PuntosAtaque / AlcanceAtaque
        +Estado / OrdenActiva
        +RecibirDano()
        +IntentarIniciarOrden()
    }
    class Aldeano {
        +CargaActual / CapacidadCarga
        +RecolectarDesde()
        +VaciarCarga()
    }
    class UnidadMilitar {
        <<abstract>>
    }
    class Soldado {
        +25 daño / alcance 1
    }
    class Arquero {
        +15 daño / alcance 4
    }
    class Edificio {
        <<abstract>>
        +Vida / VidaMaxima
        +RecibirDano()
    }
    class Castillo {
        +ColaEntrenamiento
        +EncolarEntrenamiento()
        +AvanzarEntrenamiento()
        +CompletarEntrenamiento()
    }
    class Recurso {
        +Tipo / CantidadRestante
        +Agotado
        +Extraer()
    }
    class RecursosJugador {
        +ObtenerCantidad()
        +IntentarGastar()
        +Reintegrar()
    }
    class InicializadorPartida {
        +Crear() : Partida
        +CentroHumano (13,7)
        +CentroMaquina (1,7)
    }
    Partida *-- Jugador
    Jugador *-- Mapa
    Jugador *-- Unidad
    Jugador *-- Edificio
    Mapa *-- Casilla
    Mapa *-- Recurso
    Unidad <|-- Aldeano
    Unidad <|-- UnidadMilitar
    UnidadMilitar <|-- Soldado
    UnidadMilitar <|-- Arquero
    Edificio <|-- Castillo
    Jugador *-- RecursosJugador
    InicializadorPartida ..> Partida : crea
    InicializadorPartida ..> Recurso : coloca
```

---

## 3. Diagrama de clases — concurrencia y servicios

```mermaid
classDiagram
    class EstadoPartidaService {
        +lock sincronizacion
        +MoverUnidad()
        +RecolectarPaso()
        +IniciarObra() / AvanzarObra()
        +EncolarEntrenamiento()
        +CompletarEntrenamientoConSpawn()
        +Atacar()
        +EjecutarTurnoMaquina()
        +GuardarProgreso() / CargarProgreso()
    }
    class MotorAcciones {
        +IniciarMovimiento()
        +IniciarRecoleccion()
        +IniciarConstruccion()
        +IniciarEntrenamiento()
        +IniciarAtaque() / IniciarBatalla()
        +IniciarIA()
    }
    class TareasJuego {
        +Iniciar() : Task.Run
        +Cancelar() / CancelarTodos()
        +IntentarObtenerResultado()
        +ConcurrentDictionary + ConcurrentQueue
    }
    class ProcesoConcurrente {
        +Id / Nombre / Finalizacion
    }
    class ResultadoProcesoConcurrente {
        +Estado : Completado/Cancelado/Fallido
        +Resultado / ErrorTecnico
    }
    class InteligenciaMaquina {
        +EjecutarTurno()
        -BuscarObjetivoCercano()
        -Golpear() / Acercarse()
        -RegresarAGuardia()
    }
    class DefensaHumana {
        +Ejecutar() : huida aldeanos
    }
    class OperacionAtaque {
        +Ejecutar()
        +EsVictoriaHumana()
        +EsVictoriaMaquina()
    }
    class ServicioArchivos {
        +GuardarConfiguracionInicial()
        +RegistrarEvento()
        +GuardarResultadoFinal()
        +GuardarProgreso() / LeerProgreso()
    }
    EstadoPartidaService --> MotorAcciones : usa
    MotorAcciones --> TareasJuego : lanza workers
    TareasJuego *-- ProcesoConcurrente
    TareasJuego *-- ResultadoProcesoConcurrente
    MotorAcciones --> InteligenciaMaquina : turno IA
    InteligenciaMaquina --> DefensaHumana : huidas
    InteligenciaMaquina --> OperacionAtaque : victoria
    EstadoPartidaService --> ServicioArchivos : archivos
```

---

## 4. Flujo general de una orden concurrente

```mermaid
flowchart TD
    A[Clic en UI<br/>Vista] --> B[ControladorAcciones<br/>valida con ReglasAcciones]
    B -->|rechazada| Z[Mensaje de error en HUD]
    B -->|válida| C[ControladorConexionApi<br/>lanza worker interno]
    C --> D[MotorAcciones<br/>reserva costo + inicia orden]
    D --> E[TareasJuego<br/>Task.Run en ThreadPool]
    E --> F[Worker: pasos con retardos<br/>todo bajo lock del servicio]
    F --> G{¿terminó?}
    G -->|no| H[Vista sondea cada 0.1s<br/>refresca snapshot]
    H --> F
    G -->|sí| I[Resultado en cola thread-safe]
    I --> J[Controlador lo recoge<br/>sincroniza Vista + mensaje]
    F -->|cancelado| K[Cancelar: rollback + reembolso]
    K --> J
```

---

## 5. Flujo de recolección (sección crítica)

```mermaid
flowchart TD
    A[IniciarRecoleccion] --> B[Depositar carga previa si hay]
    B --> C[Aproximar al nodo<br/>casilla libre adyacente]
    C --> D{¿capacidad llena<br/>o nodo agotado?}
    D -->|no| E[Esperar 1s<br/>RecolectarPaso: tasa a carga]
    E --> D
    D -->|sí| F[Ir al Castillo<br/>DepositarCarga al saldo]
    F --> G{¿nodo agotado?}
    G -->|sí| H[RetirarRecurso:<br/>casilla libre + sprite oculto]
    H --> Z[Fin: En espera de órdenes]
    G -->|no| C
```

---

## 6. Flujo de ataque y victoria

```mermaid
flowchart TD
    A[Atacar: atacante + objetivo] --> B{¿militar propia,<br/>objetivo enemigo,<br/>en alcance?}
    B -->|no| Z1[Rechazo con motivo en HUD]
    B -->|sí| C[Hasta 40 rondas:<br/>golpear cada 1s o acercarse]
    C --> D{¿destruido?}
    D -->|no| E[Impacto: daño a vida restante]
    D -->|sí| F[Eliminar + Liberar casilla]
    F --> G{¿sin Castillo Y sin<br/>unidades enemigas?}
    G -->|sí| H[¡Victoria! + anuncio HUD<br/>+ resultado_final.txt]
    G -->|no| I[Destruido. Quedan N enemigos]
```

---

## 7. Turno de la máquina (IA)

```mermaid
flowchart TD
    A[Cada 2s: EjecutarTurno] --> B[DefensaHumana:<br/>aldeanos huyen de militares a 3]
    B --> C[Por cada militar viva]
    C --> D{¿vida menor o igual a 1/4?}
    D -->|sí| E[RegresarAGuardia<br/>junto al Castillo]
    D -->|no| F[Buscar objetivo en radio 7:<br/>1.º militares, 2.º resto]
    F --> G{¿hay objetivo?}
    G -->|no| E
    G -->|sí| H{¿en alcance?}
    H -->|sí| I[Golpear: daño + bitácora]
    H -->|no| J[Avanzar 1 paso con A*]
    I --> K{¿humano sin unidades?}
    K -->|sí| L[¡Victoria máquina! + derrota en HUD]
    K -->|no| C
```

---

## 8. Comunicación REST externa (modo alterno)

```mermaid
sequenceDiagram
    participant U as Unity (hilo principal)
    participant S as Servidor REST :5086
    participant M as Modelo + workers
    U->>S: POST /api/partida/mover-concurrente {unidadId, destino}
    S->>M: IniciarMovimiento (worker)
    S-->>U: 202 {procesoId}
    loop cada 0.1s (corutina, sin bloquear)
        U->>S: GET /api/procesos/{id}/resultado
        S-->>U: 204 vacío (sigue) / 200 Completado
    end
    U->>S: GET /api/partida
    S-->>U: 200 estado JSON
    U->>U: Sincronizar Vista + mensaje
```
