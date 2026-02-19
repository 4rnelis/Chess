using Chess.Engine.Logic;
namespace Chess.Engine.Structures;

[Flags]
public enum MOVE_FLAGS
{
    None = 0,
    Capture = 1 << 0,
    Promotion = 1 << 1,
    EnPassant = 1 << 2,
    Castling = 1 << 3,
    DoublePawnPush = 1 << 4
}

public readonly struct Move
{
    public readonly int Source;
    public readonly int Target;
    public readonly MOVE_FLAGS Flags;
    public readonly PIECE_TYPE Promotion;

    public Move(int source, int target, MOVE_FLAGS flags)
    {
        Source = source;
        Target = target;
        Flags = flags;
        Promotion = PIECE_TYPE.NONE;
    }

    public Move(int source, int target, MOVE_FLAGS flags, PIECE_TYPE promotion)
    {
        Source = source;
        Target = target;
        Flags = flags;
        Promotion = promotion;
    }

    public override string ToString()
    {
        return $"Source: {MoveMaker.SquareToUCI(Source)}, Target: {MoveMaker.SquareToUCI(Target)}, Flags: {Flags}";
    }
}
