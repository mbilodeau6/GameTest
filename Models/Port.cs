using System;
using System.Threading;
using GameTest.DTOs;

namespace GameTest.Models;

public class Port
{
    private static int s_nextId;

    public string Id { get; init; }

    // References the 2 edges this ports connects to (required)
    public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
    public PortType Type { get; init; }

    public Port(Vertex vertex1, Vertex vertex2, PortType type)
    {
        Id = $"R{Interlocked.Increment(ref s_nextId)}";

        if (vertex1 == null)
            throw new ArgumentNullException("vertex1");

        if (vertex2 == null)
            throw new ArgumentNullException("vertex2");

        if (vertex1.Id == vertex2.Id)
            throw new ArgumentException("The same port was provided twice.");

        Vertices.Add(vertex1);
        Vertices.Add(vertex2);

        Type = type;
    }

    public Port(PortDTO portDto, List<Vertex> vertices)
    {
        Id = portDto.Id;
        Type = portDto.Type;
        
        Vertices = new List<Vertex>();
        foreach (var vertexId in portDto.Vertices)
        {
            var vertex = vertices.FirstOrDefault(v => v.Id == vertexId);
            if (vertex != null)
                Vertices.Add(vertex);
        }
    }
}