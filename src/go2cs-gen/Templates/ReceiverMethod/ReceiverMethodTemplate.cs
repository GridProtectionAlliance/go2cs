using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;

namespace go2cs.Templates.ReceiverMethod;

internal class ReceiverMethodTemplate : TemplateBase
{
    // Template Parameters
    public required string MethodName;
    public required string ReturnType;
    public required List<(string typeName, string paramName)> MethodParameters;
    private string? m_receiverParamName;

    public override string TemplateBody =>
        $$"""
              [{{GeneratedCodeAttribute}}]
              {{Scope}} static {{ReturnType}} {{MethodName}}({{DeclParams}})
              {
                  ref var {{ReceiverParamName}} = ref Ꮡ{{ReceiverParamName}}.val;
                  return {{ReceiverParamName}}.{{MethodName}}({{CallParams}});
              }
          """;

    private string DeclParams
    {
        get
        {
            List<string> result = [];
            bool first = true;

            foreach ((string typeName, string paramName) in MethodParameters)
            {
                if (first)
                {
                    result.Add($"this ptr<{typeName}> Ꮡ{paramName}");
                    first = false;
                }
                else
                {
                    result.Add($"{typeName} {paramName}");
                }
            }

            return string.Join(", ", result);
        }
    }

    private string ReceiverParamName =>
        m_receiverParamName ??= MethodParameters.First().paramName;

    private string CallParams => 
        string.Join(", ", MethodParameters.Skip(1).Select(item => item.paramName));
}
