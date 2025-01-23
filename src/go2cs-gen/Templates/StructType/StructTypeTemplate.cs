using System.Collections.Generic;
using System.Linq;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.StructType;

internal class StructTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required List<(string typeName, string fieldName, bool isReferenceType)> StructFields;

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial struct {{StructName}}
            {
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
                    {{(StructFields.Count > 0 ? string.Join(", ", StructFields.Select(GetToStringImplementation)) : "\"\"")}}
                ]), "}");
            }
        """;

    private string FieldReferences
    {
        get
        {
            StringBuilder result = new();

            foreach ((string typeName, string fieldName, _) in StructFields)
            {
                if (result.Length > 0)
                    result.Append($"\r\n{TypeElemIndent}");

                result.Append($"public static ref {typeName} Ꮡ{fieldName}(ref {StructName} instance) => ref instance.{fieldName};");
            }

            if (result.Length == 0)
                result.Append($"// -- {StructName} has no defined fields");

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
            foreach ((_, string fieldName, _) in StructFields)
            {
                result.Append($"{TypeElemIndent}    ");

                // TODO: Handle promoted struct field
                // if (PrototedStructs.Contains(fieldName)
                //     result.AppendLine($"this.m_{fieldName}Ref = new ptr<{typeName}>(new {typeName}(nil));");
                // else

                result.AppendLine($"this.{fieldName} = default;");
            }

            result.AppendLine($"{TypeElemIndent}}}");
            result.AppendLine();

            // Construct from fields
            result.Append($"{TypeElemIndent}public {StructName}(");
            result.Append(string.Join(", ", StructFields.Select(item => $"{item.typeName} {item.fieldName} = default")));
            result.AppendLine(")");
            result.AppendLine($"{TypeElemIndent}{{");

            foreach ((_, string fieldName, _) in StructFields)
            {
                result.Append($"{TypeElemIndent}    ");

                // TODO: Handle promoted struct field
                // if (PrototedStructs.Contains(fieldName)
                //     result.AppendLine($"this.m_{fieldName}Ref = new ptr<{typeName}>(new {typeName}({fieldName}));");
                // else

                result.AppendLine($"this.{fieldName} = {fieldName};");
            }

            result.Append($"{TypeElemIndent}}}");

            return result.ToString();
        }
    }

    private string GetToStringImplementation((string _, string fieldName, bool isReferenceType) item)
    {
        (_, string fieldName, bool isReferenceType) = item;
        return isReferenceType ? $"{fieldName}?.ToString() ?? \"<nil>\"" : $"{fieldName}.ToString()";
    }
}
