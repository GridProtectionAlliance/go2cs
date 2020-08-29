// package test23 -- go2cs converted at 2020 August 29 10:02:03 UTC
// import "cmd/go/testdata/src/complex/nest/sub/test23" ==> using test23 = go.cmd.go.testdata.src.complex.nest.sub.test23_package
// Original source: C:\Go\src\cmd\go\testdata\src\complex\nest\sub\test23\p.go
// Check that vendor/v3 is used but vendor/v2 is NOT used (sub/vendor/v2 wins).

using v2 = go.v2_package;
using v3 = go.v3_package;
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
    public static partial class test23_package
    {
        private static readonly var x = v3.ComplexNestVendorV3;

        private static readonly var y = v2.ComplexNestSubVendorV2;

    }
}}}}}}}}
