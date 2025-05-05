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
// </ImportedTypeAliases>

using go;
using static go.vendor.golang.org.x.net.dns.dnsmessage_package;

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
[assembly: GoTypeAlias("AAAAResource", "ΔAAAAResource")]
[assembly: GoTypeAlias("AResource", "ΔAResource")]
[assembly: GoTypeAlias("CNAMEResource", "ΔCNAMEResource")]
[assembly: GoTypeAlias("MXResource", "ΔMXResource")]
[assembly: GoTypeAlias("NSResource", "ΔNSResource")]
[assembly: GoTypeAlias("OPTResource", "ΔOPTResource")]
[assembly: GoTypeAlias("PTRResource", "ΔPTRResource")]
[assembly: GoTypeAlias("Question", "ΔQuestion")]
[assembly: GoTypeAlias("SOAResource", "ΔSOAResource")]
[assembly: GoTypeAlias("SRVResource", "ΔSRVResource")]
[assembly: GoTypeAlias("TXTResource", "ΔTXTResource")]
[assembly: GoTypeAlias("UnknownResource", "ΔUnknownResource")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<nestedError, error>]
[assembly: GoImplement<ΔAAAAResource, ResourceBody>]
[assembly: GoImplement<ΔAResource, ResourceBody>]
[assembly: GoImplement<ΔCNAMEResource, ResourceBody>]
[assembly: GoImplement<ΔMXResource, ResourceBody>]
[assembly: GoImplement<ΔNSResource, ResourceBody>]
[assembly: GoImplement<ΔOPTResource, ResourceBody>]
[assembly: GoImplement<ΔPTRResource, ResourceBody>]
[assembly: GoImplement<ΔSOAResource, ResourceBody>]
[assembly: GoImplement<ΔSRVResource, ResourceBody>]
[assembly: GoImplement<ΔTXTResource, ResourceBody>]
[assembly: GoImplement<ΔUnknownResource, ResourceBody>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go.vendor.golang.org.x.net.dns;

[GoPackage("dnsmessage")]
public static partial class dnsmessage_package
{
}
