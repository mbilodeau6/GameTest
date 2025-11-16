using GameTest.Models;

namespace GameTest.Services;

public class CandidatePath
{
    public Edge NextEdge { get; private set; }
    public Edge? FirstEdge { get; private set; }
    private Player? Owner { get; set; } // Saftey check to make sure someone isn't traversing through roads owned by different players.

    public int NewBuildRequired { get; private set; } = 0;

    public CandidatePath(Edge next)
    {
        NextEdge = next;

        if (next.Owner == null)
        {
            FirstEdge = next;
            NewBuildRequired++;
        }
        else
            Owner = next.Owner;
    }

    private void SafetyCheck(Edge next)
    {
         if (next.Owner != null && Owner != null && next.Owner.Id != Owner.Id)
            throw new InvalidOperationException("Unexpected. Caller traversing across roads owned by different players.");
    }

    public CandidatePath CreateBranchOfPath(Edge next)
    {
        SafetyCheck(next);

        CandidatePath newPath = new CandidatePath(next);

        if (FirstEdge != null)
            newPath.FirstEdge = FirstEdge;

        newPath.NewBuildRequired += NewBuildRequired;

        return newPath;
    }

    public void SetNextEdge(Edge next)
    {
        SafetyCheck(next);

        NextEdge = next;

        if (next.Owner == null)
        {
            if  (FirstEdge == null)
                FirstEdge = next;

            NewBuildRequired++;
        }
    }
}
