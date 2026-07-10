using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class ISliceTypeTemplate
{
    public static string Generate(string structName, string typeName, string targetTypeName) =>
        $$"""
        
                public {{targetTypeName}}[] Source => m_value;

                /// <summary>Field-ref accessor for the ж<T>.of() pointer-reinterpret projection —
                /// `(*[][]byte)(buf)` emits `Ꮡbuf.of({{structName}}.Ꮡm_value)`, a true aliasing view.</summary>
                internal static ref slice<{{targetTypeName}}> Ꮡm_value(ref {{structName}} instance) => ref instance.m_value;

                /// <summary>ISliceWrap factory — wraps a window in this named slice type, sharing backing.</summary>
                public static {{structName}} Wrap(in slice<{{targetTypeName}}> source) => new {{structName}}(source);
                    
                public ISlice<{{targetTypeName}}> Append({{targetTypeName}}[] elems) => m_value.Append(elems);
                    
                public nint Low => ((ISlice)m_value).Low;
                
                public nint High => ((ISlice)m_value).High;
                
                public nint Capacity => ((ISlice)m_value).Capacity;
                
                public nint Available => ((ISlice)m_value).Available;
                
                public nint Length => ((IArray)m_value).Length;
                
                global::System.Array IArray.Source => ((IArray)m_value).Source!;
                
                object? IArray.this[nint index]
                {
                    get => ((IArray)m_value)[index];
                    set => ((IArray)m_value)[index] = value;
                }
                    
                public ref {{targetTypeName}} this[nint index] => ref m_value[index];
        
                // Slicing a named slice KEEPS the named type (Go: nat[a:b] IS a nat - math/big's
                // `u[s:].norm()` bound slice<Word> instead, CS1929 x21); the wrapper shares the
                // same backing window.
                public {{structName}} this[global::System.Range range] => new {{structName}}(m_value[range]);

                ISlice<{{targetTypeName}}> ISlice<{{targetTypeName}}>.this[global::System.Range range] => m_value[range];
                
                public {{structName}} Slice(int start, int length) => new {{structName}}(m_value.Slice(start, length));
                
                ISlice<{{targetTypeName}}> ISlice<{{targetTypeName}}>.Slice(int start, int length) => m_value.Slice(start, length);
                
                public {{structName}} Slice(nint start, nint length) => new {{structName}}(m_value.Slice(start, length));
                
                ISlice<{{targetTypeName}}> ISlice<{{targetTypeName}}>.Slice(nint start, nint length) => m_value.Slice(start, length);
        
                public global::System.Span<{{targetTypeName}}> {{EllipsisOperator}} => ToSpan();

                public global::System.Span<{{targetTypeName}}> ToSpan() => m_value.ToSpan();

                public ISlice? Append(object[] elems) => ((ISlice)m_value).Append(elems);

                public global::System.Collections.Generic.IEnumerator<(nint, {{targetTypeName}})> GetEnumerator() => m_value.GetEnumerator();

                global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => ((global::System.Collections.IEnumerable)m_value).GetEnumerator();

                public bool Equals(IArray<{{targetTypeName}}>? other) => m_value.Equals(other);

                public bool Equals(ISlice<{{targetTypeName}}>? other) => m_value.Equals(other);

                public object Clone() => ((global::System.ICloneable)m_value).Clone();
                
                public static {{structName}} Make(nint p1 = 0, nint p2 = -1) => new {{structName}}(p1, p2);
        
                public {{structName}}(nint length, nint capacity = -1, nint low = 0) => m_value = new {{typeName}}(length, capacity, low);
        """;
}
