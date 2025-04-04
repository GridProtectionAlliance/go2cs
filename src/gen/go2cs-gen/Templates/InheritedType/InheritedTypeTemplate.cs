using static go2cs.Common;

namespace go2cs.Templates.InheritedType;

internal class InheritedTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string ObjectName;
    public string ObjectKind = "struct";
    public required string TypeName;
    public required string TargetTypeName;
    public string? TargetValueTypeName = null;
    public required string TypeClass;

    public string ImplementedInterface => TypeClass switch
    {
        "Slice" => $" : ISlice<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Map" => $" : IMap<{TargetTypeName}, {TargetValueTypeName}>, ISupportMake<{ObjectName}>",
        "Channel" => $" : IChannel<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Array" => $", IArray<{TargetTypeName}>",
        "Pointer" => $" : IPointer<{TargetTypeName}>",
        "Numeric" => $" : IEquatable<{TargetTypeName}>",
        _ => ""
    };

    public string InterfaceImplementation => TypeClass switch
    {
        // TODO: Complete the implementation for the following types
        "Slice" => ISliceTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName),
        //"Map" => IMapTypeTemplate.Generate(TargetTypeName, TargetValueTypeName),
        //"Channel" => IChannelTypeTemplate.Generate(TargetTypeName),
        //"Array" => IArrayImplementation.Generate(TargetTypeName),
        "Numeric" => NumericTypeTemplate.Generate(TypeName, TargetTypeName),
        "Pointer" => PointerTypeTemplate.Generate(ObjectName, TargetTypeName),
        _ => ""
    };

    public string ToStringImplementation => TypeClass switch
    {
        "bool" => "m_value.ToString().ToLowerInvariant()",
        _ => "m_value.ToString()"
    };

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial {{ObjectKind}} {{ObjectName}}{{ImplementedInterface}}
            {
                // Value of the {{ObjectKind}} '{{ObjectName}}'
                private readonly {{TypeName}} m_value;
                {{InterfaceImplementation}}
                
                public {{ObjectName}}({{TypeName}} value) => m_value = value;
        
                public override string ToString() => {{ToStringImplementation}};
        
                public static bool operator ==({{ObjectName}} left, {{ObjectName}} right) => left.Equals(right);
        
                public static bool operator !=({{ObjectName}} left, {{ObjectName}} right) => !(left == right);
        
                // Handle implicit conversions between '{{TypeName}}' and {{ObjectKind}} '{{ObjectName}}'
                public static implicit operator {{ObjectName}}({{TypeName}} value) => new {{ObjectName}}(value);
                    
                public static implicit operator {{TypeName}}({{ObjectName}} value) => value.m_value;
                    
                // Handle comparisons between 'nil' and {{ObjectKind}} '{{ObjectName}}'
                public static bool operator ==({{ObjectName}} value, NilType nil) => value.Equals(default({{ObjectName}}));
        
                public static bool operator !=({{ObjectName}} value, NilType nil) => !(value == nil);
        
                public static bool operator ==(NilType nil, {{ObjectName}} value) => value == nil;
        
                public static bool operator !=(NilType nil, {{ObjectName}} value) => value != nil;
        
                public static implicit operator {{ObjectName}}(NilType nil) => default({{ObjectName}})!;
            }
        """;
}
