// package test12 -- go2cs converted at 2020 August 29 10:02:03 UTC
// import "cmd/go/testdata/src/complex/nest/sub/test12" ==> using test12 = go.cmd.go.testdata.src.complex.nest.sub.test12_package
// Original source: C:\Go\src\cmd\go\testdata\src\complex\nest\sub\test12\p.go
// Check that vendor/v1 is used but vendor/v2 is NOT used (sub/vendor/v2 wins).

using v1 = go.v1_package;
using v2 = go.v2_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace testdata {
namespace src {
namespace complex {
namespace nest {
namespace sub
{
    public static partial class test12_package
    {
        private static readonly var x = v1.ComplexNestVendorV1;

        private static readonly var y = v2.ComplexNestSubVendorV2;

    }
}}}}}}}}
