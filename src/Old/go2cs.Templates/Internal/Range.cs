// Proxy implementations of System.Range and System.Index for .NET Standard 2.0



// ReSharper disable once CheckNamespace
namespace System;

internal readonly struct Range(Index start, Index end) : IEquatable<Range>
{
    public Index Start { get; } = start;

    public Index End { get; } = end;

    public override bool Equals(object? value)
    {
        return value is Range r && r.Start.Equals(Start) && r.End.Equals(End);
    }

    public bool Equals(Range other)
    {
        return other.Start.Equals(Start) && other.End.Equals(End);
    }

    public override int GetHashCode()
    {
        return Start.GetHashCode() * 31 + End.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Start}..{End}";
    }

    public static Range StartAt(Index start)
    {
        return new Range(start, Index.End);
    }

    public static Range EndAt(Index end)
    {
        return new Range(Index.Start, end);
    }

    public static Range All => new(Index.Start, Index.End);

    public (int Offset, int Length) GetOffsetAndLength(int length)
    {
        int start;
        Index startIndex = Start;

        if (startIndex.IsFromEnd)
            start = length - startIndex.Value;
        else
            start = startIndex.Value;

        int end;
        Index endIndex = End;
            
        if (endIndex.IsFromEnd)
            end = length - endIndex.Value;
        else
            end = endIndex.Value;

        if ((uint)end > (uint)length || (uint)start > (uint)end)
            throw new ArgumentOutOfRangeException(nameof(length));

        return (start, end - start);
    }
}