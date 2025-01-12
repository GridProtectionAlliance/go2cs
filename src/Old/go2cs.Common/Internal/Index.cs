

// ReSharper disable once CheckNamespace
namespace System;

internal readonly struct Index : IEquatable<Index>
{
    private readonly int m_value;

    public Index(int value, bool fromEnd = false)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");

        if (fromEnd)
            m_value = ~value;
        else
            m_value = value;
    }

    private Index(int value)
    {
        m_value = value;
    }

    public static Index Start => new Index(0);

    public static Index End => new Index(~0);

    public static Index FromStart(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");

        return new Index(value);
    }

    public static Index FromEnd(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "value must be non-negative");

        return new Index(~value);
    }

    public int Value => m_value < 0 ? ~m_value : m_value;

    public bool IsFromEnd => m_value < 0;

    public int GetOffset(int length)
    {
        int offset = m_value;

        if (IsFromEnd)
            offset += length + 1;
            
        return offset;
    }

    public override bool Equals(object? value)
    {
        return value is Index index && m_value == index.m_value;
    }

    public bool Equals(Index other) => m_value == other.m_value;

    public override int GetHashCode()
    {
        return m_value;
    }

    public static implicit operator Index(int value)
    {
        return FromStart(value);
    }

    public override string ToString()
    {
        return IsFromEnd ? $"^{(uint)Value}" : ((uint)Value).ToString();
    }
}
