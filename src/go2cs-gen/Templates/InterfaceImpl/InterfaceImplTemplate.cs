using System.Collections.Generic;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceImpl;

internal class InterfaceImplTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required string InterfaceName;
    public required bool Promoted;
    public required HashSet<string> Overrides;
    public required List<MethodInfo> Methods;

    public override string TemplateBody =>
        $$"""
             partial struct {{StructName}} : {{InterfaceName}}
             {
                 {{MethodsImplementation}}
                 
                 // Handle comparisons between '{{StructName}}' and '{{GetSimpleName(InterfaceName)}}'
                 public static bool operator ==({{StructName}} src, {{InterfaceName}} iface)
                 {
                     return iface is {{StructName}} val && val == src;
                 }
                 
                 public static bool operator !=({{StructName}} src, {{InterfaceName}} iface)
                 {
                     return !(src == iface);
                 }
                 
                 public static bool operator ==({{InterfaceName}} iface, {{StructName}} src)
                 {
                     return iface is {{StructName}} val && val == src;
                 }
                 
                 public static bool operator !=({{InterfaceName}} iface, {{StructName}} src)
                 {
                     return !(iface == src);
                 }
             }
         """;

    private string MethodsImplementation
    {
        get
        {
            StringBuilder result = new();

            foreach (MethodInfo method in Methods)
            {
                string simpleInterfaceName = GetSimpleName(InterfaceName);
                string simpleMethodName = GetSimpleName(method.Name);
                bool methodOverriden = Overrides.Contains(simpleMethodName);

                if (result.Length > 0)
                    result.Append("\r\n\r\n        ");

                if (Promoted && !methodOverriden)
                {
                    result.Append($"// '{simpleInterfaceName}.{simpleMethodName}' implicit implementation mapped to promoted interface receiver method:\r\n        ");
                    result.Append($"public {method.ReturnType} {method.GetSignature()} => {GetSimpleName(InterfaceName)}.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
                }
                else
                {
                    if (Promoted && methodOverriden)
                    {
                        result.Append($"// '{simpleInterfaceName}.{simpleMethodName}' explicit implementation mapped to direct struct receiver method,\r\n        ");
                        result.Append($"// this overrides promoted interface method '{GetSimpleName(InterfaceName)}.{simpleMethodName}':\r\n        ");
                    }
                    else
                    {
                        result.Append($"// '{simpleInterfaceName}.{simpleMethodName}' explicit implementation mapped to direct struct receiver method:\r\n        ");
                    }

                    result.Append($"{method.ReturnType} {method.GetSignature()} => this.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
                }
            }

            return result.ToString();
        }
    }
}
