using static go2cs.Common;

namespace go2cs.Templates.InheritedType;

internal class InheritedTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string ObjectName;
    public string ObjectKind = "struct";
    public bool ReadOnlyValue = true;
    public required string TypeName;
    public required string TargetTypeName;
    public string? TargetValueTypeName = null;
    public string? TargetTypeSize = null;
    public required string TypeClass;

    private string ImplementedInterface => TypeClass switch
    {
        "Slice" => $" : ISlice<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Map" => $" : IMap<{TargetTypeName}, {TargetValueTypeName}>, ISupportMake<{ObjectName}>",
        "Channel" => $" : IChannel<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Array" => $" : IArray<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Pointer" => $" : IPointer<{TargetTypeName}>",
        "Numeric" => $" : IEquatable<{TargetTypeName}>",
        _ => ""
    };

    private string InterfaceImplementation => TypeClass switch
    {
        // TODO: Complete the implementation for the following types
        "Slice" => ISliceTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName),
        //"Map" => IMapTypeTemplate.Generate(TargetTypeName, TargetValueTypeName),
        //"Channel" => IChannelTypeTemplate.Generate(TargetTypeName),
        "Array" => IArrayTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName, TargetTypeSize),
        "Numeric" => NumericTypeTemplate.Generate(TypeName, TargetTypeName),
        "Pointer" => PointerTypeTemplate.Generate(ObjectName, TargetTypeName),
        _ => ""
    };

    private string ValueGetter => TypeClass switch
    {
        "Array" => $"m_value ??= new {TypeName}({TargetTypeSize ?? "0"})",
        _ => "m_value"
    };

    private string Value => TypeClass switch
    {
        "Array" => "val", // Null-coalescing property auto-creates array on first reference
        _ => "m_value"
    };

    private string ToStringImplementation => TypeClass switch
    {
        "bool" => $"{Value}.ToString().ToLowerInvariant()",
        _ => $"{Value}.ToString()"
    };

    private string ReadOnly => ReadOnlyValue ? "readonly " : "";

    private string Nullable => ReadOnlyValue ? "" : "?";

    // A C# constructor name must not carry the type's generic parameters (e.g. the constructor for
    // a generic named array type `vec<T>` is `vec(...)`, not `vec<T>(...)`). Non-generic types have
    // no '<' so ConstructorName equals ObjectName — emitting byte-identical output.
    private string ConstructorName
    {
        get
        {
            int angle = ObjectName.IndexOf('<');
            return angle < 0 ? ObjectName : ObjectName.Substring(0, angle);
        }
    }

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial {{ObjectKind}} {{ObjectName}}{{ImplementedInterface}}
            {
                // Value of the {{ObjectKind}} '{{ObjectName}}'
                private {{ReadOnly}}{{TypeName}}{{Nullable}} m_value;
                {{InterfaceImplementation}}
                
                public {{ConstructorName}}({{TypeName}} value) => m_value = value;

                public {{ConstructorName}}(NilType _) => m_value = default!;

                public {{TypeName}} val => {{ValueGetter}};
                
                public override string ToString() => {{ToStringImplementation}};
        
                public static bool operator ==({{ObjectName}} left, {{ObjectName}} right) => left.Equals(right);
        
                public static bool operator !=({{ObjectName}} left, {{ObjectName}} right) => !(left == right);
        
                // Handle implicit conversions between '{{TypeName}}' and {{ObjectKind}} '{{ObjectName}}'
                public static implicit operator {{ObjectName}}({{TypeName}} value) => new {{ObjectName}}(value);
                    
                public static implicit operator {{TypeName}}({{ObjectName}} value) => value.{{Value}};
                    
                // Handle comparisons between 'nil' and {{ObjectKind}} '{{ObjectName}}'
                public static bool operator ==({{ObjectName}} value, NilType nil) => value.Equals(default({{ObjectName}}));
        
                public static bool operator !=({{ObjectName}} value, NilType nil) => !(value == nil);
        
                public static bool operator ==(NilType nil, {{ObjectName}} value) => value == nil;
        
                public static bool operator !=(NilType nil, {{ObjectName}} value) => value != nil;
        
                public static implicit operator {{ObjectName}}(NilType nil) => default({{ObjectName}})!;
            }
        """;
}
