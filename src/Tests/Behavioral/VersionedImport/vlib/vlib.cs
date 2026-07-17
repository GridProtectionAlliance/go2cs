namespace go.vlib;

partial class vlib_package {

[GoType] partial interface Source {
    uint64 Uint64();
}

[GoType] partial struct PCG {
    internal uint64 state;
}

public static ж<PCG> NewPCG(uint64 seed) {
    return Ꮡ(new PCG(state: seed));
}

[GoRecv] public static uint64 Uint64(this ref PCG p) {
    p.state ^= (p.state << (int)(13));
    p.state ^= (p.state >> (int)(7));
    p.state ^= (p.state << (int)(17));
    return p.state;
}

[GoType] partial struct Rand {
    internal Source src;
}

public static ж<Rand> New(Source src) {
    return Ꮡ(new Rand(src: src));
}

[GoRecv] public static nint IntN(this ref Rand r, nint n) {
    return (nint)(r.src.Uint64() % (uint64)n);
}

} // end vlib_package
