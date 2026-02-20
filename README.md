# Progetto Fine Modulo 5 — Epicode Game Development

Un gioco stealth in visuale isometrica sviluppato in Unity come progetto conclusivo del Modulo 5 del Master in Game Development Epicode.

Il giocatore deve attraversare un labirinto ed raggiungere l'uscita evitando le guardie, interagendo con elementi dell'ambiente e utilizzando colpi stordenti per aprirsi un varco.

---

## Tecnologie utilizzate

- **Unity** (versione con AI Navigation package)
- **C#**
- **Cinemachine** — telecamera isometrica
- **NavMesh / NavMeshSurface** — navigazione AI
- **ProBuilder** (opzionale) / primitive Unity per la mappa

---

## Funzionalità implementate

### Player
- Movimento **click-to-move** tramite NavMeshAgent e Raycast sulla NavMesh
- LayerMask "Ground" per filtrare i click solo sul pavimento
- **Colpo stordente** (tasto Space) — spara un proiettile verso il punto cliccato
- Rilevamento cattura tramite `OnTriggerEnter` con tag "Enemy"

### Telecamera
- Visuale **isometrica** stile Diablo/Hades con Cinemachine Virtual Camera
- Body: `Transposer` con `Binding Mode: World Space` — angolo fisso assoluto
- Segue il player mantenendo offset costante

### Mappa
- Labirinto costruito con primitive Unity (Plane + Cube)
- **NavMesh baked** a runtime con `NavMeshSurface` per supportare porte dinamiche

### Sistema Nemici — FSM con Enum

Tutti i nemici condividono la classe base `EnemyBase` con 5 stati:

| Stato | Descrizione |
|-------|-------------|
| `Idle` | Comportamento di default (rotazione o patrolling) |
| `Chase` | Insegue il player attivamente |
| `Search` | Cerca nell'area dell'ultima posizione nota |
| `Return` | Torna alla posizione/percorso originale |
| `Stunned` | Bloccato temporaneamente dal colpo stordente |

**StationaryEnemy** — si gira di 90° ogni X secondi, poi torna alla rotazione iniziale dopo il rientro.

**PatrolEnemy** — segue un array di waypoints in loop, riprende dall'ultimo waypoint visitato dopo il rientro.

#### Cono di visione
- Tre check in sequenza: range → angolo → line of sight (Raycast verso i muri)
- Visualizzato nell'Editor tramite Gizmos (colore varia per stato)

#### Alert globale
- Quando un nemico vede il player, avvisa tutti i nemici nel raggio `_alertRadius` tramite `Physics.OverlapSphere`
- I nemici allertati passano direttamente in Chase

### Interazione Ambiente
- **Bottone** — rilevamento proximità con `_interactionRange`, interazione con tasto E
- **Porta** — si muove sull'asse Z tramite coroutine con `Vector3.MoveTowards`
- **NavMesh rebake a runtime** — `NavMeshSurface.BuildNavMesh()` chiamato solo dopo che la porta ha raggiunto la posizione finale
- **UI Proximity** — Canvas in World Space figlio del bottone, appare/sparisce in base alla distanza del player

### Sistema di cattura
- **Respawn** — il player viene teletrasportato al punto di spawn tramite `NavMeshAgent.Warp()`
- `GameController` implementato come **Singleton** con `DontDestroyOnLoad`

### Menu Principale
- Due bottoni: **Start** (carica Level1) e **Exit Game**
- Gestione scene tramite `SceneManager`

---

## Extra implementati

### FSM Avanzata
- Stato **Search**: dopo aver perso il player, il nemico genera punti casuali nell'area tramite `NavMesh.SamplePosition` prima di tornare alla base
- **Alert globale** con flag `_hasAlerted` per evitare chiamate ridondanti ogni frame

### Player Stun
- Proiettile fisico con `Rigidbody` kinematico e `Collision Detection: Continuous`
- Collisione Player/Proiettile disabilitata via **Physics Layer Collision Matrix** (soluzione engine-level, zero overhead)
- Il nemico ricorda lo stato precedente allo stun e vi torna al termine

---

## Autore

Michele Grimaldi — Master in Game Development, Epicode — Modulo 5