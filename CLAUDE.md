# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Settlers of Catan game engine** implemented as an Azure Functions v4 HTTP API in C# (.NET 8.0). It provides a complete game state management system with:
- RESTful API endpoints for all game actions
- Bot AI for autonomous players
- Undo/redo functionality
- Game state persistence to Azure Blob Storage
- Player authentication via token validation

## Build and Test Commands

```bash
# Build the project
dotnet build

# Run all tests
dotnet test

# Run a specific test
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run tests in a specific class
dotnet test --filter "FullyQualifiedName~GamePlayHelpersTests"

# Run the Azure Functions locally (requires Azurite for storage emulation)
func start
```

## Architecture

### Layer Structure

```
HTTP Request → Functions/Games.cs (Azure Function endpoints)
                      ↓
              Services/GameService.cs (orchestrator, persistence)
                      ↓
    ┌─────────────────┼─────────────────┐
    ↓                 ↓                 ↓
GamePlayHelpers    BotAI         PossiblePlayerActions
(game rules)    (AI decisions)   (action validation)
    ↓                 ↓                 ↓
              Models/GameState.cs
              (central state container)
                      ↓
              DTOs/ (serialization layer)
                      ↓
              Azure Blob Storage
```

### Key Components

- **Functions/Games.cs**: All HTTP endpoints. Each action (CreateGame, BuildRoad, etc.) is a separate Azure Function
- **Services/GameService.cs**: Main orchestrator handling game lifecycle and blob storage persistence
- **Services/GamePlayHelpers.cs**: Core game rules implementation (~1500 lines) - the largest and most critical file
- **Services/BotAI.cs** + **AIHelpers.cs**: Bot decision-making using heuristics
- **Services/PossiblePlayerActions.cs**: Validates legal moves for current game state
- **Services/BoardCreationHelpers.cs**: Board initialization with deterministic seeding
- **Services/Bank.cs**: Resource management and trading logic
- **Services/UndoHelpers.cs**: Undo/redo state management using a stack of snapshots

### Game State Flow

The game progresses through 14 distinct phases defined in `Models/GameStates.cs`:
1. Setup: SettingUpBoard → PlaceFirstSettlement → PlaceFirstRoad → PlaceSecondSettlement → PlaceSecondRoad
2. Main game loop: RollOrUseDevCard → BuildOrTrade (with interrupts for robber, discards, trades)
3. Special states: PlaceRobber, DiscardCards, RespondToTrade, SelectTarget
4. Development card roads: FirstDevCardRoad, SecondDevCardRoad
5. Terminal: GameOver

### API Response Pattern

All endpoints return `ResponseDTO` containing:
- `Success`: boolean
- `ErrorCode`: numeric code (see `DTOs/ResponseDTO.cs` for full mapping)
- `ErrorMessage`: human-readable message
- `GameState`: current game state (on success)
- `PossibleActions`: list of valid actions for the requesting player

### Model/DTO Dual Representation

Internal models (`Models/`) are converted to DTOs (`DTOs/`) for:
- Azure Blob Storage persistence
- API responses
- System.Text.Json serialization with custom `[JsonConstructor]` attributes

## Local Development Setup

The project uses Azure Storage emulator. Configure `local.settings.json`:
```json
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AZURE_STORAGE_CONNECTION_STRING": "UseDevelopmentStorage=true"
  }
}
```

Start Azurite before running locally.
