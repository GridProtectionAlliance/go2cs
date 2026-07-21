// IArrayTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class IArrayTypeTemplate
{
    public static string Generate(string structName, string typeName, string targetTypeName, string? targetTypeSize) =>
        $$"""
                
                public {{targetTypeName}}[] Source => Value;
                
                public nint Length => Value.Length;
                
                global::System.Array IArray.Source => ((IArray)Value).Source!;
                
                object? IArray.this[nint index]
                {
                    get => ((IArray)Value)[index];
                    set => ((IArray)Value)[index] = value;
                }
                    
                public ref {{targetTypeName}} this[nint index] => ref Value[index];
            
                public ref {{targetTypeName}} this[int index] => ref Value[(nint)index];
            
                public ref {{targetTypeName}} this[ulong index] => ref Value[(nint)index];

                public slice<{{targetTypeName}}> this[global::System.Range range] => Value[range];

                public slice<{{targetTypeName}}> Slice(nint start, nint length) => Value.Slice(start, length);

                public global::System.Span<{{targetTypeName}}> {{EllipsisOperator}} => ToSpan();

                public global::System.Span<{{targetTypeName}}> ToSpan() => Value.ToSpan();

                public global::System.Collections.Generic.IEnumerator<(nint, {{targetTypeName}})> GetEnumerator() => Value.GetEnumerator();

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable)Value).GetEnumerator();

                public bool Equals(IArray<{{targetTypeName}}>? other) => Value.Equals(other);

                public {{structName}} Clone() => new {{structName}}(Value.Clone());

                object global::System.ICloneable.Clone() => Clone();

                public static {{structName}} Make(nint p1 = 0, nint p2 = -1) => new {{structName}}();
        """;
}
