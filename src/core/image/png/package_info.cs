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
global using colorꓸRGBA = go.image.color_package.ΔRGBA;
global using imageꓸRGBA = go.image_package.ΔRGBA;
// </ImportedTypeAliases>

using go;
using static go.image.png_package;

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
[assembly: GoImplement<(image.Image, error), image_package.Image>]
[assembly: GoImplement<(image.PalettedImage, bool), image_package.PalettedImage>]
[assembly: GoImplement<(io.ReadCloser, error), io_package.ReadCloser>]
[assembly: GoImplement<(opaquer, bool), opaquer>]
[assembly: GoImplement<FormatError, error>]
[assembly: GoImplement<UnsupportedError, error>]
[assembly: GoImplement<bufio_package.Writer, io_package.Writer>]
[assembly: GoImplement<decoder, io_package.Reader>]
[assembly: GoImplement<encoder, io_package.Writer>]
[assembly: GoImplement<image.color_package.Palette, image.color_package.Model>]
[assembly: GoImplement<image_package.Gray, image_package.Image>]
[assembly: GoImplement<image_package.Gray16, image_package.Image>]
[assembly: GoImplement<image_package.NRGBA, image_package.Image>]
[assembly: GoImplement<image_package.NRGBA64, image_package.Image>]
[assembly: GoImplement<image_package.Paletted, image_package.Image>]
[assembly: GoImplement<image_package.RGBA64, image_package.Image>]
[assembly: GoImplement<image_package.ΔRGBA, image_package.Image>]
[assembly: GoImplement<io_package.ReadCloser, io_package.Reader>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<EncoderBuffer, ж<encoder>>(Indirect = true)]
[assembly: GoImplicitConv<encoder, ж<EncoderBuffer>>(Indirect = true)]
// </ImplicitConversions>

namespace go.image;

[GoPackage("png")]
public static partial class png_package
{
}
