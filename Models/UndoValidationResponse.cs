namespace GameTest.Models;

public class UndoValidationResponse
{
    public bool UndoPossible { get; init; } = false;
    public int ErrorCode { get; init; } = 0;
    public Player? Player { get; init; }
    public EventRecordDTO? Event { get; init; }
    public int? EventBlockingUndo {get; init; } = -1;
}