# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Settlers of Catan game engine** implemented as an Azure Functions v4 HTTP API in C# (.NET 8.0). It provides a complete game state management system with:
- RESTful API endpoints for all game actions
- Bot AI for autonomous players
- Undo/redo functionality
- Game state persistence to Azure Blob Storage
- Player authentication via token validation

## Development Philosophy

- **Learning project with production aspirations** - build quality code, but don't over-engineer
- **Cost-conscious** - keep Azure hosting free/near-free (~$0.10/month currently); avoid services that increase costs
- **Agile approach** - build only what's needed for the current iteration; keep flexibility to pivot
- No premature abstractions or "just in case" features

## Related Projects

The primary frontend is **GT_PlayBack** located at `C:\Users\mbilo\Documents\src\GT_PlayBack`:
- Azure Static Web App with vanilla JavaScript
- `index.html` - main game UI
- `replay.html` - dev/support tool to review finished games (must not impact main game performance)
- See its CLAUDE.md for full API curl examples and sample ResponseDTO

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

### API Endpoints

RESTful patterns under `/api/Games`:
- `POST /api/Games` - CreateGame
- `POST /api/Games/{gameId}` - GetGame (with playerId in body)
- `POST /api/Games/{gameId}/players` - AddPlayer
- `POST /api/Games/{gameId}/start` - StartGame
- `POST /api/Games/{gameId}/build/road` - BuildRoad
- `POST /api/Games/{gameId}/build/settlement` - BuildSettlement
- `POST /api/Games/{gameId}/build/city` - BuildCity
- `POST /api/Games/{gameId}/roll` - RollDice
- `POST /api/Games/{gameId}/end-turn` - EndTurn
- `POST /api/Games/{gameId}/trades/bank` - TradeWithBank
- `POST /api/Games/{gameId}/trades/open` - OpenTrade
- `POST /api/Games/{gameId}/trades/respond` - RespondToTrade
- `POST /api/Games/{gameId}/trades/accept` - AcceptTrade
- `POST /api/Games/{gameId}/dev-card/buy` - BuyDevCard
- `POST /api/Games/{gameId}/dev-card/play` - PlayDevCard
- `POST /api/Games/{gameId}/place-robber` - PlaceRobber
- `POST /api/Games/{gameId}/select-target` - SelectTarget
- `POST /api/Games/{gameId}/discard-cards` - DiscardCards
- `POST /api/Games/{gameId}/undo` - Undo

### API Response Pattern

All endpoints return `ResponseDTO` containing:
- `Success`: boolean
- `ErrorCode`: numeric code (see `DTOs/ResponseDTO.cs` for full mapping of ~50 error codes)
- `ErrorMessage`: human-readable message
- `GameState`: current game state (on success)
- `PossibleActions`: list of valid actions for the requesting player

**The `PossibleActions` field is critical** - it drives the frontend UI by indicating which actions are valid for the current player based on game phase, board state, and available resources/dev cards. This is computed by `Services/PossiblePlayerActions.cs`.

### Model/DTO Dual Representation

Internal models (`Models/`) are converted to DTOs (`DTOs/`) for:
- Azure Blob Storage persistence
- API responses
- System.Text.Json serialization with custom `[JsonConstructor]` attributes

### Resource Types

Five resource types: Wood, Brick, Wool, Grain, Ore (plus Desert for tiles). The frontend displays these as colored blocks matching the board colors.

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

## Known Bot AI Limitations

Current areas for improvement in `Services/BotAI.cs`:
- Bots don't buy or use development cards
- Bots don't respond to player trades
- Bots don't value longest road as an objective

## Testing Philosophy

- TDD approach: reproduce bugs with tests before fixing
- Goal is to cover all production gameplay scenarios
- No specific code coverage target
- Known gap: Functions and Blob storage integration are harder to test

## Multiplayer Coordination

Polling-based with progressive backoff:
1. Fast phase: 2-second intervals
2. Slow phase: 4-second intervals
3. Dialog prompts user after extended inactivity - polling pauses
4. User responds → returns to fast phase

No WebSockets or real-time push - clients periodically fetch game state.
