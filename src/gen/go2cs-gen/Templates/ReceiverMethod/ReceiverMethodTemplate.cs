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
    public required string Options;

    private bool? m_capturePointer;
    private bool CapturePointer => m_capturePointer ??= string.Equals(Options, "capture", StringComparison.OrdinalIgnoreCase);

    private string? m_receiverParamName;
    private string ReceiverParamName => m_receiverParamName ??= Method.Parameters.First().name;

    private string? m_receiverParamType;
    private string ReceiverParamType => m_receiverParamType ??= $"{PointerPrefix}<{Method.Parameters.First().type}>";

    public override string Generate()
    {
        const string ThreadingUsing = "using System.Threading;";

        UsingStatements = UsingStatements is null ?
            [ThreadingUsing] :
            UsingStatements.Concat([ThreadingUsing]).ToArray();

        return base.Generate();
    }

    public override string TemplateBody =>
        $$"""
        {{CaptureDeclarations}}    [{{GeneratedCodeAttribute}}]
            {{TargetScope}} static {{Method.ReturnType}} {{Method.Name}}{{Method.GetGenericSignature()}}({{DeclParams}}){{Method.GetWhereConstraints()}}
            {{{CaptureOperation}}
                ref var {{ReceiverParamName}} = ref {{AddressPrefix}}{{ReceiverParamName}}.val;
                {{ReturnStatement}}{{ReceiverParamName}}.{{Method.Name}}({{CallParams}});
            }
        """;

    private string CaptureDeclarations => !CapturePointer ? "" :
        $"""
            private static ThreadLocal<{ReceiverParamType}>? {Method.Name}{CapturedVarMarker}{ReceiverParamName};
            private static {ReceiverParamType} {Method.Name}{TypeAliasDot}{AddressPrefix}{ReceiverParamName} => {Method.Name}{CapturedVarMarker}{ReceiverParamName}?.Value ??
                throw new NullReferenceException("Receiver target \"{Method.Name}{TypeAliasDot}{AddressPrefix}{ReceiverParamName}\" is not initialized");
        
        
        """;

    private string CaptureOperation => !CapturePointer ? "" :
        $"""
    
                {Method.Name}{CapturedVarMarker}{ReceiverParamName} = new ThreadLocal<{ReceiverParamType}>(() => {AddressPrefix}{ReceiverParamName});
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
                    result.Add($"this {PointerPrefix}<{type}> {AddressPrefix}{name}");
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
            string receiverScope = char.IsUpper(GetSimpleName(Method.Parameters[0].type)[0]) ? "public" : "internal";
            return Scope == receiverScope ? Scope : "internal";
        }
    }
}
