using System;
using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.ReceiverMethod;

internal class ReceiverMethodTemplate : TemplateBase
{
    // Template Parameters
    public required MethodInfo Method;

    private string? m_receiverParamName;
    private string ReceiverParamName => m_receiverParamName ??= Method.Parameters.First().name;

    // The heap-box parameter/local name (`Ꮡx`). Built from the UNescaped receiver name: a C#-keyword
    // receiver is escaped as `@enum`, but `Ꮡ@enum` is invalid ('@' is only valid as a leading prefix).
    // The `Ꮡ` prefix already yields a distinct, valid identifier, so strip the '@' → `Ꮡenum`.
    private string? m_receiverBoxName;
    private string ReceiverBoxName => m_receiverBoxName ??= $"{AddressPrefix}{GetUnsanitizedIdentifier(ReceiverParamName)}";

    private string? m_receiverParamType;
    private string ReceiverParamType => m_receiverParamType ??= $"{PointerPrefix}<{Method.Parameters.First().type}>";

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{TargetScope}} static {{Method.ReturnType}} {{Method.Name}}{{Method.GetGenericSignature()}}({{DeclParams}}){{Method.GetWhereConstraints()}}
            {
                ref var {{ReceiverParamName}} = ref {{ReceiverBoxName}}.Value;
                {{ReturnStatement}}{{ReceiverParamName}}.{{Method.Name}}({{CallParams}});
            }
        """;

    private string DeclParams
    {
        get
        {
            List<string> result = [];
            bool first = true;

            foreach ((string type, string name) in Method.Parameters)
            {
                if (first)
                {
                    result.Add($"this {PointerPrefix}<{type}> {ReceiverBoxName}");
                    first = false;
                }
                else
                {
                    result.Add($"{type} {name}");
                }
            }

            return string.Join(", ", result);
        }
    }

    private string ReturnStatement =>
        Method.ReturnType == "void" ? "" : "return ";

    private string CallParams => 
        string.Join(", ", Method.Parameters.Skip(1).Select(item => item.name));

    private string TargetScope
    {
        get
        {
            string receiverScope = GetScope(GetSimpleName(Method.Parameters[0].type));
            return Scope == receiverScope ? Scope : "internal";
        }
    }
}
