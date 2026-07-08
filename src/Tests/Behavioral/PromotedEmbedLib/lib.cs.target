namespace go;

partial class PromotedEmbedLib_package {

[GoType] partial struct Reading {
    public nint Sum;
}

[GoType] partial struct common {
    internal nint sum;
}

internal static void Add(this ж<common> Ꮡc, nint n) {
    ref var c = ref Ꮡc.Value;

    var p = Ꮡc.of(common.Ꮡsum);
    p.Value += n;
}

internal static Reading Report(this ж<common> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    var p = Ꮡc.of(common.Ꮡsum);
    return new Reading(Sum: p.Value);
}

[GoType] partial struct Counter {
    internal partial ref common common { get; }
    public @string Label;
}

public static ж<Counter> New(@string label) {
    return Ꮡ(new Counter(Label: label));
}

} // end PromotedEmbedLib_package
