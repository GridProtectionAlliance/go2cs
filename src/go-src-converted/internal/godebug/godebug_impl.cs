// go2cs NATIVE IMPLEMENTATION (hand-owned companion; supplies the runtime-provided
// //go:linkname hooks godebug.cs declares as bodyless partials — see PartialStubGenerator,
// which emits throwing stubs only for partials with no implementing part).
//
// In Go these live in runtime/runtime.go: setUpdate stores the package's update callback and
// immediately notifies it once with (godebugDefault, $GODEBUG) — godebugDefault is the default
// GODEBUG list baked in from the main module's go.mod `godebug` lines (none for converted
// programs, so empty); re-notification happens only when os.Setenv("GODEBUG") changes the
// environment, which the converted runtime does not surface, so the one-shot notify is the
// operative behavior. registerMetric registers a runtime/metrics reader for
// /godebug/non-default-behavior/<name>:events and setNewIncNonDefault installs the hook
// runtime/metrics uses to count non-default godebug uses — both are metrics plumbing with no
// behavioral effect on the program itself, so they are inert here.

using System;

namespace go.@internal;

partial class godebug_package
{
    internal static partial void setUpdate(Action<@string, @string> update)
    {
        // One-shot notify, matching runtime.godebug_setUpdate → godebugNotify: the package
        // parses $GODEBUG and applies non-default settings before any Setting.Value read.
        update(""u8, Environment.GetEnvironmentVariable("GODEBUG") ?? "");
    }

    internal static partial void registerMetric(@string name, Func<uint64> read)
    {
        // runtime/metrics registration — inert (no converted metrics consumer).
    }

    internal static partial void setNewIncNonDefault(Func<@string, Action> newIncNonDefault)
    {
        // runtime/metrics hook — inert (no converted metrics consumer).
    }
}
