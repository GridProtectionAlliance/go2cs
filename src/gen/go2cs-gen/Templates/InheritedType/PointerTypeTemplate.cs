namespace go2cs.Templates.InheritedType;

internal static class PointerTypeTemplate
{
    public static string Generate(string className, string targetTypeName) =>
        $$"""
        
                public override bool Equals(object? obj) => obj is {{className}} other && m_value == other.m_value;
                
                public override int GetHashCode() => m_value.GetHashCode();
        
                public ref {{targetTypeName}} val => ref m_value.val;
                
                public ж<TElem> of<TElem>(FieldRefFunc<TElem> fieldRefFunc) => m_value.of(fieldRefFunc);
                
                public ж<TElem> of<TElem>(FieldRefFunc<{{targetTypeName}}, TElem> fieldRefFunc) => m_value.of(fieldRefFunc);
                
                public ж<Telem> at<Telem>(int index) => m_value.at<Telem>(index);
                
                static {{targetTypeName}} IPointer<{{targetTypeName}}>.operator ~(IPointer<{{targetTypeName}}> value) => value.val;
        """;
}
