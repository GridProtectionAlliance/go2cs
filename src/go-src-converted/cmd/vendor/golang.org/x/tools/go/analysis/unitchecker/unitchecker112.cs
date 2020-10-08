// +build go1.12

// package unitchecker -- go2cs converted at 2020 October 08 04:58:21 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/unitchecker" ==> using unitchecker = go.cmd.vendor.golang.org.x.tools.go.analysis.unitchecker_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\unitchecker\unitchecker112.go
using importer = go.go.importer_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis
{
    public static partial class unitchecker_package
    {
        private static void init()
        {
            importerForCompiler = importer.ForCompiler;
        }
    }
}}}}}}}}
