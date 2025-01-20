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
        "Slice" => $" : ISlice<{TargetTypeName}>",
        "Map" => $" : IMap<{TargetTypeName}, {TargetValueTypeName}>",
        "Channel" => $" : IChannel<{TargetTypeName}>",
        "Array" => $" : IArray<{TargetTypeName}>",
        _ => ""
    };

    public string InterfaceImplementation => TypeClass switch
    {
        // TODO: Complete the implementation for the following types
        "Slice" => ISliceTypeTemplate.Generate(TargetTypeName),
        //"Map" => IMapTypeTemplate.Generate(TargetTypeName, TargetValueTypeName),
        //"Channel" => IChannelTypeTemplate.Generate(TargetTypeName),
        //"Array" => IArrayImplementation.Generate(TargetTypeName),
        _ => ""
    };

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial struct {{StructName}}{{ImplementedInterface}}
            {
                // Value of the {{StructName}} struct
                private readonly {{TypeName}} m_value;
                {{InterfaceImplementation}}
                
                public {{StructName}}({{TypeName}} value) => m_value = value;
        
                // Enable implicit conversions between {{TypeName}} and {{StructName}} struct
                public static implicit operator {{StructName}}({{TypeName}} value) => new {{StructName}}(value);
                    
                public static implicit operator {{TypeName}}({{StructName}} value) => value.m_value;
                    
                // Enable comparisons between nil and {{StructName}} struct
                public static bool operator ==({{StructName}} value, NilType nil) => value.Equals(default({{StructName}}));
        
                public static bool operator !=({{StructName}} value, NilType nil) => !(value == nil);
        
                public static bool operator ==(NilType nil, {{StructName}} value) => value == nil;
        
                public static bool operator !=(NilType nil, {{StructName}} value) => value != nil;
        
                public static implicit operator {{StructName}}(NilType nil) => default({{StructName}});
            }
        """;
}
