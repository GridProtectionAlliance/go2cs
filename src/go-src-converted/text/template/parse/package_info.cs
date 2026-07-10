// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using runtimeꓸError = go.runtime_package.ΔError;
// </ImportedTypeAliases>

using go;
using static go.text.template.parse_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<ActionNode, Node>(Pointer = true)]
[assembly: GoImplement<BoolNode, Node>(Pointer = true)]
[assembly: GoImplement<BreakNode, Node>(Pointer = true)]
[assembly: GoImplement<ChainNode, Node>(Pointer = true)]
[assembly: GoImplement<CommandNode, Node>(Pointer = true)]
[assembly: GoImplement<CommentNode, Node>(Pointer = true)]
[assembly: GoImplement<ContinueNode, Node>(Pointer = true)]
[assembly: GoImplement<DotNode, Node>(Pointer = true)]
[assembly: GoImplement<FieldNode, Node>(Pointer = true)]
[assembly: GoImplement<IdentifierNode, Node>(Pointer = true)]
[assembly: GoImplement<IfNode, Node>(Pointer = true)]
[assembly: GoImplement<ListNode, Node>(Pointer = true)]
[assembly: GoImplement<NilNode, Node>(Pointer = true)]
[assembly: GoImplement<NumberNode, Node>(Pointer = true)]
[assembly: GoImplement<PipeNode, Node>(Pointer = true)]
[assembly: GoImplement<RangeNode, Node>(Pointer = true)]
[assembly: GoImplement<StringNode, Node>(Pointer = true)]
[assembly: GoImplement<TemplateNode, Node>(Pointer = true)]
[assembly: GoImplement<TextNode, Node>(Pointer = true)]
[assembly: GoImplement<VariableNode, Node>(Pointer = true)]
[assembly: GoImplement<WithNode, Node>(Pointer = true)]
[assembly: GoImplement<elseNode, Node>(Pointer = true)]
[assembly: GoImplement<endNode, Node>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go.text.template;

[GoPackage("parse")]
public static partial class parse_package
{
}
