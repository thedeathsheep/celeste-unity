# Celeste Unity Remake - Code Logic Documentation

This is a Unity-based remake of Celeste, focusing on demonstrating how to implement complex 2D platformer mechanics.

## Project Architecture Overview

### Core System Design
The project uses a modular design where each game mechanic has independent script components:

```
Assets/Scripts/
├── Player/                 # Player Core Systems
│   ├── PlayerMovement.cs   # Player Movement Controller (Core)
│   ├── DeathAndRespawn.cs  # Death and Respawn System
│   ├── UpdateAnimation.cs  # Animation State Management
│   └── PlayerCollectables.cs # Collectibles System
├── Platform Systems/       # Platform Systems
│   ├── Moving Platform/    # Moving Platforms
│   ├── Collapsing Platform/ # Collapsing Platforms
│   └── One Way Platform/   # One-Way Platforms
└── Game Systems/          # Game Systems
    ├── ScreenTransitionManager.cs # Screen Transitions
    ├── StrawberryCollect.cs      # Strawberry Collection
    └── CrystalActivation.cs      # Crystal Activation
```

## Core Code Logic Details

### 1. Player Movement System (`PlayerMovement.cs`)

#### Input Processing Mechanism
```csharp
// Custom input state enumeration
private enum KeyState { Off, Held, Up, Down }

// Input detection system
private KeyState UpdateKeyState(string keyName)
{
    return Input.GetButton(keyName) ? KeyState.Held : KeyState.Off;
}
```

#### State Management
- **Ground State Detection**: Uses Physics2D.BoxCast to detect ground contact
- **Air Control**: Air movement has additional inertia system
- **Dash System**: Includes dash count limits and directional control
- **Climbing System**: Stamina consumption and wall grabbing logic

#### Physics System
```csharp
// Movement speed control
if (IsGrounded()) {
    rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
} else {
    // Air movement has inertia limitations
    float horizontalVelocity = rb.velocity.x + dirX * moveSpeed / 8;
    rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
}
```

### 2. Death and Respawn System (`DeathAndRespawn.cs`)

#### Death Detection
- **Squish Detection**: Triggers death when player is crushed by walls
- **Death Animation**: Particle effects and visual feedback
- **State Reset**: Resets all player states upon respawn

#### Respawn Logic
```csharp
// Automatically find nearest checkpoint
spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
respawnPosition = Nearest(spawnPoints);

// Reset states upon respawn
transform.position = respawnPosition;
GetComponent<PlayerMovement>().ResetDashAndGrab();
```

### 3. Screen Transition System (`ScreenTransitionManager.cs`)

#### Camera Switching Mechanism
- **Virtual Camera System**: Uses Cinemachine for smooth camera transitions
- **Trigger Detection**: Activates new camera when player enters specific areas
- **State Preservation**: Ensures player state consistency during screen transitions

#### Transition Logic
```csharp
private void OnTriggerEnter2D(Collider2D coll)
{
    if (virtualCamera.activeInHierarchy == false && coll.CompareTag("Player")) {
        virtualCamera.SetActive(true);
        screenManager.currentCamera = virtualCamera;
        player.GetComponent<StopObject>().Stop(0.4f, upperTransition, virtualCamera);
    }
}
```

### 4. Platform System Design

#### Moving Platforms (`Moving Platform/`)
- **Path System**: Supports various movement paths (linear, circular, custom)
- **Speed Control**: Adjustable movement speed and acceleration
- **Player Interaction**: Platform movement affects player velocity

#### Collapsing Platforms (`Collapsing Platform/`)
- **Trigger Mechanism**: Starts collapse countdown when player steps on
- **Visual Feedback**: Animation effects during collapse process
- **Reset System**: Platform automatically resets after player death

#### One-Way Platforms (`One Way Platform/`)
- **Collision Detection**: Only allows passage from below
- **Jump Through**: Allows platform penetration when jump key is pressed

### 5. Collectibles System

#### Strawberry Collection (`StrawberryCollect.cs`)
- **Collection Detection**: Collision detection and collection animation
- **State Persistence**: Collectible state persistence
- **Visual Effects**: Particle effects during collection

#### Winged Strawberries (`WingedStrawberry.cs`)
- **AI Behavior**: Automatic flight and player avoidance
- **State Management**: Flight, collection, reset states
- **Difficulty Adjustment**: Adjustable flight speed and reaction time

## Technical Implementation Details

### Physics System
- **Rigidbody2D**: Used for player physics simulation
- **BoxCollider2D**: Precise collision detection
- **LayerMask**: Layered collision system

### Animation System
- **Animator Controller**: State machine management
- **Animation Events**: Keyframe-triggered game logic
- **Blend Animations**: Smooth animation transitions

### Performance Optimization
- **Object Pooling**: Reuse frequently created objects (e.g., particle effects)
- **Event System**: Reduce unnecessary Update calls
- **Cached References**: Avoid frequent GetComponent calls

## Code Design Patterns

### 1. Component Pattern
Each functionality is an independent MonoBehaviour component, facilitating maintenance and extension.

### 2. State Machine Pattern
Player states are managed through enums and state machines, ensuring consistent state transitions.

### 3. Observer Pattern
Uses Unity's event system for loose coupling between components.

### 4. Factory Pattern
Used for creating and managing game objects (e.g., particle effects, collectibles).

## Extensibility Design

### Adding New Mechanics
- **New Platform Types**: Inherit from base platform class and implement specific behaviors
- **New Collectibles**: Implement ICollectable interface
- **New Movement Abilities**: Add new movement states in PlayerMovement

### Configuration System
- **ScriptableObject**: Game data configuration
- **Inspector Parameters**: Runtime-adjustable game parameters
- **Save System**: Player progress and settings persistence

## Development Guidelines

### Code Standards
- Use meaningful variable and method names
- Add appropriate comments for complex logic
- Follow Unity naming conventions

### Debugging Tools
- Use Debug.Log for key state information output
- Display colliders and trigger areas in Scene view
- Use Unity Profiler for performance monitoring

### Testing Strategy
- Unit test critical game logic
- Integration test player-environment interactions
- Performance test for smooth operation

## Project Status

✅ **Completed**
- Core movement system
- Death and respawn mechanism
- Screen transition system
- Basic platform system
- Collectibles system

🔄 **In Progress**
- Level design optimization
- Performance optimization
- Code refactoring

📋 **Planned**
- Audio system
- UI system
- Save system
- Achievement system

---

*This project is for learning and research purposes only. The original concept, sounds, and images of Celeste belong to their respective owners.*
