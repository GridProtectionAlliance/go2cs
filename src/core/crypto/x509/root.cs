// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using godebug = @internal.godebug_package;
using sync = sync_package;
using _ = unsafe_package; // for linkname
using @internal;

partial class x509_package {

// systemRoots should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/breml/rootcerts
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname systemRoots
internal static sync.Once once;

internal static sync.RWMutex systemRootsMu;

internal static ж<CertPool> systemRoots;

internal static error systemRootsErr;

internal static bool fallbacksSet;

internal static ж<CertPool> systemRootsPool() => func((defer, _) => {
    once.Do(initSystemRoots);
    systemRootsMu.RLock();
    var systemRootsMuʗ1 = systemRootsMu;
    defer(systemRootsMuʗ1.RUnlock);
    return systemRoots;
});

internal static void initSystemRoots() => func((defer, _) => {
    systemRootsMu.Lock();
    var systemRootsMuʗ1 = systemRootsMu;
    defer(systemRootsMuʗ1.Unlock);
    (systemRoots, systemRootsErr) = loadSystemRoots();
    if (systemRootsErr != default!) {
        systemRoots = default!;
    }
});

internal static ж<godebug.Setting> x509usefallbackroots = godebug.New("x509usefallbackroots"u8);

// SetFallbackRoots sets the roots to use during certificate verification, if no
// custom roots are specified and a platform verifier or a system certificate
// pool is not available (for instance in a container which does not have a root
// certificate bundle). SetFallbackRoots will panic if roots is nil.
//
// SetFallbackRoots may only be called once, if called multiple times it will
// panic.
//
// The fallback behavior can be forced on all platforms, even when there is a
// system certificate pool, by setting GODEBUG=x509usefallbackroots=1 (note that
// on Windows and macOS this will disable usage of the platform verification
// APIs and cause the pure Go verifier to be used). Setting
// x509usefallbackroots=1 without calling SetFallbackRoots has no effect.
public static void SetFallbackRoots(ж<CertPool> Ꮡroots) => func((defer, _) => {
    ref var roots = ref Ꮡroots.val;

    if (roots == nil) {
        throw panic("roots must be non-nil");
    }
    // trigger initSystemRoots if it hasn't already been called before we
    // take the lock
    _ = systemRootsPool();
    systemRootsMu.Lock();
    var systemRootsMuʗ1 = systemRootsMu;
    defer(systemRootsMuʗ1.Unlock);
    if (fallbacksSet) {
        throw panic("SetFallbackRoots has already been called");
    }
    fallbacksSet = true;
    if (systemRoots != nil && (systemRoots.len() > 0 || (~systemRoots).systemPool)) {
        if (x509usefallbackroots.Value() != "1"u8) {
            return;
        }
        x509usefallbackroots.IncNonDefault();
    }
    (systemRoots, systemRootsErr) = (roots, default!);
});

} // end x509_package
