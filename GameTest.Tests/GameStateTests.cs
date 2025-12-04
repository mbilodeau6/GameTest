using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;

namespace GameTest.Tests;

public class GameStateTests
{
    [Fact]
    public void Constructor_DefaultNoGameType_EmptyState()
    {
        // Act
        var game = new GameState(Guid.NewGuid());

        // Assert
        Assert.Empty(game.Players);
        Assert.Empty(game.Tiles);
        Assert.Empty(game.Edges);
        Assert.Empty(game.Vertices);
        Assert.Empty(game.Ports);
        Assert.Null(game.RobberTile);
        Assert.Null(game.PlayerWithLongestRoad);
        Assert.Null(game.PlayerWithLargestArmy);
        Assert.All(game.Resources.Values, v => Assert.Equal(19, v));
        Assert.Equal(25, game.DevelopmentCards.Count);
        Assert.Equal(2, game.DevelopmentCards.Count(dc => dc == DevelopmentCardType.Monopoly));
        Assert.Equal(2, game.DevelopmentCards.Count(dc => dc == DevelopmentCardType.RoadBuilding));
        Assert.Equal(2, game.DevelopmentCards.Count(dc => dc == DevelopmentCardType.YearOfPlenty));
        Assert.Equal(14, game.DevelopmentCards.Count(dc => dc == DevelopmentCardType.Knight));
        Assert.Equal(5, game.DevelopmentCards.Count(dc => dc == DevelopmentCardType.VictoryPoint));
        Assert.NotNull(game.Phase);
        Assert.Null(game.Phase.CurrentPlayer);
        Assert.Equal(GameStates.SettingUpBoard, game.Phase.PhaseState);
        Assert.Null(game.Phase.EndPlayer);
        Assert.Equal(GameType.Default, game.Settings.Type);
        Assert.Equal(10, game.Settings.VictoryPointsToWin);
        Assert.Equal(10, game.Settings.VictoryPointsToWin);
        Assert.True(game.Dice.Die1.Random);
    }

    [Fact]
    public void Constructor_DefaultWithGameType_EmptyState()
    {
        // Act
        var game = new GameState(Guid.NewGuid(), GameType.Starter);

        // Assert
        Assert.Empty(game.Players);
        Assert.Empty(game.Tiles);
        Assert.Equal(GameType.Starter, game.Settings.Type);
        Assert.Equal(GameStates.SettingUpBoard, game.Phase.PhaseState);
    }

    [Fact]
    public void Constructor_FromDTO_CreatesEmptyObject()
    {
        // Arrange
        var dto = new GameStateDTO(Guid.NewGuid().ToString(), new GameSettingsDTO(new GameSettings()));

        // Act
        var game = new GameState(dto);

        // Assert
        Assert.Equal(dto.Id, game.Id.ToString());
        Assert.Equal(dto.Settings.Type, game.Settings.Type.ToString());

        Assert.Empty(game.Players);
        Assert.Empty(game.Tiles);
        Assert.Empty(game.Edges);
        Assert.Empty(game.Vertices);
        Assert.Empty(game.Ports);
    }

    [Fact]
    public void Constructor_FromDTO_NoPlayersForPhaseState()
    {
        // Arrange
        var gs = new GameState(new Guid());
        gs.Phase = new GamePhase(GameStates.BuildOrTrade);
        var dto = new GameStateDTO(gs);

        // Act
        var game = new GameState(dto);

        // Assert
        Assert.Equal(dto.Id, game.Id.ToString());
        Assert.Equal(dto.Settings.Type, game.Settings.Type.ToString());
        Assert.Equal(gs.Phase.PhaseState, game.Phase.PhaseState);
        Assert.Null(game.Phase.CurrentPlayer);
        Assert.Null(game.Phase.EndPlayer);
    }

