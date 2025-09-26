using GameTest.Models;

namespace GameTest.DTOs;

public class GameStateDTO
{
    // TODO: I keep on going back and forth on whether I should only store occupied edges/vertices
    // or all edges/vertices in the game. Right now I'm only storing occupied ones. Thinking about
    // changing but need to check with Eric.
    public List<PlayerDTO> Players { get; } = new();
    public List<TileDTO> Tiles { get; } = new();
    public List<EdgeDTO> Edges { get; } = new();
    public List<VertexDTO> Vertices { get; } = new();


    public GameStateDTO(GameState gameState)
    { 
        foreach (var player in gameState.Players)
            Players.Add(new PlayerDTO(player));

        foreach (var tile in gameState.Tiles)
            Tiles.Add(new TileDTO(tile));

        foreach (var edge in gameState.Edges)
            Edges.Add(new EdgeDTO(edge));

        foreach (var vertex in gameState.Vertices)
            Vertices.Add(new VertexDTO(vertex));
    }
}