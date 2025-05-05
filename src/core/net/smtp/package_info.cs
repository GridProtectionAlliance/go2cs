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
global using netꓸAddr = go.net_package.ΔAddr;
global using netꓸError = go.net_package.ΔError;
global using textprotoꓸError = go.net.textproto_package.ΔError;
global using tlsꓸConnectionState = go.crypto.tls_package.ΔConnectionState;
// </ImportedTypeAliases>

using go;
using static go.net.smtp_package;

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
[assembly: GoTypeAlias("Auth", "ΔAuth")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<(io.WriteCloser, error), io_package.WriteCloser>]
[assembly: GoImplement<(net.Conn, error), net_package.Conn>]
[assembly: GoImplement<cramMD5Auth, ΔAuth>]
[assembly: GoImplement<dataCloser, io_package.WriteCloser>]
[assembly: GoImplement<net.textproto_package.ΔError, error>]
[assembly: GoImplement<net_package.Conn, io_package.ReadWriteCloser>]
[assembly: GoImplement<plainAuth, ΔAuth>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<struct{Name string; TLS bool; Auth <>string}, struct{Name string; TLS bool; Auth <>string}>(Inverted = true)]
[assembly: GoImplicitConv<struct{wall uint64; ext int64; loc *time.Location}, struct{wall uint64; ext int64; loc *time.Location}>(Inverted = true)]
// </ImplicitConversions>

namespace go.net;

[GoPackage("smtp")]
public static partial class smtp_package
{
}
