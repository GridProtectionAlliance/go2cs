using System;

namespace go;

// IByteSeq models Go's `string | []byte` union constraint, which C# cannot express directly
// (C# generic constraints are conjunctive — "and", never "or"). It exposes only the read
// operations common to both members — length, indexing, and sub-slicing — which is all a
// `string | []byte`-constrained function body can legally use (the union is neither comparable
// nor additive, so no operator interfaces are implied). The converter emits this interface
// (instantiated as IByteSeq<byte>) for such constraints; see getGenericDefinition.
//
// It is generic so the generic slice<T> can implement it (a byte-specific interface could not be
// conditionally implemented only when T is byte). @string implements IByteSeq<byte>; slice<T>
// implements IByteSeq<T>, so slice<byte> satisfies the IByteSeq<byte> constraint as well.
public interface IByteSeq
{
    // Length is the element count (Go's len). nint so it matches the indexer/slice domain.
    nint Length { get; }
}

public interface IByteSeq<T> : IByteSeq
{
    // Read-only element access (Go's s[i]). A ref-returning indexer (e.g. slice<T>'s) satisfies
    // this through an explicit implementation that reads the element by value.
    T this[nint index] { get; }

    // Sub-slice (Go's s[lo:hi]); returns the same sequence kind so a chain stays an IByteSeq<T>.
    // For @string this is an @string, for slice<T> a slice<T> — both already IByteSeq<T>.
    IByteSeq<T> this[Range range] { get; }
}