    [Fact]
    public void Constructor_FromDTO_CreatesValidObject()
    {
        // Arrange
        var gs = new GameState(Guid.NewGuid(), GameType.Test);
        var player1 = new Player("Alice", PlayerColor.Blue);
        gs.AddPlayer(player1);
        var player2 = new Player("Bob", PlayerColor.Red);
        gs.AddPlayer(player2);

        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        gs.AddTile(tile1);
        var tile2 = new Tile(ResourceType.Desert, 0, 2, 0);
        gs.AddTile(tile2);
        gs.AddEdge(new Edge(tile1, HexDirection.NE));
        gs.Edges[0].BuildRoad(player2);

        var vertex1 = new Vertex(tile1, tile2); 
        vertex1.BuildSettlement(player1);
        gs.AddVertex(vertex1);
        var vertex2 = new Vertex(tile2, VertexDirection.N);
        gs.AddVertex(vertex2);
        
        gs.SetRobberTile(tile1);
        gs.Phase = new GamePhase(GameStates.SettingUpBoard, player1, player2);

        gs.Ports.Add(new Port(vertex1, vertex2, PortType.ThreeToOne));

        var dto = new GameStateDTO(gs);

        // Act
        var game = new GameState(dto);

        // Assert
        Assert.Equal(dto.Id, game.Id.ToString());
        Assert.Equal(dto.Settings.Type, game.Settings.Type.ToString());
        Assert.Equal(2, game.Settings.MaxPlayers);
        Assert.Equal(5, game.Settings.VictoryPointsToWin);
        Assert.Equal(6, game.Settings.RoadsPerPlayer);
        Assert.Equal(3, game.Settings.SettlementsPerPlayer);
        Assert.Equal(2, game.Settings.CitiesPerPlayer);
        Assert.Equal(2, game.Players.Count);
        Assert.Equal(player2.Name, game.Players[1].Name);
        Assert.Equal(player1.Color, game.Players[0].Color);
        Assert.Equal(2, game.Tiles.Count);
        Assert.Equal(ResourceType.Brick, game.Tiles[0].Resource);
        Assert.Equal(8, game.Tiles[0].DiceNumber);
        Assert.Single(game.Edges);
        Assert.NotNull(game.Edges[0].Owner);
        Assert.Equal(player2.Id, game.Edges[0].Owner.Id);
        Assert.Equal(2, game.Vertices.Count);
        Assert.Equal(BuildingType.Settlement, game.Vertices[0].Building);
        Assert.Equal(gs.Phase.PhaseState, game.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.NotNull(game.Phase.CurrentPlayer);
        Assert.Equal(gs.Phase.CurrentPlayer.Id, game.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.NotNull(game.Phase.EndPlayer);
        Assert.Equal(gs.Phase.EndPlayer.Id, game.Phase.EndPlayer.Id);
        Assert.False(gs.Dice.Die1.Random);
        Assert.False(gs.Dice.Die2.Random);
        Assert.Single(game.Ports);
        Assert.Equal(PortType.ThreeToOne, game.Ports[0].Type);
        Assert.Null(game.PlayerWithLargestArmy);
        Assert.Null(game.PlayerWithLongestRoad);
    }

    [Fact]
    public void PlaceRobberOnDesert_OneDesert()
    {
        // Arrange
        var desertTile = new Tile(ResourceType.Desert, 0, 0, 0);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(desertTile);

        // Act
        game.PlaceRobberOnDesert();

        // Assert
        Assert.Equal(desertTile, game.RobberTile);
    }

    [Fact]
    public void PlaceRobberOnDesert_TwoDeserts()
    {
        // Arrange
        var desertTile1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var desertTile2 = new Tile(ResourceType.Desert, 0, 1, -1);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(desertTile1);
        game.AddTile(desertTile2);

        // Assert
        game.PlaceRobberOnDesert();

        Assert.True(game.RobberTile == desertTile1 || game.RobberTile == desertTile2);
    }

    [Fact]
    public void PlaceRobberOnDesert_NoDesert_ThrowsInvalidOperationException()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(tile);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => game.PlaceRobberOnDesert());
        Assert.Equal("No desert tile found in the game.", exception.Message);
    }

    [Fact]
    public void AddPlayer_AddSinglePlayer()
    {
        // Arrange
        string expectedName = "Alice";

        // Act
        var game = new GameState(Guid.NewGuid());
        game.AddPlayer(new Player(expectedName, PlayerColor.Blue));

        // Assert
        Assert.NotEmpty(game.Players);
        Assert.Equal(expectedName, game.Players[0].Name);
    }

    [Fact]
    public void AddTile_AddSingleTile()
    {
        // Arrange
        ResourceType expectedResourceType = ResourceType.Wool;

        // Act
        var game = new GameState(Guid.NewGuid());
        game.AddTile(new Tile(expectedResourceType, 5, 0, 0));

        // Assert
        Assert.NotEmpty(game.Tiles);
        Assert.Equal(expectedResourceType, game.Tiles[0].Resource);
    }

    [Fact]
    public void AddEdge_AddSingleEdge()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var edge = new Edge(tile, HexDirection.NE);
        var expectedEdgeId = edge.Id;

        // Act
        var game = new GameState(Guid.NewGuid());
        game.AddEdge(edge);

