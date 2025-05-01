using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.StructType;

internal class StructTypeTemplate : TemplateBase
{
    // Template Parameters
    public required GeneratorExecutionContext Context;
    public required string StructName;
    public required string FullyQualifiedStructType;
    public required List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)> StructMembers;
    public required bool HasEqualityOperators;

    private string? m_nonGenericStructName;
    public string NonGenericStructName => m_nonGenericStructName ??= GetSimpleName(StructName, true);

    private List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)>? m_publicStructMembers;
    private List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)> PublicStructMembers => 
        m_publicStructMembers ??= StructMembers.Where(item => char.IsUpper(GetSimpleName(item.memberName)[0])).ToList();

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial struct {{StructName}}
            {
                // Promoted Struct References
                {{PromotedStructDeclarations}}
        
                // Field References
                {{FieldReferences}}
                
                // Constructors
                {{Constructors}}
                
                // Handle comparisons between struct '{{NonGenericStructName}}' instances
                public bool Equals({{StructName}} other) =>
                    {{CompareFields}};
                
                public override bool Equals(object? obj) => obj is {{StructName}} other && Equals(other);
                
                public override int GetHashCode() => {{HashCode}};
                
                public static bool operator ==({{StructName}} left, {{StructName}} right) => left.Equals(right);
                
                public static bool operator !=({{StructName}} left, {{StructName}} right) => !(left == right);
        
                // Handle comparisons between 'nil' and struct '{{NonGenericStructName}}'
                public static bool operator ==({{StructName}} value, NilType nil) => value.Equals(default({{StructName}}));

                public static bool operator !=({{StructName}} value, NilType nil) => !(value == nil);

                public static bool operator ==(NilType nil, {{StructName}} value) => value == nil;

                public static bool operator !=(NilType nil, {{StructName}} value) => value != nil;

                public static implicit operator {{StructName}}(NilType nil) => default({{StructName}});

                public override string ToString() => string.Concat("{", string.Join(" ",
                [
                    {{(StructMembers.Count > 0 ? string.Join(",\r\n            ", StructMembers.Select(GetToStringImplementation)) : "\"\"")}}
                ]), "}");
            }{{PromotedStructReceivers()}}
        """;

    private string PromotedStructDeclarations
    {
        get
        {
            (string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)[] promotedStructs = StructMembers.Where(item => item.isPromotedStruct).ToArray();

            if (promotedStructs.Length == 0)
                return $"// -- {NonGenericStructName} has no promoted structs";

            StringBuilder result = new();

            foreach ((string typeName, string memberName, _, _) in promotedStructs)
            {
                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                result.Append($"private readonly {PointerPrefix}<{typeName}> {AddressPrefix}{CapturedVarMarker}{memberName};");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Accessors");

            foreach ((string typeName, string memberName, _, _) in promotedStructs)
            {
                string typeScope = char.IsUpper(GetSimpleName(typeName)[0]) ? "public" : "internal";
                result.Append($"\r\n{TypeElemIndent}{typeScope} partial ref {typeName} {memberName} => ref {AddressPrefix}{CapturedVarMarker}{memberName}.val;");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessors");

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                foreach ((string typeName, string memberName) in getStructMembers(promotedStructType))
                {
                    string typeScope = char.IsUpper(GetSimpleName(typeName)[0]) ? "public" : "internal";
                    result.Append($"\r\n{TypeElemIndent}{typeScope} ref {typeName} {memberName} => ref {GetSimpleName(promotedStructType, dropCollisionPrefix: true)}.{memberName};");
                }
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessor References");

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                foreach ((string typeName, string memberName) in getStructMembers(promotedStructType))
                {
                    string typeScope = char.IsUpper(GetSimpleName(typeName)[0]) ? "public" : "internal";
                    result.Append($"\r\n{TypeElemIndent}{typeScope} static ref {typeName} {AddressPrefix}{memberName}(ref {NonGenericStructName} instance) => ref instance.{GetSimpleName(promotedStructType, dropCollisionPrefix: true)}.{memberName};");
                }
            }

            return result.ToString();

            IEnumerable<(string typeName, string memberName)> getStructMembers(string structTypeName)
            {
                (StructDeclarationSyntax? structDecl, Compilation? compilation) = Context.GetStructDeclaration(structTypeName);
                
                if (structDecl is null)
                    return [];

                return structDecl
                    .GetStructMembers(compilation!, true)
                    .Select(item => (item.typeName, item.memberName)) ?? [];
            }
        }
    }

    private string PromotedStructReceivers()
    {
        (string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)[] promotedStructs = StructMembers.Where(item => item.isPromotedStruct).ToArray();

        if (promotedStructs.Length == 0)
            return "";

        StringBuilder result = new();

        result.Append("\r\n\r\n    // Promoted Struct Receivers");

        // Get all extension methods for the struct, any directly defined receivers
        // take precedence over promoted struct methods that have the same name
        (StructDeclarationSyntax? structDecl, Compilation? compilation) = Context.GetStructDeclaration(FullyQualifiedStructType);
        IEnumerable<MethodInfo>? structMethods = structDecl is null ? [] : structDecl.GetExtensionMethods(compilation!);
        HashSet<string> structMethodNames = new(structMethods?.Select(method => method.Name) ?? [], StringComparer.Ordinal);

        foreach ((string promotedStructType, _, _, _) in promotedStructs)
        {
            (structDecl, compilation) = Context.GetStructDeclaration(promotedStructType);
            IEnumerable<MethodInfo>? promotedStructMethods = structDecl is null ?[] : structDecl.GetExtensionMethods(compilation!);

            foreach (MethodInfo method in promotedStructMethods ?? [])
            {
                if (structMethodNames.Contains(method.Name))
                {
                    result.Append($"\r\n    // '{GetSimpleName(promotedStructType)}.{method.Name}' method mapped to overridden '{NonGenericStructName}' receiver method");
                    continue;
                }

                // Add ref extension method
                string methodScope = Scope ?? "public";
                methodScope = method.ReturnType == "void" ? methodScope : char.IsUpper(GetSimpleName(method.ReturnType)[0]) ? methodScope : "internal";
                result.Append($"\r\n    {methodScope} static {method.ReturnType} {method.Name}(this ref {StructName} target");

                if (method.Parameters.Length > 1)
                {
                    result.Append(", ");
                    result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => $"{param.type} {param.name}")));
                }
                
                result.Append($") => target.{GetSimpleName(promotedStructType, dropCollisionPrefix: true)}.{method.Name}(");
                result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => param.name)));
                result.Append(");");

                // Add pointer extension method
                result.Append($"\r\n    {methodScope} static {method.ReturnType} {method.Name}(this {PointerPrefix}<{StructName}> {AddressPrefix}target");

                if (method.Parameters.Length > 1)
                {
                    result.Append(", ");
                    result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => $"{param.type} {param.name}")));
                }

                result.AppendLine(")");
                result.AppendLine("    {");
                result.AppendLine($"        ref var target = ref {AddressPrefix}target.val;");
                result.Append($"        {(method.ReturnType == "void" ? "" : "return ")}target.{method.Name}(");
                result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => param.name)));
                result.AppendLine(");");
                result.Append("    }");
            }
        }

        return result.ToString();
    }

    private string FieldReferences
    {
        get
        {
            if (StructMembers.Count == 0)
                return $"// -- {NonGenericStructName} has no defined fields";

            StringBuilder result = new();

            foreach ((string typeName, string memberName, _, _) in StructMembers)
            {
                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                string fieldScope = char.IsUpper(GetSimpleName(typeName)[0]) ? "public" : "internal";

                result.Append($"{fieldScope} static ref {typeName} {AddressPrefix}{GetUnsanitizedIdentifier(memberName)}(ref {StructName} instance) => ref instance.{memberName};");
            }

            return result.ToString();
        }
    }

    private string Constructors
    {
        get
        {
            StringBuilder result = new();

            result.AppendLine($"public {NonGenericStructName}(NilType _)");
            result.AppendLine($"{TypeElemIndent}{{");

            // Construct from nil
            foreach ((string typeName, string memberName, _, bool isPromotedStruct) in StructMembers)
            {
                result.Append($"{TypeElemIndent}    ");

                result.AppendLine(isPromotedStruct ? 
                    $"{AddressPrefix}{CapturedVarMarker}{memberName} = new {PointerPrefix}<{typeName}>(new {typeName}(nil));" : 
                    $"this.{memberName} = default!;");
            }

            result.AppendLine($"{TypeElemIndent}}}");

            // Generate exported constructor from public fields
            GenerateConstructor("public", PublicStructMembers, result);

            // Generate internal constructor with all fields
            if (PublicStructMembers.Count != StructMembers.Count)
            {
                result.AppendLine();
                GenerateConstructor("internal", StructMembers, result);
            }

            return result.ToString();
        }
    }

    private void GenerateConstructor(string scope, List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)> structMembers, StringBuilder result)
    {
        if (structMembers.Count == 0)
            return;

        result.AppendLine();
        result.Append($"{TypeElemIndent}{scope} {NonGenericStructName}(");
        result.Append(string.Join(", ", structMembers.Select(item => $"{item.typeName} {item.memberName} = default!")));
        result.AppendLine(")");
        result.AppendLine($"{TypeElemIndent}{{");

        foreach ((string typeName, string memberName, _, bool isPromotedStruct) in structMembers)
        {
            result.Append($"{TypeElemIndent}    ");

            result.AppendLine(isPromotedStruct ?
                $"{AddressPrefix}{CapturedVarMarker}{memberName} = new {PointerPrefix}<{typeName}>({memberName});" :
                $"this.{memberName} = {memberName};");
        }

        result.Append($"{TypeElemIndent}}}");
    }

    private static string GetToStringImplementation((string typeName, string memberName, bool isReferenceType, bool isPromotedStruct) item)
    {
        return item.isReferenceType ? $"{item.memberName}?.ToString() ?? \"<nil>\"" : $"{item.memberName}.ToString()";
    }

    private string CompareFields => HasEqualityOperators && StructMembers.Count > 0 ? 
        string.Join(" &&\r\n            ", CompareList) :
        StructMembers.Count > 0 ? "false /* missing equality constraints */" : "true /* empty */";

    private IEnumerable<string> CompareList => StructMembers.Select(member => $"{member.memberName} == other.{member.memberName}");

    public string HashCode => StructMembers.Count == 0 ? "base.GetHashCode()" : 
        $"""                    
        runtime.HashCode.Combine(
                    {ParamList})
        """;

    private string ParamList => string.Join(",\r\n            ", StructMembers.Select(member => member.memberName));
}
