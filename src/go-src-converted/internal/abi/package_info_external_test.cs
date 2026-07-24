// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /
// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code
// (adapter classes, partial-struct implementations, conversion operators) must anchor to
// the test package class — the source generators host output in the first class of the
// attribute-bearing file, and test-file cast sites reference the adapters as members of
// the test package class. Production-anchored records stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.@internal.abi_package;
using static go.@internal.abi_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<testing_package.T, testing_package.TB>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go.@internal;

public static partial class abi_test_package
{
}
