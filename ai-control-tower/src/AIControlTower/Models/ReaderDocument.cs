namespace AIControlTower.Models;

public sealed record ReaderDocument(string FilePath, string Title, string RawText, string Format, string Error)
{
    public bool Success => Error.Length == 0;
}
public sealed record DocumentSearchHit(int Offset, int Length, int Line, string Preview);
