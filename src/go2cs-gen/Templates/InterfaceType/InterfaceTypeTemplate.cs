using System.Linq;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceType;

internal class InterfaceTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string InterfaceName;
    public required string[] OperatorConstraints;

    private const string Indent = "        ";

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
            string constraints = string.Join(",\r\n", OperatorConstraints.SelectMany(GetConstraintName));

            implementation.AppendLine(" :");
            implementation.AppendLine(constraints);
            implementation.AppendLine($"{Indent}where T :");
            implementation.Append(constraints);

            return implementation.ToString();
        }
    }

    private static string[] GetConstraintName(string name)
    {
        return name switch
        {
            "Sum" => [$"{Indent}IAdditionOperators<T, T, T>"],
            "Arithmetic" => [$"{Indent}ISubtractionOperators<T, T, T>", $"{Indent}IMultiplyOperators<T, T, T>", $"{Indent}IDivisionOperators<T, T, T>"],
            "Integer" => [$"{Indent}IModulusOperators<T, T, T>", $"{Indent}IBitwiseOperators<T, T, T>", $"{Indent}IShiftOperators<T, T, T>"],
            "Comparable" => [$"{Indent}IEqualityOperators<T, T, bool>"],
            "Ordered" => [$"{Indent}IComparisonOperators<T, T, bool>"],
            _ => [$"{Indent}{name}"]
        };
    }
}
