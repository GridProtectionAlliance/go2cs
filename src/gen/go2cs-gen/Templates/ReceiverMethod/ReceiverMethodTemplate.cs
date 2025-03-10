using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;

namespace go2cs.Templates.ReceiverMethod;

internal class ReceiverMethodTemplate : TemplateBase
{
    // Template Parameters
    public required MethodInfo Method;
    private string? m_receiverParamName;

    public override string TemplateBody =>
        $$"""
              [{{GeneratedCodeAttribute}}]
              {{TargetScope}} static {{Method.ReturnType}} {{Method.Name}}{{Method.GetGenericSignature()}}({{DeclParams}}){{Method.GetWhereConstraints()}}
              {
                  ref var {{ReceiverParamName}} = ref Ꮡ{{ReceiverParamName}}.val;
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
                    result.Add($"this ж<{type}> Ꮡ{name}");
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

    private string ReceiverParamName =>
        m_receiverParamName ??= Method.Parameters.First().name;

    private string CallParams => 
        string.Join(", ", Method.Parameters.Skip(1).Select(item => item.name));

    private string TargetScope
    {
        get
        {
            string receiverScope = char.IsUpper(GetSimpleName(Method.Parameters[0].name)[0]) ? "public" : "internal";
            return Scope == receiverScope ? Scope : "internal";
        }
    }
}
