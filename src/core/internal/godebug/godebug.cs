// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package godebug makes the settings in the $GODEBUG environment variable
// available to other packages. These settings are often used for compatibility
// tweaks, when we need to change a default behavior but want to let users
// opt back in to the original. For example GODEBUG=http2server=0 disables
// HTTP/2 support in the net/http server.
//
// In typical usage, code should declare a Setting as a global
// and then call Value each time the current setting value is needed:
//
//	var http2server = godebug.New("http2server")
//
//	func ServeConn(c net.Conn) {
//		if http2server.Value() == "0" {
//			disallow HTTP/2
//			...
//		}
//		...
//	}
//
// Each time a non-default setting causes a change in program behavior,
// code must call [Setting.IncNonDefault] to increment a counter that can
// be reported by [runtime/metrics.Read]. The call must only happen when
// the program executes a non-default behavior, not just when the setting
// is set to a non-default value. This is occasionally (but very rarely)
// infeasible, in which case the internal/godebugs table entry must set
// Opaque: true, and the documentation in doc/godebug.md should
// mention that metrics are unavailable.
//
// Conventionally, the global variable representing a godebug is named
// for the godebug itself, with no case changes:
//
//	var gotypesalias = godebug.New("gotypesalias") // this
//	var goTypesAlias = godebug.New("gotypesalias") // NOT THIS
//
// The test in internal/godebugs that checks for use of IncNonDefault
// requires the use of this convention.
//
// Note that counters used with IncNonDefault must be added to
// various tables in other packages. See the [Setting.IncNonDefault]
// documentation for details.
namespace go.@internal;

// Note: Be careful about new imports here. Any package
// that internal/godebug imports cannot itself import internal/godebug,
// meaning it cannot introduce a GODEBUG setting of its own.
// We keep imports to the absolute bare minimum.
using bisect = @internal.bisect_package;

using godebugs = @internal.godebugs_package;

using sync = sync_package;

using atomic = sync.atomic_package;

using @unsafe = unsafe_package;

using _ = unsafe_package; // go:linkname
using sync;

