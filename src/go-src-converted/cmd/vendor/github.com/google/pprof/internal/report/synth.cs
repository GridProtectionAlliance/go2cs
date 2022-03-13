// package report -- go2cs converted at 2022 March 13 06:36:54 UTC
// import "cmd/vendor/github.com/google/pprof/internal/report" ==> using report = go.cmd.vendor.github.com.google.pprof.@internal.report_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\report\synth.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using profile = github.com.google.pprof.profile_package;


// synthCode assigns addresses to locations without an address.

public static partial class report_package {

private partial struct synthCode {
    public ulong next;
    public map<ptr<profile.Location>, ulong> addr; // Synthesized address assigned to a location
}

private static ptr<synthCode> newSynthCode(slice<ptr<profile.Mapping>> mappings) { 
    // Find a larger address than any mapping.
    ptr<synthCode> s = addr(new synthCode(next:1));
    foreach (var (_, m) in mappings) {
        if (s.next < m.Limit) {
            s.next = m.Limit;
        }
    }    return _addr_s!;
}

// address returns the synthetic address for loc, creating one if needed.
private static ulong address(this ptr<synthCode> _addr_s, ptr<profile.Location> _addr_loc) => func((_, panic, _) => {
    ref synthCode s = ref _addr_s.val;
    ref profile.Location loc = ref _addr_loc.val;

    if (loc.Address != 0) {
        panic("can only synthesize addresses for locations without an address");
    }
    {
        var addr__prev1 = addr;

        var (addr, ok) = s.addr[loc];

        if (ok) {
            return addr;
        }
        addr = addr__prev1;

    }
    if (s.addr == null) {
        s.addr = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<profile.Location>, ulong>{};
    }
    var addr = s.next;
    s.next++;
    s.addr[loc] = addr;
    return addr;
});

} // end report_package
