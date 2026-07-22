// go2cs metadata anchor for the EXTERNAL test package (<name>_test): GoImplement /
// GoImplicitConv attributes recorded by its converted _test files whose GENERATED code
// (adapter classes, partial-struct implementations, conversion operators) must anchor to
// the test package class — the source generators host output in the first class of the
// attribute-bearing file, and test-file cast sites reference the adapters as members of
// the test package class. Production-anchored records stay in package_test_info.cs.

// <ImportedTypeAliases>
// </ImportedTypeAliases>

using go;
using static go.sort_package;
using static go.sort_test_package;

// <ExportedTypeAliases>
// </ExportedTypeAliases>

// <InterfaceImplementations>
[assembly: GoImplement<ByAge, sort_package.Interface>]
[assembly: GoImplement<ByName, sort_package.Interface>]
[assembly: GoImplement<ByWeight, sort_package.Interface>]
[assembly: GoImplement<adversaryTestingData, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<go.math.rand.rand_package.PCG, go.math.rand.rand_package.Source>(Pointer = true)]
[assembly: GoImplement<intPairs, sort_package.Interface>]
[assembly: GoImplement<multiSorter, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<myStructs, sort_package.Interface>]
[assembly: GoImplement<nonDeterministicTestingData, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<planetSorter, sort_package.Interface>(Pointer = true)]
[assembly: GoImplement<testingData, sort_package.Interface>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
// </ImplicitConversions>

namespace go;

public static partial class sort_test_package
{
}
