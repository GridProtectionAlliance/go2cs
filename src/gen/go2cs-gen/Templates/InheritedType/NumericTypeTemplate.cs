namespace go2cs.Templates.InheritedType;

internal static class NumericTypeTemplate
{
    private static bool IsUnsignedType(string typeName) =>
        typeName is "uint8" or "uint16" or "uint32" or "uint64" or "byte" or "rune" or "uintptr" or "nuint" or "uint";

    public static string Generate(string typeName, string targetTypeName) =>
        $$"""
        
                public bool Equals({{targetTypeName}} other) => m_value == other.m_value;

                public override bool Equals(object? obj)
                {
                    return obj switch
                    {
                        {{targetTypeName}} other => Equals(other),
                        {{typeName}} value => Equals(value),
                        _ => false
                    };
                }
                
                public override int GetHashCode() => m_value.GetHashCode();
                
                public static bool operator <({{targetTypeName}} left, {{targetTypeName}} right) => left.m_value < right.m_value;
                
                public static bool operator <=({{targetTypeName}} left, {{targetTypeName}} right) => left.m_value <= right.m_value;
                
                public static bool operator >({{targetTypeName}} left, {{targetTypeName}} right) => left.m_value > right.m_value;
                
                public static bool operator >=({{targetTypeName}} left, {{targetTypeName}} right) => left.m_value >= right.m_value;
                
                public static {{targetTypeName}} operator +({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value + right.m_value);
                
                public static {{targetTypeName}} operator -({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value - right.m_value);{{GetUnaryNegationOperator(typeName, targetTypeName)}}
                
                public static {{targetTypeName}} operator *({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value * right.m_value);
                
                public static {{targetTypeName}} operator /({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value / right.m_value);
                
                public static {{targetTypeName}} operator %({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value % right.m_value);

                public static {{targetTypeName}} operator ++({{targetTypeName}} value) => ({{targetTypeName}})(value.m_value + ({{typeName}})1);

                public static {{targetTypeName}} operator --({{targetTypeName}} value) => ({{targetTypeName}})(value.m_value - ({{typeName}})1);{{GetComplementOperator(typeName, targetTypeName)}}
        """;

    // Bitwise complement keeps the WRAPPER type (Go `^T(0)` all-ones idiom - os exec_windows'
    // ^syscall.Handle(0) passed to a Handle parameter; the implicit-to-underlying conversion
    // made `~x` the raw numeric, CS1503). Integer underlyings only - `~` is invalid on floats.
    // Shifts and the binary bitwise operators keep the wrapper type for the same reason: Go
    // `x[i] >> ŝ` on a named integer (math/big's Word) IS a Word, but with no wrapper operator
    // C# resolved through the implicit-to-underlying conversion and the whole expression
    // degraded to the raw numeric (CS0266 ×45 across math/big's arith.cs).
    private static string GetComplementOperator(string typeName, string targetTypeName) => typeName.StartsWith("float") || typeName.StartsWith("complex") ? "" :
       $"""


                public static {targetTypeName} operator ~({targetTypeName} value) => ({targetTypeName})(~value.m_value);

                public static {targetTypeName} operator <<({targetTypeName} value, int shift) => ({targetTypeName})(value.m_value << shift);

                public static {targetTypeName} operator >>({targetTypeName} value, int shift) => ({targetTypeName})(value.m_value >> shift);

                public static {targetTypeName} operator >>>({targetTypeName} value, int shift) => ({targetTypeName})(value.m_value >>> shift);

                public static {targetTypeName} operator &({targetTypeName} left, {targetTypeName} right) => ({targetTypeName})(left.m_value & right.m_value);

                public static {targetTypeName} operator |({targetTypeName} left, {targetTypeName} right) => ({targetTypeName})(left.m_value | right.m_value);

                public static {targetTypeName} operator ^({targetTypeName} left, {targetTypeName} right) => ({targetTypeName})(left.m_value ^ right.m_value);
        """;

    private static string GetUnaryNegationOperator(string typeName, string targetTypeName) => IsUnsignedType(typeName) ? "" : 
       $"""
        
        
                public static {targetTypeName} operator -({targetTypeName} value) => ({targetTypeName})(-value.m_value);
        """;
}
