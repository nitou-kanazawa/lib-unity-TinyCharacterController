# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity package called "Tiny Character Controller" (TCC) - a modular character controller system for Unity 6000.0+. The project is based on Unity's official [Project_TCC](https://github.com/unity3d-jp/Project_TCC) sample, customized as a reusable package.

TCC is a component-based character control system where character behavior is composed of small, specialized components:
- **Brain**: Aggregates results from Check, Effect, and Control components and writes final position/rotation to Transform
- **Check**: Sensor components that gather environmental information (ground detection, head contact, sight detection, etc.)
- **Control**: Components that handle character movement based on input (movement, jumping, camera control, etc.)
- **Effect**: Components that apply additional forces or influences (gravity, platform interaction, AddForce, etc.)

## Architecture

### Core Module Structure
- **Foundation** (`Nitou.TCC.Foundation`): Base utilities and shared systems (located in `Assets/TinyCharacterController/Foundation/`)
- **Controller** (`Nitou.TCC.Controller`): Core character control logic with Brain components and managers (`Assets/TinyCharacterController/Core/Controller/`)
- **Inputs** (`Nitou.TCC.Inputs`): Input handling with ActorBrain classes and input action handlers (`Assets/TinyCharacterController/Core/Inputs/`)
- **Implements** (`Nitou.TCC.Implements`): Concrete implementations of character behaviors (`Assets/TinyCharacterController/Implements/`)
- **Tools** (`Nitou.TCC.Tools`): Development tools and editor utilities (`Assets/TinyCharacterController/Tools/`)

### Brain System Architecture

The Brain system follows a strict component hierarchy:
1. **BrainBase** (abstract): Core brain that orchestrates all managers (MoveManager, TurnManager, EffectManager, CameraManager, CollisionManager, WarpManager, UpdateComponentManager)
2. **ActorBrain** (abstract): Handles input processing and ActorActions lifecycle (Update/FixedUpdate mode selection)
3. **Concrete Brains**: PlayerBrain, EnemyBrain for specific actor types

Key Brain responsibilities:
- Position = current cached position, Rotation = current cached rotation
- TotalVelocity = MoveVelocity + EffectVelocity
- UpdateBrain() orchestrates: component updates → velocity calculation → position/rotation application → camera processing
- Warp methods bypass normal movement and directly set position/rotation

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

### Component Interfaces

Key interfaces in `Nitou.TCC.Controller.Interfaces.Components`:
- **IMove**: Provides MoveVelocity for character movement
- **ITurn**: Provides rotation control with TurnSpeed and YawAngle
- **IEffect**: Provides EffectVelocity for gravity, forces, etc.
- **IPriorityLifecycle<T>**: Callbacks when component gains/loses highest priority
- **IEarlyUpdateComponent**: Components that need pre-Brain updates

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

### Custom Packages

The project includes custom packages in `Packages/`:
- **jp.nitou.batchprocessor** (v0.9.0): Batch processing system for Unity update loop optimization
- **jp.nitou.ngizmo** (v0.8.0): Custom gizmo drawing library used by TCC components
- **jp.nitou.gameobjectpool** (v0.9.0): GameObject pooling system

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
    "com.nitou.tiny-character-controller": "https://github.com/nitou-kanazawa/lib-unity-TinyCharacterController.git?path=Assets/TinyCharacterController"
  }
}
```

## Dependencies

### Required External Packages
- **UniTask**: Async/await support for Unity (from GitHub)
- **UniRx**: Reactive extensions (from GitHub)
- **Unity Input System** (v1.14.2): New input system for player controls
- **Sirenix Odin Inspector**: Enhanced editor attributes (REQUIRED for Controller module)
- **Universal Render Pipeline** (v17.2.0): Unity URP

### Optional Packages
- **Unity AI Navigation** (v2.0.9): For NavMesh-based movement
- **Unity Timeline** (v1.8.9): For cutscenes and sequences

## Code Conventions

### Namespace Structure
- `Nitou.TCC.Controller.Core`: Brain and manager implementations
- `Nitou.TCC.Controller.Interfaces`: Component interfaces (IMove, ITurn, IEffect, etc.)
- `Nitou.TCC.Controller.Shared`: Shared utilities and settings
- `Nitou.TCC.Inputs`: Input handling and ActorBrain classes
- `Nitou.TCC.Utils`: General utility functions
- `Nitou.BatchProcessor`: Custom update loop system
- `Nitou.NGizmos`: Gizmo drawing utilities

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

### Update Timing
- **ActorBrain** offers UpdateMode choice (FixedUpdate or Update) for input processing
- **BrainBase** abstracts update timing via UpdateTiming property
- Components implement IEarlyUpdateComponent for pre-Brain position/rotation caching