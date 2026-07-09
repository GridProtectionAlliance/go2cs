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

    // Methods declared on a VALUE-embedded field of the hop type with a POINTER receiver bind a
    // projected field box — `this.TCPConn.of(TCPConn.Ꮡconn).Read(p)` (net CS1929 x2); the map
    // carries the `.of(…)` suffix per method name.
    public Dictionary<string, string>? EmbedHopDeepPaths;

    // Single VALUE-embedded field (`addrPortUDPAddr struct { netip.AddrPort }`): an interface
    // member with no direct struct method promotes through it (`this.AddrPort.String()`).
    public string? ValueEmbedHop;

    // Non-null when the value embed's type is FOREIGN (dotted) - the extension lives in another
    // namespace segment the file only aliases, so the forwarding calls the package-class static
    // directly: `global::go.net.netip_package.String(this.AddrPort)`.
    public string? ValueEmbedHopStaticClass;

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
                    // The forwarding receiver is the embedded interface FIELD, which carries the
                    // Go embed name — the Δ-stripped simple name when the interface TYPE was
                    // collision-renamed (bare `ΔHandler.Enabled(…)` binds nothing, CS0103 —
                    // slogtest's `wrapper` embeds slog.ΔHandler as field `Handler`).
                    result.Append($"// '{simpleInterfaceName}.{simpleMethodName}()' implicit implementation mapped to promoted interface receiver method:\r\n        ");
                    result.Append($"public {method.ReturnType} {method.GetSignature()} => {GetSimpleName(InterfaceName, dropCollisionPrefix: true)}.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
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

                    string receiver = "this";

                    if (!methodOverriden && EmbedHop is not null)
                    {
                        if (EmbedHopDeepPaths is not null && EmbedHopDeepPaths.TryGetValue(simpleMethodName, out string? deepPath))
                            receiver = $"this.{EmbedHop}{deepPath}";
                        else
                            receiver = EmbedHopBoxMethods.Contains(simpleMethodName) ? $"this.{EmbedHop}" : $"this.{EmbedHop}.Value";
                    }
                    else if (!methodOverriden && ValueEmbedHop is not null)
                    {
                        if (ValueEmbedHopStaticClass is not null)
                        {
                            string staticArgs = string.IsNullOrEmpty(method.CallParameters) ? $"this.{ValueEmbedHop}" : $"this.{ValueEmbedHop}, {method.CallParameters}";
                            result.Append($"{method.ReturnType} {method.GetSignature()} => {ValueEmbedHopStaticClass}.{simpleMethodName}{method.GetGenericSignature()}({staticArgs});");
                            continue;
                        }

                        receiver = $"this.{ValueEmbedHop}";
                    }

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
