// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;
using goexperiment = @internal.goexperiment_package;

partial class buildcfg_package {

// ExperimentFlags represents a set of GOEXPERIMENT flags relative to a baseline
// (platform-default) experiment configuration.
[GoType] partial struct ExperimentFlags {
    public partial ref @internal.goexperiment_package.Flags Flags { get; }
    internal @internal.goexperiment_package.Flags baseline;
}

// Experiment contains the toolchain experiments enabled for the
// current build.
//
// (This is not necessarily the set of experiments the compiler itself
// was built with.)
//
// experimentBaseline specifies the experiment flags that are enabled by
// default in the current toolchain. This is, in effect, the "control"
// configuration and any variation from this is an experiment.
public static ExperimentFlags Experiment = () => {
    var (flags, err) = ParseGOEXPERIMENT(GOOS, GOARCH, envOr("GOEXPERIMENT"u8, defaultGOEXPERIMENT));
    if (err != default!) {
        var Error = err;
        return new ExperimentFlags(nil);
    }
    return flags.val;
}();

// DefaultGOEXPERIMENT is the embedded default GOEXPERIMENT string.
// It is not guaranteed to be canonical.
public static readonly @string DefaultGOEXPERIMENT = "";

// FramePointerEnabled enables the use of platform conventions for
// saving frame pointers.
//
// This used to be an experiment, but now it's always enabled on
// platforms that support it.
//
// Note: must agree with runtime.framepointer_enabled.
public static bool FramePointerEnabled = GOARCH == "amd64"u8 || GOARCH == "arm64"u8;

// ParseGOEXPERIMENT parses a (GOOS, GOARCH, GOEXPERIMENT)
// configuration tuple and returns the enabled and baseline experiment
// flag sets.
//
// TODO(mdempsky): Move to internal/goexperiment.
public static (ж<ExperimentFlags>, error) ParseGOEXPERIMENT(@string goos, @string goarch, @string goexp) {
    // regabiSupported is set to true on platforms where register ABI is
    // supported and enabled by default.
    // regabiAlwaysOn is set to true on platforms where register ABI is
    // always on.
    bool regabiSupported = default!;
    bool regabiAlwaysOn = default!;
    var exprᴛ1 = goarch;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8 || exprᴛ1 == "loong64"u8 || exprᴛ1 == "ppc64le"u8 || exprᴛ1 == "ppc64"u8 || exprᴛ1 == "riscv64"u8) {
        regabiAlwaysOn = true;
        regabiSupported = true;
    }

    ref var baseline = ref heap<@internal.goexperiment_package.Flags>(out var Ꮡbaseline);
    baseline = new goexperiment.Flags(
        RegabiWrappers: regabiSupported,
        RegabiArgs: regabiSupported,
        CoverageRedesign: true
    );
    // Start with the statically enabled set of experiments.
    var flags = Ꮡ(new ExperimentFlags(
        Flags: baseline,
        baseline: baseline
    ));
    // Pick up any changes to the baseline configuration from the
    // GOEXPERIMENT environment. This can be set at make.bash time
    // and overridden at build time.
    if (goexp != ""u8) {
        // Create a map of known experiment names.
        var names = new map<@string, Action<bool>>();
        var rv = reflect.ValueOf(Ꮡ((~flags).Flags)).Elem();
        var rt = rv.Type();
        for (nint i = 0; i < rt.NumField(); i++) {
            var field = rv.Field(i);
            names[strings.ToLower(rt.Field(i).Name)] = 
            var fieldʗ1 = field;
            () => fieldʗ1.SetBool();
        }
        // "regabi" is an alias for all working regabi
        // subexperiments, and not an experiment itself. Doing
        // this as an alias make both "regabi" and "noregabi"
        // do the right thing.
        names["regabi"u8] = 
        var flagsʗ1 = flags;
        (bool v) => {
            flagsʗ1.RegabiWrappers = v;
            flagsʗ1.RegabiArgs = v;
        };
        // Parse names.
        foreach (var (_, f) in strings.Split(goexp, ","u8)) {
            if (f == ""u8) {
                continue;
            }
            if (f == "none"u8) {
                // GOEXPERIMENT=none disables all experiment flags.
                // This is used by cmd/dist, which doesn't know how
                // to build with any experiment flags.
                flags.val.Flags = new goexperiment.Flags(nil);
                continue;
            }
            var val = true;
            if (strings.HasPrefix(f, "no"u8)) {
                (f, val) = (f[2..], false);
            }
            var set = names[f];
            var ok = names[f];
            if (!ok) {
                return (default!, fmt.Errorf("unknown GOEXPERIMENT %s"u8, f));
            }
            set(val);
        }
    }
    if (regabiAlwaysOn) {
        flags.RegabiWrappers = true;
        flags.RegabiArgs = true;
    }
    // regabi is only supported on amd64, arm64, loong64, riscv64, ppc64 and ppc64le.
    if (!regabiSupported) {
        flags.RegabiWrappers = false;
        flags.RegabiArgs = false;
    }
    // Check regabi dependencies.
    if (flags.RegabiArgs && !flags.RegabiWrappers) {
        return (default!, fmt.Errorf("GOEXPERIMENT regabiargs requires regabiwrappers"u8));
    }
    return (flags, default!);
}

// String returns the canonical GOEXPERIMENT string to enable this experiment
// configuration. (Experiments in the same state as in the baseline are elided.)
[GoRecv] public static @string String(this ref ExperimentFlags exp) {
    return strings.Join(expList(Ꮡ(exp.Flags), Ꮡ(exp.baseline), false), ","u8);
}

// expList returns the list of lower-cased experiment names for
// experiments that differ from base. base may be nil to indicate no
// experiments. If all is true, then include all experiment flags,
// regardless of base.
internal static slice<@string> expList(ж<goexperiment.Flags> Ꮡexp, ж<goexperiment.Flags> Ꮡbase, bool all) {
    ref var exp = ref Ꮡexp.val;
    ref var @base = ref Ꮡbase.val;

    slice<@string> list = default!;
    var rv = reflect.ValueOf(exp).Elem();
    reflectꓸValue rBase = default!;
    if (@base != nil) {
        rBase = reflect.ValueOf(@base).Elem();
    }
    var rt = rv.Type();
    for (nint i = 0; i < rt.NumField(); i++) {
        @string name = strings.ToLower(rt.Field(i).Name);
        var val = rv.Field(i).Bool();
        var baseVal = false;
        if (@base != nil) {
            baseVal = rBase.Field(i).Bool();
        }
        if (all || val != baseVal) {
            if (val){
                list = append(list, name);
            } else {
                list = append(list, "no"u8 + name);
            }
        }
    }
    return list;
}

// Enabled returns a list of enabled experiments, as
// lower-cased experiment names.
[GoRecv] public static slice<@string> Enabled(this ref ExperimentFlags exp) {
    return expList(Ꮡ(exp.Flags), nil, false);
}

// All returns a list of all experiment settings.
// Disabled experiments appear in the list prefixed by "no".
[GoRecv] public static slice<@string> All(this ref ExperimentFlags exp) {
    return expList(Ꮡ(exp.Flags), nil, true);
}

} // end buildcfg_package
