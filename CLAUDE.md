# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity package called "Tiny Character Controller" (TCC) - a modular character controller system for Unity 6000.0+. The project is structured as a Unity package located in `Assets/TinyCharacterController/` and includes several supporting modules.

## Architecture

### Core Module Structure
- **Foundation** (`Nitou.TCC.Foundation`): Base utilities, batch processing systems, body references, and movement references
- **Controller** (`Nitou.TCC.Controller`): Core character control logic including Brain components, managers (Move, Turn, Effect, Collision, Camera), and component interfaces
- **Inputs** (`Nitou.TCC.Inputs`): Input handling system with ActorBrain classes, input actions, and handlers for different input systems
- **Tools** (`Nitou.TCC.Tools`): Development tools including prototype model databases and creation menus

### Key Components
- **Brain System**: Modular brain components (CharacterBrain, ActorBrain, PlayerBrain, EnemyBrain) that control character behavior
- **Manager Components**: Specialized managers for movement, turning, effects, collisions, camera, and warping
- **Check Components**: Ground checking, head contact detection, sight checking, and range target detection
- **Effect System**: Gravity, additional velocity, and extra force effects

### Assembly Definitions
The project uses multiple assembly definitions for modular compilation:
- `Nitou.TCC.Foundation` - Base utilities
- `Nitou.TCC.Controller` - Core controller logic  
- `Nitou.TCC.Inputs` - Input handling
- `Nitou.TCC.Tools` - Development tools
- `Nitou.NGizmos` - Custom gizmo drawing system

## Development Commands

### Unity Development
- Open project in Unity 6000.0 or later
- Use Unity's built-in compilation (no external build commands needed)
- Project uses Visual Studio solution file (`lib-unity-TinyCharacterController.sln`)

### Package Installation
Install as Unity package via Package Manager:
```
https://github.com/nitou-kanazawa/lib-unity-TinyCharacterController.git?path=Assets/TinyCharacterController
```

## Dependencies

### External Packages
- UniTask (async/await for Unity)
- UniRx (reactive extensions)
- Unity Input System
- Sirenix Odin Inspector (for enhanced editor UI)
- Universal Render Pipeline

### Internal Dependencies
- Foundation layer provides base utilities for all other modules
- Controller depends on Foundation
- Inputs system integrates with Controller components

## Code Conventions

### Namespace Structure
- `Nitou.TCC.Controller` - Core controller functionality
- `Nitou.TCC.Inputs` - Input handling
- `Nitou.TCC.Utils` - Utility functions
- `Nitou.NGizmos` - Custom gizmo system

### Component Architecture
- Brain components inherit from `BrainBase` or `ActorBrain`
- Manager components handle specific aspects (movement, effects, etc.)
- Interface-driven design with clear separation of concerns
- Uses Sirenix Odin attributes for enhanced Inspector experience

### File Organization
- Scripts organized by functionality in clearly named folders
- Icons stored alongside relevant scripts
- Assembly definitions separate different functional areas
- Editor scripts separated from runtime scripts