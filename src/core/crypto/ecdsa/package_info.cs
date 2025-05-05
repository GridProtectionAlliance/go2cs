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
global using bigmodꓸNat = go.crypto.@internal.bigmod_package.ΔNat;
global using bigꓸInt = go.math.big_package.ΔInt;
global using bigꓸRat = go.math.big_package.ΔRat;
global using ecdhꓸCurve = go.crypto.ecdh_package.ΔCurve;
global using ecdhꓸPublicKey = go.crypto.ecdh_package.ΔPublicKey;
// </ImportedTypeAliases>

using go;
using static go.crypto.ecdsa_package;

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
[assembly: GoImplement<(Point, error), Point>]
[assembly: GoImplement<(io.Reader, error), io_package.Reader>]
[assembly: GoImplement<(p Point, err error), Point>]
[assembly: GoImplement<cipher.Block, error), crypto.cipher_package.Block>]
[assembly: GoImplement<crypto.cipher_package.StreamReader, io_package.Reader>]
[assembly: GoImplement<zr, crypto.cipher_package.Stream>]
[assembly: GoImplement<zr, io_package.Reader>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<PrivateKey, ж<PrivateKey>>(Indirect = true)]
[assembly: GoImplicitConv<PublicKey, ж<PublicKey>>(Indirect = true)]
[assembly: GoImplicitConv<big.nat}, big.nat}>(Inverted = true)]
[assembly: GoImplicitConv<bigmod.Modulus, ж<bigmod.Modulus>>(Indirect = true)]
[assembly: GoImplicitConv<bigmodꓸNat, ж<bigmodꓸNat>>(Indirect = true)]
// </ImplicitConversions>

namespace go.crypto;

[GoPackage("ecdsa")]
public static partial class ecdsa_package
{
}
