using System.Linq;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceType;

internal class InterfaceTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string InterfaceName;
    public required string[] OperatorConstraints;

    public override string TemplateBody =>
        $$"""
              [{{GeneratedCodeAttribute}}]
              {{Scope}} partial interface {{InterfaceName}}{{AppliedOperatorConstraints}}
              {
              }
          """;

    private string AppliedOperatorConstraints
    {
        get
        {
            if (OperatorConstraints.Length == 0)
                return string.Empty;

            StringBuilder implementation = new();
            string constraints = string.Join(",\r\n", OperatorConstraints.Select(GetConstraintName));

            implementation.AppendLine(" :");
            implementation.AppendLine(constraints);
            implementation.AppendLine("        where T :");
            implementation.Append(constraints);

            return implementation.ToString();
        }
    }

    private static string GetConstraintName(string name)
    {
        return "        " + name switch
        {
            "Sum" => "runtime.SumOperator<T>",
            "Arithmetic" => "runtime.ArithmeticOperators<T>",
            "Integer" => "runtime.IntegerOperators<T>",
            "Comparable" => "comparable<T>",
            "Ordered" => "runtime.OrderedOperators<T>",
            _ => name
        };
    }
}
