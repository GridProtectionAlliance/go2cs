// Hand-owned manual conversion of state.go — the marker below drops state.go from the convert set,
// so a re-transpile must NOT overwrite this file (it emits the state.cs.auto review sibling instead)
// while STILL analyzing/visiting state.go so its package-wide state (the lifted schedlike_disable
// anonymous-struct type, the sched/newprocs package vars) reaches the sibling main.cs emission.
// This file guards the seeded-reconvert defect fixed 2026-07-17: skipping a marked file's entire
// visit corrupted every sibling file (raw Go struct{...} text in selectors, package-var assignments
// re-declared as shadowing locals).
[module: go.GoManualConversion]

namespace go;

partial class main_package {

[GoType("dyn")] partial struct schedlike_disable {
    internal bool user;
    internal int32 n;
}

[GoType] partial struct schedlike {
    internal schedlike_disable disable;
    internal @string label;
}

internal static ж<schedlike> Ꮡsched = new(default(schedlike));
internal static ref schedlike sched => ref Ꮡsched.Value;

internal static int32 newprocs;

} // end main_package
