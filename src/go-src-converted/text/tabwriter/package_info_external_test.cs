// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /
// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code
// (adapter classes, partial-struct implementations, conversion operators) must anchor to
// the test package class — the source generators host output in the first class of the
// attribute-bearing file, and test-file cast sites reference the adapters as members of
// the test package class. Production-anchored records stay in package_test_info.cs.

// <ImportedTypeAliases>
using tabwriter = go.text.tabwriter_package;
using testing = go.testing_package;
// </ImportedTypeAliases>

using go;
using static go.text.tabwriter_package;
using static go.text.tabwriter_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<panicWriter, io_package.Writer>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<buffer, ж<buffer>>(Indirect = true)]
// </ImplicitConversions>

namespace go.text;

public static partial class tabwriter_test_package
{
}
