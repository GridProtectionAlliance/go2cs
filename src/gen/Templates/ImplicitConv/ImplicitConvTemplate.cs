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
    public required bool Inverted;
    public required bool Indirect;
    public required List<(string typeName, string memberName)> StructMembers;

    public override string TemplateBody =>
        $$"""
             partial struct {{SourceTypeName}}
             {
                 public static implicit operator {{LHTypeName}}({{RHTypeName}} src) => {{ConvExpr}};
             }    
         """;

    public string LHTypeName => Inverted ? SourceTypeName : TargetTypeName;

    public string RHTypeName => Inverted ? TargetTypeName : SourceTypeName;

    private string ConvExpr
    {
        get
        {
            if (!PointerExpr.IsMatch(LHTypeName))
                return $"new {LHTypeName}({ParamList})";
            
            int ltIndex = LHTypeName.IndexOf('<');
            int gtIndex = LHTypeName.LastIndexOf('>');

            if (gtIndex > ltIndex)
                return $"Ꮡ(new {LHTypeName[(ltIndex + 1)..gtIndex]}({ParamList}))";

            throw new FormatException($"Unexpected target type name \"{LHTypeName}\"");
        }
    }

    private string ParamList => string.Join(", ", StructMembers.Select(GetParamExpr));

    private string GetParamExpr((string, string) member)
    {
        (string typeName, string memberName) = member;
        return Indirect && PointerExpr.IsMatch(typeName) ? $"src.{memberName}?.val ?? default!" : $"src.{memberName}";
    }
}
