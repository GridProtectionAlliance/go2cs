using static go2cs.Common;

namespace go2cs.Templates.InheritedType;

internal class InheritedTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required string TypeName;
    public required string TargetTypeName;
    public string? TargetValueTypeName = null;
    public required string TypeClass;

    public string ImplementedInterface => TypeClass switch
    {
        "Slice" => $" : ISlice<{TargetTypeName}>, ISupportMake<{StructName}>",
        "Map" => $" : IMap<{TargetTypeName}, {TargetValueTypeName}>, ISupportMake<{StructName}>",
        "Channel" => $" : IChannel<{TargetTypeName}>, ISupportMake<{StructName}>",
        "Array" => $", IArray<{TargetTypeName}>",
        _ => ""
    };

    public string InterfaceImplementation => TypeClass switch
    {
        // TODO: Complete the implementation for the following types
        "Slice" => ISliceTypeTemplate.Generate(StructName, TypeName, TargetTypeName),
        //"Map" => IMapTypeTemplate.Generate(TargetTypeName, TargetValueTypeName),
        //"Channel" => IChannelTypeTemplate.Generate(TargetTypeName),
        //"Array" => IArrayImplementation.Generate(TargetTypeName),
        "Numeric" => NumericTypeTemplate.Generate(TargetTypeName),
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
            {{Scope}} partial struct {{StructName}}{{ImplementedInterface}}
            {
                // Value of the struct '{{StructName}}'
                private readonly {{TypeName}} m_value;
                {{InterfaceImplementation}}
                
                public {{StructName}}({{TypeName}} value) => m_value = value;
        
                public override string ToString() => {{ToStringImplementation}};
        
                public static bool operator ==({{StructName}} left, {{StructName}} right) => left.Equals(right);
        
                public static bool operator !=({{StructName}} left, {{StructName}} right) => !(left == right);
        
                // Handle implicit conversions between '{{TypeName}}' and struct '{{StructName}}'
                public static implicit operator {{StructName}}({{TypeName}} value) => new {{StructName}}(value);
                    
                public static implicit operator {{TypeName}}({{StructName}} value) => value.m_value;
                    
                // Handle comparisons between 'nil' and struct '{{StructName}}'
                public static bool operator ==({{StructName}} value, NilType nil) => value.Equals(default({{StructName}}));
        
                public static bool operator !=({{StructName}} value, NilType nil) => !(value == nil);
        
                public static bool operator ==(NilType nil, {{StructName}} value) => value == nil;
        
                public static bool operator !=(NilType nil, {{StructName}} value) => value != nil;
        
                public static implicit operator {{StructName}}(NilType nil) => default({{StructName}});
            }
        """;
}
