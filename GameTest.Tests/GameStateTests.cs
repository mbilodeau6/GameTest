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
        Assert.Equal(dto.Settings.Type, game.Settings.Type);

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
        Assert.Equal(dto.Settings.Type, game.Settings.Type);
        Assert.Equal(gs.Phase.PhaseState, game.Phase.PhaseState);
        Assert.Null(game.Phase.CurrentPlayer);
        Assert.Null(game.Phase.EndPlayer);
    }

    [Fact]
    public void Constructor_FromDTO_CreatesValidObject()
    {
        // Arrange
        var gs = new GameState(Guid.NewGuid(), GameType.Test);
        var player1 = Player.CreateTestPlayer("Alice", PlayerColor.Blue);
        gs.AddPlayer(player1);
        var player2 = Player.CreateTestPlayer("Bob", PlayerColor.Red);
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
        Assert.Equal(dto.Settings.Type, game.Settings.Type);
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
    }

    [Fact]
    public void AddPlayer_AddSinglePlayer()
    {
        // Arrange
        string expectedName = "Alice";

        // Act
        var game = new GameState(Guid.NewGuid());
        game.AddPlayer(Player.CreateTestPlayer(expectedName, PlayerColor.Blue));

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
        var p1 = Player.CreateTestPlayer("Ann", PlayerColor.Red);
        game.AddPlayer(p1);
        var p2 = Player.CreateTestPlayer("Tim", PlayerColor.Blue);
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
        var p1 = Player.CreateTestPlayer("Ann", PlayerColor.Red);
        game.AddPlayer(p1);
        var p2 = Player.CreateTestPlayer("Tim", PlayerColor.Blue);
        game.AddPlayer(p2);

        // Act
        game.AssignLargestArmyToPlayer(p1);

        // Assert
        Assert.NotNull(game.PlayerWithLargestArmy);
        Assert.Equal(p1.Id, game.PlayerWithLargestArmy.Id);
    }


    private static GameState CreateTestGameWithManyRoads()
    {
        // TODO: Should use Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        TestHelpers.AddPlayers(gs);

        var player3 = Player.CreateTestPlayer("Alex", PlayerColor.Orange);
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
        Assert.Equal(5, bluePlayer.FullVictoryPoints);

        bluePlayer.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        gs.UpdatePlayerVictoryPoints(bluePlayer);
        Assert.Equal(6, bluePlayer.FullVictoryPoints);

        bluePlayer.MakeNewDevelopmentCardsPlayable();
        bluePlayer.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        gs.UpdatePlayerVictoryPoints(bluePlayer);
        Assert.Equal(7, bluePlayer.FullVictoryPoints);
    }

    // TODO: Add tests where victory points come from dev cards, longest road, and largest army

    private static TestGameBoard CreateBoardWithOnlyOneOfEachBuildAvailable()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var player = board.GetRedPlayer();

        // build all but one road
        board.GetEdge(TestEdge.E25).BuildRoad(player);
        board.GetEdge(TestEdge.E5).BuildRoad(player);
        board.GetEdge(TestEdge.E12).BuildRoad(player);
        board.GetEdge(TestEdge.E6).BuildRoad(player);

        // build all but one city
        board.GetVertex(TestVertex.V5).UpgradeToCity();

        // build all but one settlement
        board.GetVertex(TestVertex.V20).BuildSettlement(player);
        board.GetVertex(TestVertex.V22).BuildSettlement(player);

        return board;
    }

    private static TestGameBoard CreateBoardWithAllBuildingsInUse()
    {
        var board = CreateBoardWithOnlyOneOfEachBuildAvailable();

        // Build remaining buildings to hit max
        board.GetVertex(TestVertex.V1).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V1).UpgradeToCity();
        board.GetEdge(TestEdge.E24).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V18).BuildSettlement(board.GetRedPlayer());

        return board;
    }


    [Fact]
    public void UnusedRoadAvailable_Yes()
    {
        // Arrange
        var board = CreateBoardWithOnlyOneOfEachBuildAvailable();

        // Act & Assert
        Assert.True(board.GetGameState().UnusedRoadAvailable(board.GetRedPlayer())); 
    }

    [Fact]
    public void UnusedRoadAvailable_No()
    {
        // Arrange
        var board = CreateBoardWithAllBuildingsInUse();

        // Act & Assert
        Assert.False(board.GetGameState().UnusedRoadAvailable(board.GetRedPlayer())); 
    }

    [Fact]
    public void UnusedSettlementAvailable_Yes()
    {
        // Arrange
        var board = CreateBoardWithOnlyOneOfEachBuildAvailable();

        // Act & Assert
        Assert.True(board.GetGameState().UnusedSettlementAvailable(board.GetRedPlayer())); 
    }

    [Fact]
    public void UnusedSettlementAvailable_No()
    {
        // Arrange
        var board = CreateBoardWithAllBuildingsInUse();

        // Act & Assert
        Assert.False(board.GetGameState().UnusedSettlementAvailable(board.GetRedPlayer())); 
    }

    [Fact]
    public void UnusedCityAvailable_Yes()
    {
        // Arrange
        var board = CreateBoardWithOnlyOneOfEachBuildAvailable();

        // Act & Assert
        Assert.True(board.GetGameState().UnusedCityAvailable(board.GetRedPlayer())); 
    }

    [Fact]
    public void UnusedCityAvailable_No()
    {
        // Arrange
        var board = CreateBoardWithAllBuildingsInUse();

        // Act & Assert
        Assert.False(board.GetGameState().UnusedCityAvailable(board.GetRedPlayer())); 
    }

    private TestGameBoard CreateLengthTestBoard2_1()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V1).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E1).BuildRoad(board.GetRedPlayer());

        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E3).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E10).BuildRoad(board.GetBluePlayer());

        return board;
    }
    [Fact]
    public void GetLongestRoadLength_One()
    {
        var board = CreateLengthTestBoard2_1();
        Assert.Equal(1, board.GetGameState().GetLongestRoadLength(board.GetRedPlayer()));        
    }

    [Fact]
    public void GetLongestRoadLength_Two()
    {
        var board = CreateLengthTestBoard2_1();
        Assert.Equal(2, board.GetGameState().GetLongestRoadLength(board.GetBluePlayer()));        
    }

    private TestGameBoard CreateLengthTestBoard6_4()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V22).BuildSettlement(board.GetBluePlayer());
        board.GetVertex(TestVertex.V1).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E27).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E26).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E25).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E11).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E5).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E12).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E6).BuildRoad(board.GetBluePlayer());

        board.GetVertex(TestVertex.V5).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V13).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E4).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E3).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E9).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E18).BuildRoad(board.GetRedPlayer());

        return board;
    }

    [Fact]
    public void GetLongestRoadLength_StraightFour()
    {
        var board = CreateLengthTestBoard6_4();

        Assert.Equal(4, board.GetGameState().GetLongestRoadLength(board.GetRedPlayer()));        
    }

    [Fact]
    public void GetLongestRoadLength_SixDueToBranch()
    {
        var board = CreateLengthTestBoard6_4();

        Assert.Equal(6, board.GetGameState().GetLongestRoadLength(board.GetBluePlayer()));        
    }

    private TestGameBoard CreateLengthTestBoard7_2()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V15).BuildSettlement(board.GetBluePlayer());
        board.GetVertex(TestVertex.V6).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E5).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E4).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E10).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E21).BuildRoad(board.GetBluePlayer());

        board.GetVertex(TestVertex.V4).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V13).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E3).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E9).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E16).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E17).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E18).BuildRoad(board.GetRedPlayer());

        return board;
    }

    [Fact]
    public void GetLongestRoadLength_Seven()
    {
        var board = CreateLengthTestBoard7_2();

        Assert.Equal(7, board.GetGameState().GetLongestRoadLength(board.GetRedPlayer()));        
    }

    [Fact]
    public void GetLongestRoadLength_TwoDueToSplit()
    {
        var board = CreateLengthTestBoard7_2();

        Assert.Equal(2, board.GetGameState().GetLongestRoadLength(board.GetBluePlayer()));        
    }

    private TestGameBoard CreateLengthTestBoard8_4()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetVertex(TestVertex.V9).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E14).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E13).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E7).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E1).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E15).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E16).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E3).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E6).BuildRoad(board.GetBluePlayer());

        board.GetVertex(TestVertex.V4).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V18).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E10).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E4).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E11).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E24).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E5).BuildRoad(board.GetRedPlayer());

        return board;
    }

    [Fact]
    public void GetLongestRoadLength_FourBranch()
    {
        var board = CreateLengthTestBoard8_4();

        Assert.Equal(4, board.GetGameState().GetLongestRoadLength(board.GetRedPlayer()));        
    }


    [Fact]
    public void GetLongestRoadLength_Eight()
    {
         var board = CreateLengthTestBoard8_4();

        Assert.Equal(8, board.GetGameState().GetLongestRoadLength(board.GetBluePlayer()));        
   }

   [Fact]
   public void Constructor_MaintainLongestRoadAndLargestArmy()
    {
        var gs = new GameState(new Guid());
        gs.AddPlayer(Player.CreateTestPlayer("Tim", PlayerColor.White));
        gs.AddPlayer(Player.CreateTestPlayer("Mary", PlayerColor.Brown));
        gs.AssignLargestArmyToPlayer(gs.Players[0]);
        gs.AssignLongestRoadToPlayer(gs.Players[1]);

        var dto = new GameStateDTO(gs);

        var newGS = new GameState(dto);

        Assert.NotNull(newGS.PlayerWithLargestArmy);
        Assert.Equal(gs.Players[0].Id, newGS.PlayerWithLargestArmy.Id);
        Assert.NotNull(newGS.PlayerWithLongestRoad);
        Assert.Equal(gs.Players[1].Id, newGS.PlayerWithLongestRoad.Id);
    }
}