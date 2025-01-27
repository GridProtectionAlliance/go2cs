using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;

namespace go2cs.Templates.StructType;

internal class StructTypeTemplate : TemplateBase
{
    // Template Parameters
    public required GeneratorExecutionContext Context;
    public required string StructName;
    public required string FullyQualifiedStructType;
    public required List<(string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)> StructMembers;

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
                
                // Enable comparisons between nil and {{StructName}} struct
                public static bool operator ==({{StructName}} value, NilType nil) => value.Equals(default({{StructName}}));

                public static bool operator !=({{StructName}} value, NilType nil) => !(value == nil);

                public static bool operator ==(NilType nil, {{StructName}} value) => value == nil;

                public static bool operator !=(NilType nil, {{StructName}} value) => value != nil;

                public static implicit operator {{StructName}}(NilType nil) => default({{StructName}});

                public override string ToString() => string.Concat("{", string.Join(" ",
                [
                    {{(StructMembers.Count > 0 ? string.Join(", ", StructMembers.Select(GetToStringImplementation)) : "\"\"")}}
                ]), "}");
            }
        """;

    private string PromotedStructDeclarations
    {
        get
        {
            (string typeName, string memberName, bool isReferenceType, bool isPromotedStruct)[] promotedStructs = StructMembers.Where(item => item.isPromotedStruct).ToArray();

            if (promotedStructs.Length == 0)
                return $"// -- {StructName} has no promoted structs";

            StringBuilder result = new();

            foreach ((string typeName, string memberName, _, _) in promotedStructs)
            {
                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                result.Append($"private readonly ж<{typeName}> Ꮡʗ{memberName};");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Accessors");

            foreach ((string typeName, string memberName, _, _) in promotedStructs)
                result.Append($"\r\n{TypeElemIndent}public partial ref {typeName} {memberName} => ref Ꮡʗ{memberName}.val;");

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Field Accessors");

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                foreach ((string typeName, string memberName) in getStructMembers(promotedStructType))
                    result.Append($"\r\n{TypeElemIndent}public ref {typeName} {memberName} => ref {GetSimpleName(promotedStructType)}.{memberName};");
            }

            result.Append($"\r\n\r\n{TypeElemIndent}// Promoted Struct Method References");

            // Get all extension methods for the struct, any directly defined receivers
            // take precedence over promoted struct methods that have the same name
            IEnumerable<MethodInfo>? structMethods = getStructDeclaration(FullyQualifiedStructType)?.GetExtensionMethods(Context);
            HashSet<string> structMethodNames = new(structMethods?.Select(method => method.Name) ?? [], StringComparer.Ordinal);

            foreach ((string promotedStructType, _, _, _) in promotedStructs)
            {
                IEnumerable<MethodInfo>? promotedStructMethods = getStructDeclaration(promotedStructType)?.GetExtensionMethods(Context);

                foreach (MethodInfo method in promotedStructMethods ?? [])
                {
                    if (structMethodNames.Contains(method.Name))
                    {
                        result.Append($"\r\n{TypeElemIndent}// '{GetSimpleName(promotedStructType)}.{method.Name}' method mapped to overridden '{StructName}' receiver method");
                        continue;
                    }

                    result.Append($"\r\n{TypeElemIndent}public {method.ReturnType} {method.Name}(");
                    result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => $"{param.type} {param.name}")));
                    result.Append($") => {GetSimpleName(promotedStructType)}.{method.Name}(");
                    result.Append(string.Join(", ", method.Parameters.Skip(1).Select(param => param.name)));
                    result.Append(");");
                }
            }

            return result.ToString();

            StructDeclarationSyntax? getStructDeclaration(string structTypeName)
            {
                return Context
                    .Compilation
                    .SyntaxTrees
                    .SelectMany(tree => tree.GetRoot()
                        .DescendantNodes()
                        .OfType<StructDeclarationSyntax>())
                    .FirstOrDefault(structDecl =>
                    {
                        ISymbol? symbol = Context
                            .Compilation
                            .GetSemanticModel(structDecl.SyntaxTree)
                            .GetDeclaredSymbol(structDecl);

                        string symbolName = symbol?.ToDisplayString() ?? "";

                        if (!symbolName.StartsWith("global::") && structTypeName.StartsWith("global::"))
                            symbolName = "global::" + symbolName;

                        return symbolName == structTypeName;
                    });
            }

            IEnumerable<(string typeName, string memberName)> getStructMembers(string structTypeName)
            {
                return getStructDeclaration(structTypeName)?
                    .GetStructMembers(Context, true)
                    .Select(item => (item.typeName, item.memberName)) ?? [];
            }
        }
    }

    private string FieldReferences
    {
        get
        {
            if (StructMembers.Count == 0)
                return $"// -- {StructName} has no defined fields";

            StringBuilder result = new();

            foreach ((string typeName, string memberName, _, _) in StructMembers)
            {
                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                result.Append($"public static ref {typeName} Ꮡ{memberName}(ref {StructName} instance) => ref instance.{memberName};");
            }

            return result.ToString();
        }
    }

    private string Constructors
    {
        get
        {
            StringBuilder result = new();

            result.AppendLine($"public {StructName}(NilType _)");
            result.AppendLine($"{TypeElemIndent}{{");

            // Construct from nil
            foreach ((string typeName, string memberName, _, bool isPromotedStruct) in StructMembers)
            {
                result.Append($"{TypeElemIndent}    ");

                result.AppendLine(isPromotedStruct ? 
                    $"Ꮡʗ{memberName} = new ж<{typeName}>(new {typeName}(nil));" : 
                    $"this.{memberName} = default!;");
            }

            result.AppendLine($"{TypeElemIndent}}}");
            result.AppendLine();

            // Construct from fields
            result.Append($"{TypeElemIndent}public {StructName}(");
            result.Append(string.Join(", ", StructMembers.Select(item => $"{item.typeName} {item.memberName} = default!")));
            result.AppendLine(")");
            result.AppendLine($"{TypeElemIndent}{{");

            foreach ((string typeName, string memberName, _, bool isPromotedStruct) in StructMembers)
            {
                result.Append($"{TypeElemIndent}    ");

                result.AppendLine(isPromotedStruct ?
                    $"Ꮡʗ{memberName} = new ж<{typeName}>(new {typeName}(nil));" :
                    $"this.{memberName} = {memberName};");
            }

            result.Append($"{TypeElemIndent}}}");

            return result.ToString();
        }
    }

    private static string GetToStringImplementation((string typeName, string memberName, bool isReferenceType, bool isPromotedStruct) item)
    {
        return item.isReferenceType ? $"{item.memberName}?.ToString() ?? \"<nil>\"" : $"{item.memberName}.ToString()";
    }
}
