using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class IArrayTypeTemplate
{
    public static string Generate(string structName, string typeName, string targetTypeName, string? targetTypeSize) =>
        $$"""
                
                public {{targetTypeName}}[] Source => val;
                
                public nint Length => val.Length;
                
                Array IArray.Source => ((IArray)val).Source!;
                
                object? IArray.this[nint index]
                {
                    get => ((IArray)val)[index];
                    set => ((IArray)val)[index] = value;
                }
                    
                public ref {{targetTypeName}} this[nint index] => ref val[index];
            
                public ref {{targetTypeName}} this[int index] => ref val[(nint)index];
            
                public ref {{targetTypeName}} this[ulong index] => ref val[(nint)index];
                
                public Span<{{targetTypeName}}> {{EllipsisOperator}} => ToSpan();
                
                public Span<{{targetTypeName}}> ToSpan() => val.ToSpan();
                
                public IEnumerator<(nint, {{targetTypeName}})> GetEnumerator() => val.GetEnumerator();
                
                IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)val).GetEnumerator();
                
                public bool Equals(IArray<{{targetTypeName}}>? other) => val.Equals(other);
                
                public object Clone() => ((ICloneable)val).Clone();
                
                public static {{structName}} Make(nint p1 = 0, nint p2 = -1) => new {{structName}}();
        """;
}
