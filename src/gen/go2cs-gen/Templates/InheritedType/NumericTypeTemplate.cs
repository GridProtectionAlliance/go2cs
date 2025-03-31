namespace go2cs.Templates.InheritedType;

internal static class NumericTypeTemplate
{
    public static string Generate(string typeName) =>
        $$"""
        
                public override bool Equals(object? obj) => obj is {{typeName}} other && m_value == other.m_value;
                
                public override int GetHashCode() => m_value.GetHashCode();
                
                public static bool operator <({{typeName}} left, {{typeName}} right) => left.m_value < right.m_value;
                
                public static bool operator <=({{typeName}} left, {{typeName}} right) => left.m_value <= right.m_value;
                
                public static bool operator >({{typeName}} left, {{typeName}} right) => left.m_value > right.m_value;
                
                public static bool operator >=({{typeName}} left, {{typeName}} right) => left.m_value >= right.m_value;
                
                public static {{typeName}} operator +({{typeName}} left, {{typeName}} right) => ({{typeName}})(left.m_value + right.m_value);
                
                public static {{typeName}} operator -({{typeName}} left, {{typeName}} right) => ({{typeName}})(left.m_value - right.m_value);
                
                public static {{typeName}} operator -({{typeName}} value) => ({{typeName}})(-value.m_value);
                
                public static {{typeName}} operator *({{typeName}} left, {{typeName}} right) => ({{typeName}})(left.m_value * right.m_value);
                
                public static {{typeName}} operator /({{typeName}} left, {{typeName}} right) => ({{typeName}})(left.m_value / right.m_value);
                
                public static {{typeName}} operator %({{typeName}} left, {{typeName}} right) => ({{typeName}})(left.m_value % right.m_value);
        """;
}
