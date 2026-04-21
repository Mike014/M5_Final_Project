# Module 5 Final Project — Epicode Game Development

A stealth game with isometric view developed in Unity as the final project for Module 5 of the Epicode Master in Game Development.
The player must navigate a maze and reach the exit while avoiding guards, interacting with environmental elements, and using stun shots to clear a path.

---

## Technologies Used

- **Unity** (version with AI Navigation package)
- **C#**
- **Cinemachine** — isometric camera
- **NavMesh / NavMeshSurface** — AI navigation
- **ProBuilder** (optional) / Unity primitives for the map

---

## Implemented Features

### Player

- **Click-to-move** movement via NavMeshAgent and Raycast on the NavMesh
- "Ground" LayerMask to filter clicks on floor surfaces only
- **Stun shot** (Space key) — fires a projectile toward the clicked point
- Capture detection via `OnTriggerEnter` with the "Enemy" tag

### Camera

- **Isometric** view in the style of Diablo/Hades using a Cinemachine Virtual Camera
- Body: `Transposer` with `Binding Mode: World Space` — fixed absolute angle
- Follows the player while maintaining a constant offset

### Map

- Maze built with Unity primitives (Plane + Cube)
- **NavMesh baked** at runtime via `NavMeshSurface` to support dynamic doors

### Enemy System — FSM with Enum

All enemies share the base class `EnemyBase` with 5 states:

| State | Description |
|-------|-------------|
| `Idle` | Default behavior (rotation or patrolling) |
| `Chase` | Actively pursues the player |
| `Search` | Searches the area around the last known position |
| `Return` | Returns to original position/route |
| `Stunned` | Temporarily disabled by a stun shot |

**StationaryEnemy** — rotates 90° every X seconds, then returns to its initial rotation after resuming.  
**PatrolEnemy** — follows an array of waypoints in a loop, resuming from the last visited waypoint after returning.

#### Vision Cone

- Three sequential checks: range → angle → line of sight (Raycast against walls)
- Visualized in the Editor via Gizmos (color varies by state)

#### Global Alert

- When an enemy spots the player, it notifies all enemies within `_alertRadius` via `Physics.OverlapSphere`
- Alerted enemies transition directly to Chase

### Environment Interaction

- **Button** — proximity detection with `_interactionRange`, interaction on key E
- **Door** — moves along the Z axis via coroutine using `Vector3.MoveTowards`
- **Runtime NavMesh rebake** — `NavMeshSurface.BuildNavMesh()` called only after the door has reached its final position
- **Proximity UI** — World Space Canvas parented to the button, appears/disappears based on player distance

### Capture System

- **Respawn** — the player is teleported to the spawn point via `NavMeshAgent.Warp()`
- `GameController` implemented as a **Singleton** with `DontDestroyOnLoad`

### Main Menu

- Two buttons: **Start** (loads Level1) and **Exit Game**
- Scene management via `SceneManager`

---

## Extra Features

### Advanced FSM

- **Search** state: after losing the player, the enemy generates random points in the area via `NavMesh.SamplePosition` before returning to base
- **Global alert** with `_hasAlerted` flag to avoid redundant calls every frame

### Player Stun

- Physical projectile with kinematic `Rigidbody` and `Collision Detection: Continuous`
- Player/Projectile collision disabled via the **Physics Layer Collision Matrix** (engine-level solution, zero overhead)
- The enemy remembers its state prior to being stunned and resumes it once the stun ends

---

## Author

Michele Grimaldi — Master in Game Development, Epicode — Module 5