        // Assert
        Assert.NotEmpty(game.Edges);
        Assert.Equal(expectedEdgeId, game.Edges[0].Id);
    }


    [Fact]
    public void AddVertex_AddSingleVertex()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(tile, VertexDirection.N);
        var expectedVertexId = vertex.Id;

        // Act
        var game = new GameState(Guid.NewGuid());
        game.AddVertex(vertex);

        // Assert
        Assert.NotEmpty(game.Vertices);
        Assert.Equal(expectedVertexId, game.Vertices[0].Id);
    }

    [Fact]
    public void SetRobberTile_ValidLocation()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(tile);

        // Act
        game.SetRobberTile(tile);

        // Assert
        Assert.Equal(tile, game.RobberTile);
    }

    [Fact]
    public void SetRobberTile_NonexistentTile()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(tile);

        var tileNotOnBoard = new Tile(ResourceType.Wood, 5, 1, -1);

        // Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            game.SetRobberTile(tileNotOnBoard));

        Assert.Equal("The specified tile does not exist in the game.", exception.Message);
    }

    [Fact]
    public void SetRobberTileId_AlreadySet()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(tile);
        game.SetRobberTile(tile);

        // Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            game.SetRobberTile(tile));

        Assert.Equal("Robber is already on the specified tile.", exception.Message);
    }

    [Fact]
    public void SetDiceForTesting_ChangeDice()
    {
        // Arrange
        var game = new GameState(Guid.NewGuid());
        var newDice = new GameDice(new GameDie(1), new GameDie(1));

        // Act
        game.SetDiceForTesting(newDice);

        // Assert
        Assert.Equal(1, game.Dice.Die1.Value); 
        Assert.Equal(1, game.Dice.Die2.Value); 
    }

    [Fact]
    public void AssignLongestRoadToPlayer()
    {
        // Arrage
        var game = new GameState(Guid.NewGuid());
        var p1 = new Player("Ann", PlayerColor.Red);
        game.AddPlayer(p1);
        var p2 = new Player("Tim", PlayerColor.Blue);
        game.AddPlayer(p2);

        // Act
        game.AssignLongestRoadToPlayer(p2);

        // Assert
        Assert.NotNull(game.PlayerWithLongestRoad);
        Assert.Equal(p2.Id, game.PlayerWithLongestRoad.Id);
    }

    [Fact]
    public void AssignLargestArmyToPlayer()
    {
        // Arrage
        var game = new GameState(Guid.NewGuid());
        var p1 = new Player("Ann", PlayerColor.Red);
        game.AddPlayer(p1);
        var p2 = new Player("Tim", PlayerColor.Blue);
        game.AddPlayer(p2);

        // Act
        game.AssignLargestArmyToPlayer(p1);

        // Assert
        Assert.NotNull(game.PlayerWithLargestArmy);
        Assert.Equal(p1.Id, game.PlayerWithLargestArmy.Id);
    }


    private static GameState CreateTestGameWithManyRoads()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        var player3 = new Player("Alex", PlayerColor.Orange);
        gs.AddPlayer(player3);
        gs.Edges[0].BuildRoad(gs.Players[0]);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        gs.Vertices[1].BuildSettlement(gs.Players[1]);
        gs.Vertices[2].BuildSettlement(gs.Players[1]);
        gs.Vertices[3].BuildSettlement(gs.Players[1]);
        gs.Vertices[4].BuildSettlement(gs.Players[1]);
        gs.Vertices[4].UpgradeToCity();
        gs.Edges[1].BuildRoad(player3);
        gs.Edges[2].BuildRoad(player3);
        gs.Vertices[5].BuildSettlement(player3);
        gs.Vertices[5].UpgradeToCity();
        gs.Vertices[6].BuildSettlement(player3);
        gs.Vertices[6].UpgradeToCity();

        return gs;
    }

    [Fact]
    public void CountSettlementsForPlayer_CountRedPlayer_1()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = gs.CountSettlementsForPlayer(gs.Players[0]);

        // Assert
        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        Assert.Equal(1, count);
    }

    [Fact]
    public void CountSettlementsForPlayer_CountBluePlayer_3()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = gs.CountSettlementsForPlayer(gs.Players[1]);

        // Assert
        Assert.Equal(PlayerColor.Blue, gs.Players[1].Color);
        Assert.Equal(3, count);
    }

    [Fact]
    public void CountSettlementsForPlayer_CountOrangePlayer_0()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = gs.CountSettlementsForPlayer(gs.Players[2]);

        // Assert
        Assert.Equal(PlayerColor.Orange, gs.Players[2].Color);
        Assert.Equal(0, count);
    }

    [Fact]
    public void CountRoadsForPlayer_CountRedPlayer_1()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = gs.CountRoadsForPlayer(gs.Players[0]);

        // Assert
        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        Assert.Equal(1, count);
    }

    [Fact]
    public void CountRoadsForPlayer_CountBluePlayer_3()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = gs.CountRoadsForPlayer(gs.Players[1]);

        // Assert
        Assert.Equal(PlayerColor.Blue, gs.Players[1].Color);
        Assert.Equal(0, count);
    }

    [Fact]
    public void CountRoadsForPlayer_CountOrangePlayer_0()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = gs.CountRoadsForPlayer(gs.Players[2]);

        // Assert
        Assert.Equal(PlayerColor.Orange, gs.Players[2].Color);
        Assert.Equal(2, count);
    }

    [Fact]
    public void UpdatePlayerVictoryPoints_BluePlayer()
    {
        var gs = CreateTestGameWithManyRoads();
        var bluePlayer = gs.Players.First(p => p.Color == PlayerColor.Blue);
        gs.UpdatePlayerVictoryPoints(bluePlayer);
        Assert.Equal(5, bluePlayer.VictoryPoints);

        bluePlayer.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        gs.UpdatePlayerVictoryPoints(bluePlayer);
        Assert.Equal(6, bluePlayer.VictoryPoints);

        bluePlayer.MakeNewDevelopmentCardsPlayable();
        bluePlayer.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        gs.UpdatePlayerVictoryPoints(bluePlayer);
        Assert.Equal(7, bluePlayer.VictoryPoints);
    }

    // TODO: Add tests where victory points come from dev cards, longest road, and largest army


}