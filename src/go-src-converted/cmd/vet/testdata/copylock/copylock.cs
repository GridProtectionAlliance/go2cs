// package copylock -- go2cs converted at 2020 October 08 04:58:36 UTC
// import "cmd/vet/testdata/copylock" ==> using copylock = go.cmd.vet.testdata.copylock_package
// Original source: C:\Go\src\cmd\vet\testdata\copylock\copylock.go
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class copylock_package
    {
        public static void BadFunc()
        {
            ptr<sync.Mutex> x;
            var p = x;
            ref sync.Mutex y = ref heap(out ptr<sync.Mutex> _addr_y);
            _addr_p = _addr_y;
            p = ref _addr_p.val;
            p.val = x.val; // ERROR "assignment copies lock value to \*p: sync.Mutex"
        }
    }
}}}}
