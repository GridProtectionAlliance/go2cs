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
using static go.image.draw_package;

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
[assembly: GoImplement<(RGBA64Image, bool), RGBA64Image>]
[assembly: GoImplement<(image.RGBA64Image, bool), image_package.RGBA64Image>]
[assembly: GoImplement<Image, image_package.Image>]
[assembly: GoImplement<floydSteinberg, Drawer>]
[assembly: GoImplement<image.color_package.Alpha16, image.color_package.Color>]
[assembly: GoImplement<image.color_package.RGBA64, image.color_package.Color>]
[assembly: GoImplement<image_package.Paletted, Image>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<image.Alpha, ж<image.Alpha>>(Indirect = true)]
[assembly: GoImplicitConv<image.CMYK, ж<image.CMYK>>(Indirect = true)]
[assembly: GoImplicitConv<image.Gray, ж<image.Gray>>(Indirect = true)]
[assembly: GoImplicitConv<image.NRGBA, ж<image.NRGBA>>(Indirect = true)]
[assembly: GoImplicitConv<image.Uniform, ж<image.Uniform>>(Indirect = true)]
[assembly: GoImplicitConv<image.YCbCr, ж<image.YCbCr>>(Indirect = true)]
[assembly: GoImplicitConv<struct{Min image.Point; Max image.Point}, struct{Min image.Point; Max image.Point}>(Inverted = true)]
[assembly: GoImplicitConv<struct{X int; Y int}, struct{X int; Y int}>(Inverted = true)]
// </ImplicitConversions>

namespace go.image;

[GoPackage("draw")]
public static partial class draw_package
{
}
