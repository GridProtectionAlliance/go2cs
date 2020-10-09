// +build go1.12

// package unitchecker -- go2cs converted at 2020 October 09 06:02:55 UTC
// import "golang.org/x/tools/go/analysis/unitchecker" ==> using unitchecker = go.golang.org.x.tools.go.analysis.unitchecker_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\unitchecker\unitchecker112.go
using importer = go.go.importer_package;
using static go.builtin;

namespace go {
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
}}}}}}