partial class godebug_package {

// A Setting is a single setting in the $GODEBUG environment variable.
[GoType] partial struct Setting {
    internal @string name;
    internal sync_package.Once once;
    public partial ref ж<setting> setting { get; }
}

[GoType] partial struct setting {
    internal sync.atomic_package.Pointer value;
    internal sync_package.Once nonDefaultOnce;
    internal sync.atomic_package.Uint64 nonDefault;
    internal ж<@internal.godebugs_package.Info> info;
}

[GoType] partial struct value {
    internal @string text;
    internal ж<@internal.bisect_package.Matcher> bisect;
}

// New returns a new Setting for the $GODEBUG setting with the given name.
//
// GODEBUGs meant for use by end users must be listed in ../godebugs/table.go,
// which is used for generating and checking various documentation.
// If the name is not listed in that table, New will succeed but calling Value
// on the returned Setting will panic.
// To disable that panic for access to an undocumented setting,
// prefix the name with a #, as in godebug.New("#gofsystrace").
// The # is a signal to New but not part of the key used in $GODEBUG.
//
// Note that almost all settings should arrange to call [IncNonDefault] precisely
// when program behavior is changing from the default due to the setting
// (not just when the setting is different, but when program behavior changes).
// See the [internal/godebug] package comment for more.
public static ж<Setting> New(@string name) {
    return Ꮡ(new Setting(name: name));
}

// Name returns the name of the setting.
[GoRecv] public static @string Name(this ref Setting s) {
    if (s.name != ""u8 && s.name[0] == (rune)'#') {
        return s.name[1..];
    }
    return s.name;
}

// Undocumented reports whether this is an undocumented setting.
[GoRecv] public static bool Undocumented(this ref Setting s) {
    return s.name != ""u8 && s.name[0] == (rune)'#';
}

// String returns a printable form for the setting: name=value.
[GoRecv] public static @string String(this ref Setting s) {
    return s.Name() + "="u8 + s.Value();
}

// IncNonDefault increments the non-default behavior counter
// associated with the given setting.
// This counter is exposed in the runtime/metrics value
// /godebug/non-default-behavior/<name>:events.
//
// Note that Value must be called at least once before IncNonDefault.
[GoRecv] public static void IncNonDefault(this ref Setting s) {
    s.nonDefaultOnce.Do(s.register);
    s.nonDefault.Add(1);
}

[GoRecv] internal static void register(this ref Setting s) {
    if (s.info == nil || s.info.Opaque) {
        throw panic("godebug: unexpected IncNonDefault of "u8 + s.name);
    }
    registerMetric("/godebug/non-default-behavior/"u8 + s.Name() + ":events"u8, s.nonDefault.Load);
}

// cache is a cache of all the GODEBUG settings,
// a locked map[string]*atomic.Pointer[string].
//
// All Settings with the same name share a single
// *atomic.Pointer[string], so that when GODEBUG
// changes only that single atomic string pointer
// needs to be updated.
//
// A name appears in the values map either if it is the
// name of a Setting for which Value has been called
// at least once, or if the name has ever appeared in
// a name=value pair in the $GODEBUG environment variable.
// Once entered into the map, the name is never removed.
internal static sync.Map cache; // name string -> value *atomic.Pointer[string]

internal static value empty;

// Value returns the current value for the GODEBUG setting s.
//
// Value maintains an internal cache that is synchronized
// with changes to the $GODEBUG environment variable,
// making Value efficient to call as frequently as needed.
// Clients should therefore typically not attempt their own
// caching of Value's result.
[GoRecv] public static @string Value(this ref Setting s) {
    s.once.Do(() => {
        s.setting = lookup(s.Name());
        if (s.info == nil && !s.Undocumented()) {
            throw panic("godebug: Value of name not listed in godebugs.All: "u8 + s.name);
        }
    });
    var v = s.value.Load().val;
    if (v.bisect != nil && !v.bisect.Stack(stderr)) {
        return ""u8;
    }
    return v.text;
}

// lookup returns the unique *setting value for the given name.
internal static ж<setting> lookup(@string name) {
    {
        var (v, ok) = cache.Load(name); if (ok) {
            return v._<setting.val>();
        }
    }
    var s = @new<setting>();
    s.val.info = godebugs.Lookup(name);
    (~s).value.Store(Ꮡ(empty));
    {
        var (v, loaded) = cache.LoadOrStore(name, s); if (loaded) {
            // Lost race: someone else created it. Use theirs.
            return v._<setting.val>();
        }
    }
    return s;
}

// setUpdate is provided by package runtime.
// It calls update(def, env), where def is the default GODEBUG setting
// and env is the current value of the $GODEBUG environment variable.
// After that first call, the runtime calls update(def, env)
// again each time the environment variable changes
// (due to use of os.Setenv, for example).
//
//go:linkname setUpdate
internal static partial void setUpdate(Action<@string, @string> update);

// registerMetric is provided by package runtime.
// It forwards registrations to runtime/metrics.
//
//go:linkname registerMetric
internal static partial void registerMetric(@string name, Func<uint64> read);

// setNewIncNonDefault is provided by package runtime.
// The runtime can do
//
//	inc := newNonDefaultInc(name)
//
// instead of
//
//	inc := godebug.New(name).IncNonDefault
//
// since it cannot import godebug.
//
//go:linkname setNewIncNonDefault
internal static partial void setNewIncNonDefault(Func<@string, Action> newIncNonDefault);

[GoInit] internal static void init() {
    setUpdate(update);
    setNewIncNonDefault(newIncNonDefault);
}

internal static Action newIncNonDefault(@string name) {
    var s = New(name);
    s.Value();
    return s.IncNonDefault;
}

internal static sync.Mutex updateMu;

// update records an updated GODEBUG setting.
// def is the default GODEBUG setting for the running binary,
// and env is the current value of the $GODEBUG environment variable.
internal static void update(@string def, @string env) => func((defer, _) => {
    updateMu.Lock();
    var updateMuʗ1 = updateMu;
    defer(updateMuʗ1.Unlock);
    // Update all the cached values, creating new ones as needed.
    // We parse the environment variable first, so that any settings it has
    // are already locked in place (did[name] = true) before we consider
    // the defaults.
    var did = new map<@string, bool>();
    parse(did, env);
    parse(did, def);
    // Clear any cached values that are no longer present.
    cache.Range(
    var didʗ2 = did;
    var emptyʗ2 = empty;
    (any name, any s) => {
        if (!didʗ2[name._<@string>()]) {
            s._<setting.val>().value.Store(Ꮡ(emptyʗ2));
        }
        return true;
    });
});

// parse parses the GODEBUG setting string s,
// which has the form k=v,k2=v2,k3=v3.
// Later settings override earlier ones.
// Parse only updates settings k=v for which did[k] = false.
// It also sets did[k] = true for settings that it updates.
// Each value v can also have the form v#pattern,
// in which case the GODEBUG is only enabled for call stacks
// matching pattern, for use with golang.org/x/tools/cmd/bisect.
internal static void parse(map<@string, bool> did, @string s) {
    // Scan the string backward so that later settings are used
    // and earlier settings are ignored.
    // Note that a forward scan would cause cached values
    // to temporarily use the ignored value before being
    // updated to the "correct" one.
    nint end = len(s);
    nint eq = -1;
    for (nint i = end - 1; i >= -1; i--) {
        if (i == -1 || s[i] == (rune)','){
            if (eq >= 0) {
                @string name = s[(int)(i + 1)..(int)(eq)];
                @string arg = s[(int)(eq + 1)..(int)(end)];
                if (!did[name]) {
                    did[name] = true;
                    var v = Ꮡ(new value(text: arg));
                    for (nint j = 0; j < len(arg); j++) {
                        if (arg[j] == (rune)'#') {
                            v.val.text = arg[..(int)(j)];
                            (v.val.bisect, _) = bisect.New(arg[(int)(j + 1)..]);
                            break;
                        }
                    }
                    (~lookup(name)).value.Store(v);
                }
            }
            eq = -1;
            end = i;
        } else 
        if (s[i] == (rune)'=') {
            eq = i;
        }
    }
}

[GoType] partial struct runtimeStderr {
}

internal static runtimeStderr stderr;

[GoRecv] internal static (nint, error) Write(this ref runtimeStderr _, slice<byte> b) {
    if (len(b) > 0) {
        write(2, new @unsafe.Pointer(Ꮡ(b, 0)), ((int32)len(b)));
    }
    return (len(b), default!);
}

// Since we cannot import os or syscall, use the runtime's write function
// to print to standard error.
//
//go:linkname write runtime.write
internal static partial int32 write(uintptr fd, @unsafe.Pointer p, int32 n);

} // end godebug_package
