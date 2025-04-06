namespace go2cs.Templates.InheritedType;

internal static class IArrayTypeTemplate
{
    public static string Generate(string structName, string typeName, string targetTypeName) =>
        $$"""
        
                public {{targetTypeName}}[] Source => m_value;
                
                public nint Length => ((IArray)m_value).Length;
                
                Array IArray.Source => ((IArray)m_value).Source!;
                
                object? IArray.this[nint index]
                {
                    get => ((IArray)m_value)[index];
                    set => ((IArray)m_value)[index] = value;
                }
                    
                public ref {{targetTypeName}} this[nint index] => ref m_value[index];
                
                public Span<{{targetTypeName}}> ꓸꓸꓸ => ToSpan();
                
                public Span<{{targetTypeName}}> ToSpan() => m_value.ToSpan();
                
                public IEnumerator<(nint, {{targetTypeName}})> GetEnumerator() => m_value.GetEnumerator();
                
                IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_value).GetEnumerator();
                
                public bool Equals(IArray<{{targetTypeName}}>? other) => m_value.Equals(other);
                
                public object Clone() => ((ICloneable)m_value).Clone();
                
                public static {{structName}} Make(nint p1 = 0, nint p2 = -1) => new {{structName}}(p1);
        
                public {{structName}}(nint length) => m_value = new {{typeName}}(length);        
        """;
}
