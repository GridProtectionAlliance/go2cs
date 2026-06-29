namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct profBuf {
    internal slice<nint> data;
}

internal static void push(this ж<profBuf> Ꮡb, nint v) {
    ref var b = ref Ꮡb.val;

    appendInt(Ꮡb.of(profBuf.Ꮡdata), v);
}

internal static void appendInt(ж<slice<nint>> Ꮡs, nint v) {
    ref var s = ref Ꮡs.val;

    s = append(s, v);
}

[GoRecv] internal static nint sum(this ref profBuf b) {
    nint t = 0;
    foreach (var (_, x) in b.data) {
        t += x;
    }
    return t;
}

[GoType] partial struct cpuProfile {
    internal nint count;
    internal ж<profBuf> log;
}

internal static ж<cpuProfile> Ꮡcpuprof = new(default(cpuProfile));
internal static ref cpuProfile cpuprof => ref Ꮡcpuprof.val;

internal static void incr(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p++;
}

internal static void run() {
    incr(Ꮡcpuprof.of(cpuProfile.Ꮡcount));
    cpuprof.log.push(cpuprof.count);
    cpuprof.log.push(cpuprof.count);
}

[GoType] partial struct holder {
    internal ж<cpuProfile> span;
}

internal static nint viaLocal(ж<holder> Ꮡh) {
    ref var h = ref Ꮡh.val;

    var s = h.span;
    (~s).log.push(10);
    return (~s).log.sum();
}

internal static void Main() {
    cpuprof.log = Ꮡ(new profBuf(nil));
    run();
    fmt.Println(cpuprof.count);
    fmt.Println(cpuprof.log.sum());
    var h = Ꮡ(new holder(span: Ꮡ(new cpuProfile(log: Ꮡ(new profBuf(nil))))));
    fmt.Println(viaLocal(h));
}

} // end main_package
