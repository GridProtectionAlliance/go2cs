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

    // Single embedded-pointer hop property (`Type` for rtype's `*abi.Type`): an interface member
    // with no direct struct method forwards through it (`this.Type.Value.M()`), matching the
    // converter's syntax-resolved promotion at Go call sites. Null when no (single) hop exists.
    public string? EmbedHop;

    // Hop-target methods that are direct-ж primaries (extensions on ж<X> with no ref twin) bind
    // the box field itself — `this.File.Read(p)`; deref'ing first strands the receiver (CS1929).
    public HashSet<string> EmbedHopBoxMethods = [];

    public override string TemplateBody =>
        $$"""
             partial struct {{StructName}} : {{InterfaceName}}
             {
                 {{MethodsImplementation}}{{Comparisions}}
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
                    result.Append($"// '{simpleInterfaceName}.{simpleMethodName}()' implicit implementation mapped to promoted interface receiver method:\r\n        ");
                    result.Append($"public {method.ReturnType} {method.GetSignature()} => {GetSimpleName(InterfaceName)}.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
                }
                else
                {
                    if (Promoted && methodOverriden)
                    {
                        result.Append($"// '{simpleInterfaceName}.{simpleMethodName}()' explicit implementation mapped to direct struct receiver method,\r\n        ");
                        result.Append($"// this overrides promoted interface method '{GetSimpleName(InterfaceName)}.{simpleMethodName}':\r\n        ");
                    }
                    else
                    {
                        result.Append($"// '{simpleInterfaceName}.{simpleMethodName}()' explicit implementation mapped to direct struct receiver method:\r\n        ");
                    }

                    string receiver = !methodOverriden && EmbedHop is not null ?
                        EmbedHopBoxMethods.Contains(simpleMethodName) ? $"this.{EmbedHop}" : $"this.{EmbedHop}.Value" :
                        "this";

                    result.Append($"{method.ReturnType} {method.GetSignature()} => {receiver}.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
                }
            }

            return result.ToString();
        }
    }

    private string Comparisions
    {
        get
        {
            // Operators can only be public
            return OperatorScope != "public" ? 
                string.Empty : 
                $"""
                
                
                        // Handle comparisons between struct '{StructName}' and interface '{GetSimpleName(InterfaceName)}'
                        public static bool operator ==({StructName} src, {InterfaceName} iface) => iface is {StructName} val && val == src;
                        
                        public static bool operator !=({StructName} src, {InterfaceName} iface) => !(src == iface);
                        
                        public static bool operator ==({InterfaceName} iface, {StructName} src) => iface is {StructName} val && val == src;
                        
                        public static bool operator !=({InterfaceName} iface, {StructName} src) => !(iface == src);
                """;
        }
    }

    private string OperatorScope
    {
        get
        {
            string structNameScope = GetScope(StructName);
            string interfaceNameScope = GetScope(GetSimpleName(InterfaceName));
            return structNameScope == interfaceNameScope ? structNameScope : "internal";
        }
    }
}
