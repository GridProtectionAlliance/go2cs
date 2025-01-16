using static go2cs.Common;

namespace go2cs.Templates.InterfaceType;

internal class InterfaceTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string InterfaceName;

    public override string TemplateBody =>
        $$"""
              [{{GeneratedCodeAttribute}}]
              {{Scope}} partial interface {{InterfaceName}}
              {
              }
          """;
}
