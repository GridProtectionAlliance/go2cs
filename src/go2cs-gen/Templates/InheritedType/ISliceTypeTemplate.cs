namespace go2cs.Templates.InheritedType;

internal static class ISliceTypeTemplate
{
    public static string Generate(string typeName) =>
        $$"""
        
                public {{typeName}}[] Source => m_value;
                    
                public ISlice<{{typeName}}> Append({{typeName}}[] elems) => m_value.Append(elems);
                    
                public nint Low => ((ISlice)m_value).Low;
                
                public nint High => ((ISlice)m_value).High;
                
                public nint Capacity => ((ISlice)m_value).Capacity;
                
                public nint Available => ((ISlice)m_value).Available;
                
                public nint Length => ((IArray)m_value).Length;
                
                Array IArray.Source => ((IArray)m_value).Source!;
                
                object? IArray.this[nint index]
                {
                    get => ((IArray)m_value)[index];
                    set => ((IArray)m_value)[index] = value;
                }
                    
                public ref {{typeName}} this[nint index]
                {
                    get => ref m_value[index];
                }
                
                public Span<{{typeName}}> ꓸꓸꓸ => ToSpan();
                
                public Span<{{typeName}}> ToSpan()
                {
                    return m_value.ToSpan();
                }
                
                public ISlice? Append(object[] elems)
                {
                    return ((ISlice)m_value).Append(elems);
                }
                
                public IEnumerator<(nint, {{typeName}})> GetEnumerator()
                {
                    return m_value.GetEnumerator();
                }
                
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return ((IEnumerable)m_value).GetEnumerator();
                }
                
                public bool Equals(IArray<{{typeName}}>? other)
                {
                    return m_value.Equals(other);
                }
                
                public bool Equals(ISlice<{{typeName}}>? other)
                {
                   return m_value.Equals(other);
                }
                
                public object Clone() => ((ICloneable)m_value).Clone();
        """;
}
