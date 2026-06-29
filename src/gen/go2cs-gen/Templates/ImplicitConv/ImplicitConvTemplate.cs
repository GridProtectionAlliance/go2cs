using System;
using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.ImplicitConv;

internal class ImplicitConvTemplate : TemplateBase
{
    // Template Parameters
    public required string SourceTypeName;
    public required string TargetTypeName;
    public required bool Inverted;
    public required bool Indirect;
    public required string? ValueType;
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
            {
                string innerType = LHTypeName[(ltIndex + 1)..gtIndex];

                // Self-boxing (`T -> ж<T>`): the source value IS the boxed element type, so box it
                // directly. Reconstructing it field-by-field would wrongly deref pointer fields
                // (`src.f?.val`) whose target ctor parameter is itself a `ж<…>` pointer field — the
                // value cannot bind the pointer parameter (CS1503). `Ꮡ(src)` boxes a copy of the
                // whole struct, identical in effect but without the per-field deref.
                if (innerType == RHTypeName)
                    return $"{AddressPrefix}(src)";

                return $"{AddressPrefix}(new {innerType}({ParamList}))";
            }

            throw new FormatException($"Unexpected target type name \"{LHTypeName}\"");
        }
    }

    private string ParamList => string.IsNullOrWhiteSpace(ValueType) ? 
        string.Join(", ", StructMembers.Select(GetParamExpr)) : 
        $"({ValueType})src.val";

    private string GetParamExpr((string, string) member)
    {
        (string typeName, string memberName) = member;
        return Indirect && PointerExpr.IsMatch(typeName) ? $"src.{memberName}?.val ?? default!" : $"src.{memberName}";
    }
}
