namespace Chess.Structures;

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

public class Move(int source, int target, MOVE_FLAGS flags)
{
    public int Source { get; } = source;
    public int Target { get; } = target;
    public MOVE_FLAGS Flags { get; } = flags;

    public override string ToString()
    {
        return $"Source: {Source}, Target: {Target}, Flags: {Flags}";
    }
}