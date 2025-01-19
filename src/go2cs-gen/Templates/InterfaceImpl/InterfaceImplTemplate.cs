using System.Collections.Generic;
using System.Text;

namespace go2cs.Templates.InterfaceImpl;

internal class InterfaceImplTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required string InterfaceName;
    public required List<MethodInfo> Methods;

    public override string TemplateBody =>
        $$"""
             partial struct {{StructName}} : {{InterfaceName}}
             {
                 {{MethodsImplementation}}
             }
         """;

    private string MethodsImplementation
    {
        get
        {
            StringBuilder result = new();

            foreach (MethodInfo method in Methods)
            {
                if (result.Length > 0)
                    result.Append("\r\n        ");

                result.Append($"{method.ReturnType} {method.GetSignature()} => this.{GetSimpleName(method.Name)}{method.GetGenericSignature()}({method.CallParameters});");
            }

            return result.ToString();
        }
    }

    private static string GetSimpleName(string name)
    {
        string[] parts = name.Split('.');
        return parts[^1];
    }
}
