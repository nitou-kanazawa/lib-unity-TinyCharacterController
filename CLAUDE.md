# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity package called "Tiny Character Controller" (TCC) - a modular character controller system for Unity 6000.0+. The project is based on Unity's official [Project_TCC](https://github.com/unity3d-jp/Project_TCC) sample, customized as a reusable package.

TCC is a component-based character control system where character behavior is composed of small, specialized components:
- **Brain**: Aggregates results from Check, Effect, and Control components and writes final position/rotation to Transform
- **Check**: Sensor components that gather environmental information (ground detection, head contact, sight detection, etc.)
- **Control**: Components that handle character movement based on input (movement, jumping, camera control, etc.)
- **Effect**: Components that apply additional forces or influences (gravity, platform interaction, AddForce, etc.)

## Architecture Philosophy

TCC's architecture is built around **component composition over inheritance** with these core principles:

1. **Single Responsibility**: Each component handles one specific aspect (e.g., GroundCheck only detects ground, doesn't move character)
2. **Priority-based Competition**: Multiple movement/rotation sources compete via priority values; highest priority wins each frame
3. **Manager Pattern Aggregation**: Brain delegates to specialized managers instead of implementing logic directly
4. **Custom Update Timing**: PlayerLoop injection enables precise execution order independent of Unity's default MonoBehaviour lifecycle
5. **Separation of Concerns**: Brain implementations (CharacterBrain, RigidbodyBrain, etc.) only differ in HOW they apply movement, not WHAT movement to apply

This design allows mixing and matching components (e.g., add dash, climbing, swimming) without modifying existing code.

## Architecture

### Core Module Structure
- **Foundation** (`Nitou.TCC.Foundation`): Base utilities and shared systems (located in `Assets/TinyCharacterController/Foundation/`)
- **Controller** (`Nitou.TCC.Controller`): Core character control logic with Brain components and managers (`Assets/TinyCharacterController/Core/Controller/`)
- **Inputs** (`Nitou.TCC.Inputs`): Input handling with ActorBrain classes and input action handlers (`Assets/TinyCharacterController/Core/Inputs/`)
- **Implements** (`Nitou.TCC.Implements`): Concrete implementations of character behaviors (`Assets/TinyCharacterController/Implements/`)
- **Tools** (`Nitou.TCC.Tools`): Development tools and editor utilities (`Assets/TinyCharacterController/Tools/`)

### Brain System Architecture

The Brain system follows a strict hierarchy designed around separation of concerns:

**BrainBase** (abstract foundation):
- Orchestrates 7 internal managers: MoveManager, TurnManager, EffectManager, CameraManager, CollisionManager, WarpManager, UpdateComponentManager
- Maintains cached Position/Rotation state
- Calculates TotalVelocity = MoveVelocity + EffectVelocity
- Implements UpdateBrain() flow: component updates → velocity calculation → position/rotation application → camera processing
- Provides Warp system for teleportation (bypasses normal movement physics)

**Concrete Brain implementations**:
- **CharacterBrain**: Unity CharacterController-based (UpdateTiming.Update, supports FreezeAxis, push physics)
- **TransformBrain**: Direct Transform manipulation for simple movement
- **RigidbodyBrain**: Physics-based movement via Rigidbody
- **NavmeshBrain**: AI navigation using Unity's NavMesh system

**ActorBrain** (input layer, sits above BrainBase):
- Abstract base for input-driven characters (PlayerBrain, EnemyBrain)
- Manages ActorActions lifecycle (UpdateMode: FixedUpdate vs Update)
- Handles input timing and action reset logic

### Manager Pattern

Managers are internal classes used by BrainBase to organize functionality:
- **MoveManager**: Manages IMove components via priority system, calculates movement velocity
- **TurnManager**: Manages ITurn components, calculates rotation angles
- **EffectManager**: Manages IEffect components, accumulates effect velocities
- **CameraManager**: Processes camera-related components
- **CollisionManager**: Handles collision detection
- **WarpManager**: Handles teleportation/warp movement
- **UpdateComponentManager**: Coordinates IEarlyUpdateComponent execution

All managers use priority-based component selection via `TryGetHighestPriority()` extension method.

### Component Interfaces and Priority System

Key interfaces in `Nitou.TCC.Controller.Interfaces`:

**Priority-based Interfaces**:
- **IPriority<T>**: Base interface with `int Priority` property (Priority ≤ 0 means inactive)
- **IMove**: Provides `Vector3 MoveVelocity` for character movement
- **ITurn**: Provides rotation control with `int TurnSpeed` (negative = instant turn) and `float YawAngle`
- **IEffect**: Provides `Vector3 Velocity` for gravity/forces (non-priority, all effects accumulate)

**Lifecycle Interfaces**:
- **IPriorityLifecycle<T>**: Callbacks for priority state changes
  - `OnAcquireHighestPriority()`: Called when component becomes highest priority
  - `OnLoseHighestPriority()`: Called when component loses highest priority
  - `OnUpdateWithHighestPriority(float deltaTime)`: Called every frame while highest priority
- **IEarlyUpdateComponent**: Components that execute before Brain (e.g., position/rotation caching)

**Priority System Rules**:
- Only the component with highest Priority > 0 is active for IMove/ITurn
- Managers use `TryGetHighestPriority()` extension to select active component each frame
- Components can dynamically change priority (e.g., MoveControl sets priority based on input state)
- IEffect components don't use priority - all active effects are summed together

### Assembly Dependencies

Dependency flow (bottom to top):
```
Nitou.TCC.Foundation (base utilities)
    ↓
Nitou.TCC.Controller (references Foundation)
    ↓
Nitou.TCC.Inputs (references Controller + Foundation)
    ↓
Nitou.TCC.Implements (references Controller + Inputs + Foundation)
```

Controller assembly requires Odin Inspector (`defineConstraints: ["ODIN_INSPECTOR"]`).

### Input System Architecture

The Inputs module (`Assets/TinyCharacterController/Core/Inputs/Runtime/`) provides actor-based input handling:

**ActorActions System**:
- Action types: `BoolAction` (buttons), `FloatAction` (axes), `Vector2Action` (2D input)
- Recent features: Input buffering, recording/playback, state tracking for Float/Vector2
- ActorBrain manages lifecycle: `InitializeActions()`, `Reset()`, and `UpdateBrainValues()`

**Input Handlers**:
- **InputSystemHandler**: Uses Unity's new Input System (PlayerInput component)
- **InputModuleHandler**: Legacy input system support
- PlayerBrain delegates to appropriate handler via `SetValues()`

**Actor Types**:
- **PlayerBrain**: Human input via InputHandler
- **EnemyBrain**: AI-driven via EnemyBehaviour components
- Both share ActorActions interface for consistent control component integration

### Custom Packages

The project includes custom packages in `Packages/`:
- **jp.nitou.batchprocessor** (v0.9.0): Batch processing system for Unity update loop optimization via PlayerLoop injection
- **com.nitou.ngizmo** (v0.8.0): Custom gizmo drawing library used by TCC components (enabled via TCC_USE_NGIZMOS define)
- **jp.nitou.gameobjectpool**: GameObject pooling system (deprecated, consider using com.annulusgames.u-pools instead)
- **com.kybernetik.animancer**: Animation system (commercial plugin for advanced animation management)

### Experimental Modules

The project includes experimental systems in `Assets/` (not part of the main package):
- **GOAP** (`Assets/Goap/`): Goal-Oriented Action Planning AI system (uses R3.dll for reactive programming)
  - Provides GoapAgent for decision-making AI
  - Includes Belief system, Action strategies, and Sensor components
  - Currently in development/prototype phase
- **Demo** (`Assets/Demo/`): Example scenes demonstrating TCC features
  - SampleScene.unity: Basic character control demonstration
  - AI Behavior.unity: AI behavior examples
  - AI Navigation.unity: NavMesh-based AI navigation examples

## Development Commands

### Unity Development
- Open project in Unity 6000.0 or later
- Compilation is handled by Unity's assembly definition system
- Project uses Visual Studio solution file (`lib-unity-TinyCharacterController.sln`)

### Package Installation
Install main package via Unity Package Manager:
```
https://github.com/nitou-kanazawa/lib-unity-TinyCharacterController.git?path=Assets/TinyCharacterController
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "jp.nitou.tiny-character-controller": "https://github.com/nitou-kanazawa/lib-unity-TinyCharacterController.git?path=Assets/TinyCharacterController"
  }
}
```

**Note**: The package name is `jp.nitou.tiny-character-controller` (not `com.nitou.`)

## Dependencies

### Required External Packages
- **UniTask** (from GitHub): Async/await support for Unity
- **UniRx** (from GitHub): Reactive extensions for Unity
- **Unity Input System** (v1.14.2): New input system for player controls
- **Sirenix Odin Inspector**: Enhanced editor attributes (REQUIRED for Controller module - compilation will fail without it)
- **Universal Render Pipeline** (v17.2.0): Unity URP rendering

### Optional Packages
- **Unity AI Navigation** (v2.0.9): For NavMesh-based movement (required for NavmeshBrain)
- **Unity Timeline** (v1.8.9): For cutscenes and sequences
- **Unity Behavior** (v1.0.13): Visual scripting for AI behaviors
- **uPools** (from GitHub): Modern GameObject pooling alternative

## Code Conventions

### Namespace Structure
- `Nitou.TCC.Controller.Core`: Brain and manager implementations
- `Nitou.TCC.Controller.Interfaces`: Component interfaces (IMove, ITurn, IEffect, etc.)
- `Nitou.TCC.Controller.Shared`: Shared utilities and settings (includes Order.cs for update timing constants)
- `Nitou.TCC.Inputs`: Input handling and ActorBrain classes
- `Nitou.TCC.Implements`: Concrete component implementations
- `Nitou.TCC.Utils`: General utility functions
- `Nitou.BatchProcessor`: Custom update loop system
- `Nitou.NGizmos`: Gizmo drawing utilities
- `Nitou.Goap`: Experimental GOAP AI system (separate module)

### Component Design Patterns
1. **Priority-based execution**: Components implement priority values; highest priority wins
2. **Lifecycle callbacks**: Components can respond to priority changes via IPriorityLifecycle
3. **Manager aggregation**: Brain uses internal managers to organize component types
4. **Interface-driven**: Heavy use of interfaces (IMove, ITurn, IEffect) for flexibility
5. **Odin attributes**: Extensive use of Sirenix attributes for inspector customization ([TitleGroup], [Indent], [ReadOnly], etc.)

### File Organization
- Component implementations in `Components/` with subfolders (Brain/, Check/, Control/, Effect/)
- Interfaces in `Interfaces/` matching component types
- Managers in `Managers/` (internal classes used by Brain)
- Editor scripts separated from runtime scripts
- Icons stored in `Icons/` folders adjacent to scripts
- Assembly definitions (`.asmdef`) at module roots

### Update Timing System

The project uses a custom PlayerLoop injection system via GameControllerManager (from jp.nitou.batchprocessor):

**Update Order Constants** (see `Assets/TinyCharacterController/Core/Controller/Scripts/Shared/Order.cs`):
```
Early Update Phase:
  -1000: PrepareEarlyUpdate (position/rotation caching), GameObjectPool
  -101:  EarlyUpdateBrain execution
  -100:  IndicatorRegister

Update/FixedUpdate Phase:
  -200:  Check components (GroundCheck, sensors)
  -100:  Gravity effects
  -50:   Other Effect components
  5:     Control components (movement, jumping)
  10:    UpdateBrain (main Brain execution)
  50:    UpdateIK
  100:   PostUpdate
```

**Key Timing Mechanisms**:
- **GameControllerManager** injects custom update callbacks into Unity's PlayerLoop subsystems
- **EarlyUpdateBrainBase**: Separate component that caches Position/Rotation before Brain updates
- **ActorBrain UpdateMode**: Choose FixedUpdate (default, physics-synced) or Update (every frame) for input processing
  - FixedUpdate mode: Calls UpdateBrainValues() in FixedUpdate, resets actions in Update
  - Update mode: Both reset and UpdateBrainValues() happen in Update
- Components use `[UpdateComponent(Order.X)]` attribute to specify execution order

## Key Implementation Patterns

### Component Communication

**Direct Reference Pattern**:
- Components get Brain reference via `GetComponent<BrainBase>()` or `RequireComponent` attribute
- Access other components directly when needed (e.g., MoveControl accesses GroundCheck)

**Event-Based Pattern**:
- Gravity component uses UniRx Observables for state changes: `OnLanding`, `OnLeave`
- Provides fall velocity data in OnLanding event for damage/landing effects

**Manager Query Pattern**:
- Brain exposes manager state through properties (e.g., `HasMove`, `MoveVelocity`, `YawAngle`)
- Components query Brain for aggregated state rather than accessing other components directly

### Dynamic Priority Example

MoveControl demonstrates dynamic priority adjustment:
```csharp
// Priority changes based on state
int ITurn.Priority => IsMove || _isTurning ? _turnPriority : 0;
int IMove.Priority => IsMove ? _movePriority : 0;

// IsMove true when speed exceeds threshold
// _isTurning true while rotation is still changing
```

### Common Component Implementations

**Check Components** (Order: -200):
- GroundCheck: Dual-state detection (IsFirmlyOnGround vs IsOnGround for hysteresis)
- HeadContactCheck: Overhead collision detection
- RangeTargetCheck: Proximity-based target detection

**Control Components** (Order: 5):
- MoveControl: Implements both IMove + ITurn with acceleration/brake physics
- JumpControl: Jump mechanics with ground check integration
- Camera controls: CursorLookControl, StickLookControl for rotation

**Effect Components**:
- Gravity (Order: -100): State machine Air ↔ Ground with landing/leave events
- AdditionalVelocity (Order: -50): External force application
- Platform interaction: Moving platform tracking

## Testing and Validation

**Current Testing Approach**:
- No automated unit tests found in TCC codebase
- Manual testing via Demo scenes (`Assets/Demo/`)
- Visual validation using custom gizmos (NGizmo library integration via `#if TCC_USE_NGIZMOS`)

**When Making Changes**:
1. Test with Demo scenes to verify behavior
2. Use Scene view gizmos to visualize component state
3. Check console for any runtime errors
4. Verify assembly dependencies if modifying module structure

## Common Pitfalls and Best Practices

### Priority System Gotchas

**Problem**: Component not working even though attached
- **Solution**: Check if Priority > 0. Components with Priority ≤ 0 are considered inactive
- **Solution**: Verify no higher-priority component is overriding (only highest priority executes)

**Problem**: Movement jitters when stopping
- **Solution**: Use dynamic priority like MoveControl (keep turn priority active while rotating even after input stops)

### Update Timing Issues

**Problem**: Position values are one frame behind
- **Solution**: Use IEarlyUpdateComponent for components that need current frame's position before Brain updates
- **Solution**: Check execution order in Order.cs - sensors (-200) must run before controls (5)

**Problem**: Input feels delayed or unresponsive
- **Solution**: Verify ActorBrain.UpdateMode matches your physics settings (FixedUpdate for physics-based, Update for responsive controls)

### Assembly Dependency Rules

**CRITICAL**: Respect the dependency hierarchy:
- Foundation → Controller → Inputs → Implements
- Controller REQUIRES Odin Inspector (will fail compilation without it)
- Never reference Implements from Controller or Inputs (creates circular dependency)
- Use `#if ODIN_INSPECTOR` for optional Odin features

### Component Implementation Best Practices

1. **Implement IPriorityLifecycle when needed**: Use OnAcquireHighestPriority to initialize state when component activates
2. **Cache component references in Awake/Start**: Don't use GetComponent in Update/FixedUpdate
3. **Use MovementReference for input transformation**: Allows camera-relative or world-space movement easily
4. **Expose state via properties**: Allow other components to query your state without tight coupling
5. **Use UniRx for events**: Follow Gravity pattern for state change notifications (OnLanding, OnLeave)

### Warp System Usage

Use Brain.Warp() methods instead of direct Transform manipulation when:
- Teleporting character (maintains velocity/state consistency)
- Respawning after death
- Cutscene positioning

Never set Transform.position directly - it bypasses Brain's state management and can cause position desync.