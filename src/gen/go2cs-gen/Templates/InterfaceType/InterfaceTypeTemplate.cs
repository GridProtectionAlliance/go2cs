using System.Linq;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceType;

internal class InterfaceTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string InterfaceName;
    public required string[] OperatorConstraints;
    public required MethodInfo[] Methods;
    public required bool Runtime;
    private string? m_nonGenericInterfaceName;
    private string? m_conversionTypeName;
    private string? m_nonGenericConversionTypeName;
    private string? m_implementedInterfaceName;

    private const string Indent = "        ";

    // Define a type T variables that will not conflict with any method generic types
    private const string TypeT = $"{ShadowVarMarker}T";
    private const string TypeTTarget = $"{TypeT}Target";

    public override string Generate()
    {
        string[] RequiredUsings = ["using System.Diagnostics;", "using System.Reflection;", "using go.runtime;"];

        UsingStatements = UsingStatements is null ?
            RequiredUsings :
            UsingStatements.Concat(RequiredUsings).ToArray();

        return base.Generate();
    }

    public override string TemplateBody =>
        $$"""
              [{{GeneratedCodeAttribute}}]
              {{Scope}} partial interface {{InterfaceName}}{{AppliedOperatorConstraints}}
              {{{RuntimeInterfaceConversionMethods}}
              }{{RuntimeInterfaceConversionType}}
          """;

    private string AppliedOperatorConstraints
    {
        get
        {
            if (OperatorConstraints.Length == 0)
                return string.Empty;

            StringBuilder implementation = new();
            string constraints = string.Join(",\r\n", OperatorConstraints.SelectMany(GetConstraintName));

            implementation.AppendLine(" :");
            implementation.AppendLine(constraints);
            implementation.AppendLine($"{Indent}where {TypeT} :");
            implementation.Append(constraints);

            return implementation.ToString();
        }
    }

    private static string[] GetConstraintName(string name)
    {
        return name switch
        {
            "Sum" => [$"{Indent}IAdditionOperators<{TypeT}, {TypeT}, {TypeT}>"],
            "Arithmetic" => [$"{Indent}ISubtractionOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IMultiplyOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IDivisionOperators<{TypeT}, {TypeT}, {TypeT}>"],
            "Integer" => [$"{Indent}IModulusOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IBitwiseOperators<{TypeT}, {TypeT}, {TypeT}>", $"{Indent}IShiftOperators<{TypeT}, {TypeT}, {TypeT}>"],
            "Comparable" => [$"{Indent}IEqualityOperators<{TypeT}, {TypeT}, bool>"],
            "Ordered" => [$"{Indent}IComparisonOperators<{TypeT}, {TypeT}, bool>"],
            _ => [$"{Indent}{name}"]
        };
    }

    private string ConversionTypeName => m_conversionTypeName ??= GetConversionTypeName();

    private string GetConversionTypeName()
    {
        string interfaceName = InterfaceName;

        return interfaceName.EndsWith(">") ? 
            $"{NonGenericConversionTypeName}<{TypeTTarget}>" : 
            $"{ShadowVarMarker}{interfaceName}<{TypeTTarget}>";
    }

    private string NonGenericConversionTypeName => m_nonGenericConversionTypeName ??= $"{ShadowVarMarker}{NonGenericInterfaceName}";

    private string NonGenericInterfaceName => m_nonGenericInterfaceName ??= GetNonGenericInterfaceName();

    private string GetNonGenericInterfaceName()
    {
        int startIndex = InterfaceName.IndexOf('<');
        return startIndex < 0 ? InterfaceName : InterfaceName[..startIndex];
    }

    private string ImplementedInterfaceName => m_implementedInterfaceName ??= GetImplementedInterfaceName();

    private string GetImplementedInterfaceName()
    {
        int startIndex = InterfaceName.IndexOf('<');
        return startIndex < 0 ? InterfaceName : $"{NonGenericInterfaceName}<{ConversionTypeName}>";
    }

    private string RuntimeInterfaceConversionMethods
    {
        get
        {
            return !Runtime ? "" : 
                $"""
                
                        // Runtime interface conversion methods
                        public static {ImplementedInterfaceName} As<{TypeTTarget}>(in {TypeTTarget} target) =>
                            ({ConversionTypeName})target!;
                
                        public static {ImplementedInterfaceName} As<{TypeTTarget}>({PointerPrefix}<{TypeTTarget}> target_ptr) =>
                            ({ConversionTypeName})target_ptr;
                """ +
                (InterfaceName == NonGenericInterfaceName ? 
                $"""
                
                
                        public static {ImplementedInterfaceName}? As(object target) =>
                            typeof({NonGenericConversionTypeName}<>).CreateInterfaceHandler<{InterfaceName}>(target);            
                """ : "");
        }
    }

    private string RuntimeInterfaceConversionType
    {
        get
        {
            return !Runtime ? "" :
                $$"""
                
                
                    // Defines a runtime type for duck-typed interface implementations based on existing
                    // extension methods that satisfy interface. This class is only used as fallback for
                    // when the interface was not able to be implemented at transpile time, e.g., with
                    // dynamically declared anonymous interfaces used with type assertions.
                    [{{GeneratedCodeAttribute}}]
                    {{Scope}} class {{ConversionTypeName}} : {{ImplementedInterfaceName}}
                    {
                        private {{TypeTTarget}} m_target = default!;
                        private readonly {{PointerPrefix}}<{{TypeTTarget}}>? m_target_ptr;
                        private readonly bool m_target_is_ptr;
                    
                        public ref {{TypeTTarget}} Target
                        {
                            get
                            {
                                if (m_target_is_ptr && m_target_ptr is not null)
                                    return ref m_target_ptr.val;
                    
                                return ref m_target;
                            }
                        }
                    
                        public {{NonGenericConversionTypeName}}(in {{TypeTTarget}} target)
                        {
                            m_target = target;
                        }
                    
                        public {{NonGenericConversionTypeName}}({{PointerPrefix}}<{{TypeTTarget}}> target_ptr)
                        {
                            m_target_ptr = target_ptr;
                            m_target_is_ptr = true;
                        }
                        {{ReceiverMethodImplementations}}
                    
                        static {{NonGenericConversionTypeName}}()
                        {
                            Type targetType = typeof({{TypeTTarget}});
                            Type targetTypeByPtr = typeof({{PointerPrefix}}<{{TypeTTarget}}>);
                            MethodInfo? extensionMethod;
                            {{ReceiverMethodInitializations}}
                        }
                    
                        public static explicit operator {{ConversionTypeName}}(in {{PointerPrefix}}<{{TypeTTarget}}> target_ptr) => new(target_ptr);
                    
                        public static explicit operator {{ConversionTypeName}}(in {{TypeTTarget}} target) => new(target);
                
                        public override int GetHashCode() => Target?.GetHashCode() ?? 0;
                
                        public static bool operator ==({{ConversionTypeName}}? left, {{ConversionTypeName}}? right) => left?.Equals(right) ?? right is null;
                        
                        public static bool operator !=({{ConversionTypeName}}? left, {{ConversionTypeName}}? right) => !(left == right);
                
                        #region [ Operator Constraint Implementations ]
                
                        // These operator constraints exist to satisfy possible constraints defined on source interface,
                        // however, the instance of this class is only used to implement the interface methods, so these
                        // operators are only placeholders and not actually functional.
                
                        public static bool operator <({{ConversionTypeName}} left, {{ConversionTypeName}} right) => false;
                        
                        public static bool operator <=({{ConversionTypeName}} left, {{ConversionTypeName}} right) => false;
                        
                        public static bool operator >({{ConversionTypeName}} left, {{ConversionTypeName}} right) => false;
                        
                        public static bool operator >=({{ConversionTypeName}} left, {{ConversionTypeName}} right) => false;
                        
                        public static {{ConversionTypeName}} operator +({{ConversionTypeName}} left, {{ConversionTypeName}} right) => default!;
                        
                        public static {{ConversionTypeName}} operator -({{ConversionTypeName}} left, {{ConversionTypeName}} right) => default!;
                        
                        public static {{ConversionTypeName}} operator -({{ConversionTypeName}} value) => default!;
                        
                        public static {{ConversionTypeName}} operator *({{ConversionTypeName}} left, {{ConversionTypeName}} right) => default!;
                        
                        public static {{ConversionTypeName}} operator /({{ConversionTypeName}} left, {{ConversionTypeName}} right) => default!;
                        
                        public static {{ConversionTypeName}} operator %({{ConversionTypeName}} left, {{ConversionTypeName}} right) => default!;
                
                        public static {{ConversionTypeName}} operator &({{ConversionTypeName}} left, {{ConversionTypeName}} right) => default!;
                        
                        public static {{ConversionTypeName}} operator |({{ConversionTypeName}} left, {{ConversionTypeName}} right) => default!;
                        
                        public static {{ConversionTypeName}} operator ^({{ConversionTypeName}} left, {{ConversionTypeName}} right) => default!;
                        
                        public static {{ConversionTypeName}} operator ~({{ConversionTypeName}} value) => default!;
                        
                        public static {{ConversionTypeName}} operator <<({{ConversionTypeName}} value, {{ConversionTypeName}} shiftAmount) => default!;
                        
                        public static {{ConversionTypeName}} operator >>({{ConversionTypeName}} value, {{ConversionTypeName}} shiftAmount) => default!;
                        
                        public static {{ConversionTypeName}} operator >>>({{ConversionTypeName}} value, {{ConversionTypeName}} shiftAmount) => default!;
                        
                        #endregion
                    
                        // Enable comparisons between nil and {{ConversionTypeName}} interface instance
                        public static bool operator ==({{ConversionTypeName}} value, NilType nil) => Activator.CreateInstance<{{ConversionTypeName}}>().Equals(value);
                    
                        public static bool operator !=({{ConversionTypeName}} value, NilType nil) => !(value == nil);
                    
                        public static bool operator ==(NilType nil, {{ConversionTypeName}} value) => value == nil;
                    
                        public static bool operator !=(NilType nil, {{ConversionTypeName}} value) => value != nil;
                    }
                """;
        }
    }

    private string ReceiverMethodImplementations
    {
        get
        {
            StringBuilder results = new();

            foreach (MethodInfo method in Methods)
                results.Append(GetReceiverMethodImplementation(method));

            return results.ToString();
        }
    }

    private string GetReceiverMethodImplementation(MethodInfo method)
    {
        return $$"""
                     
                         // Implementation for '{{NonGenericInterfaceName}}.{{method.Name}}' receiver method 
                         private delegate {{method.ReturnType}} {{method.Name}}ByPtr{{method.GetGenericSignature()}}({{PointerPrefix}}<{{TypeTTarget}}> target{{CapturedVarMarker}}{{getCommaPrefixedTypedParameters()}}){{method.GetWhereConstraints()}};
                         private delegate {{method.ReturnType}} {{method.Name}}ByVal{{method.GetGenericSignature()}}({{TypeTTarget}} target{{CapturedVarMarker}}{{getCommaPrefixedTypedParameters()}}){{method.GetWhereConstraints()}};
                         
                         private static readonly {{method.Name}}ByPtr? s_{{method.Name}}ByPtr;
                         private static readonly {{method.Name}}ByVal? s_{{method.Name}}ByVal;
                         
                         [DebuggerNonUserCode]
                         public {{method.ReturnType}} {{method.GetSignature()}}
                         {
                             {{TypeTTarget}} target = m_target;
                         
                             if (m_target_is_ptr && m_target_ptr is not null)
                                 target = m_target_ptr.val;
                         
                             if (s_{{method.Name}}ByPtr is null || !m_target_is_ptr)
                                 {{getReturnStatement()}}s_{{method.Name}}ByVal!(target{{getCommaPrefixedCallParameters()}});
                         
                             {{getReturnStatement()}}s_{{method.Name}}ByPtr(m_target_ptr!{{getCommaPrefixedCallParameters()}});
                         }
                 """;

        string getCommaPrefixedTypedParameters()
        {
            string typeParameters = method.TypedParameters;

            if (typeParameters.Length > 0)
                typeParameters = $", {typeParameters}";

            return typeParameters;
        }

        string getCommaPrefixedCallParameters()
        {
            string callParameters = method.CallParameters;

            if (callParameters.Length > 0)
                callParameters = $", {callParameters}";

            return callParameters;
        }

        string getReturnStatement()
        {
            return method.ReturnType == "void" ? "" : "return ";
        }
    }

    private string ReceiverMethodInitializations
    {
        get
        {
            StringBuilder results = new();

            foreach (MethodInfo method in Methods)
                results.Append(GetReceiverMethodInitialization(method));

            return results.ToString();
        }
    }

    private string GetReceiverMethodInitialization(MethodInfo method)
    {
        return $$"""
                     
                             // Initialization of '{{NonGenericInterfaceName}}.{{method.Name}}' receiver method implementation
                             extensionMethod = targetTypeByPtr.GetExtensionMethod(nameof({{method.Name}}));
                             
                             if (extensionMethod is not null)
                                 s_{{method.Name}}ByPtr = extensionMethod.CreateStaticDelegate(typeof({{method.Name}}ByPtr)) as {{method.Name}}ByPtr;
                             
                             extensionMethod = targetType.GetExtensionMethod(nameof({{method.Name}}));
                             
                             if (extensionMethod is not null)
                                 s_{{method.Name}}ByVal = extensionMethod.CreateStaticDelegate(typeof({{method.Name}}ByVal)) as {{method.Name}}ByVal;
                             
                             if (s_{{method.Name}}ByPtr is null && s_{{method.Name}}ByVal is null)
                                 throw new NotImplementedException($"{targetType.FullName} does not implement '{{NonGenericInterfaceName}}.{nameof({{method.Name}})}' method");
                 """;
    }
}
