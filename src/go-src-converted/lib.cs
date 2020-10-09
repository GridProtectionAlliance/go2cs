// package lib -- go2cs converted at 2020 October 09 06:03:59 UTC
// import "golang.org/x/tools/go/analysis/passes/ctrlflow/testdata/src/lib" ==> using lib = go.golang.org.x.tools.go.analysis.passes.ctrlflow.testdata.src.lib_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\ctrlflow\testdata\src\lib\lib.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes {
namespace ctrlflow {
namespace testdata {
namespace src
{
    public static partial class lib_package
    {
        public static void CanReturn()
        {
        }

        public static void NoReturn()
        {
            while (true)
            {
            }


        }
    }
}}}}}}}}}}
