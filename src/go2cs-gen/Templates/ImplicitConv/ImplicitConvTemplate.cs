using System;
using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;

namespace go2cs.Templates.ImplicitConv;

internal class ImplicitConvTemplate : TemplateBase
{
    // Template Parameters
    public required string SourceTypeName;
    public required string TargetTypeName;
    public required List<(string typeName, string memberName)> StructMembers;

    public override string TemplateBody =>
        $$"""
             partial struct {{SourceTypeName}}
             {
                 public static implicit operator {{TargetTypeName}}({{SourceTypeName}} src)
                 {
                     return {{ConvExpr}};
                 }
             }    
         """;

    private string ConvExpr
    {
        get
        {
            if (!PointerExpr.IsMatch(TargetTypeName))
                return $"new {TargetTypeName}({ParamList})";
            
            int ltIndex = TargetTypeName.IndexOf('<');
            int gtIndex = TargetTypeName.LastIndexOf('>');

            if (gtIndex > ltIndex)
                return $"Ꮡ(new {TargetTypeName[(ltIndex + 1)..gtIndex]}({ParamList}))";

            throw new FormatException($"Unexpected target type name \"{TargetTypeName}\"");
        }
    }

    private string ParamList => string.Join(", ", StructMembers.Select(GetParamExpr));

    private static string GetParamExpr((string typeName, string memberName) member)
    {
        (string typeName, string memberName) = member;
        return PointerExpr.IsMatch(typeName) ? $"src.{memberName}?.val ?? default!" : $"src.{memberName}";
    }
}
