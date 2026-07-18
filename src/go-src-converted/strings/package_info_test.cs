// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /
// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code
// (adapter classes, partial-struct implementations, conversion operators) must anchor to
// the test package class — the source generators host output in the first class of the
// attribute-bearing file, and test-file cast sites reference the adapters as members of
// the test package class. Production-anchored records stay in package_test_info.cs.

// <ImportedTypeAliases>
using strings = go.strings_package;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.strings_package;
using static go.strings_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<errWriter, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go;

public static partial class strings_test_package
{
}
