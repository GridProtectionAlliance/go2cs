using System.Collections.Generic;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceImpl;

internal class InterfaceImplTemplate : TemplateBase
{
    // Template Parameters
    public required string InterfaceName;
    public required string StructName;
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
                    result.AppendLine("        ");

                string genericSignature = getGenericSignature(method);
                result.Append($"{method.ReturnType} {InterfaceName}.{method.Name}{genericSignature}({method.TypedParameters}) => this.{method.Name}{genericSignature}({method.Parameters});");
            }

            return result.ToString();

            string getGenericSignature(MethodInfo method)
            {
                return method.IsGeneric ? $"<{method.TypeParameters}>" : "";
            }
        }
    }
}
