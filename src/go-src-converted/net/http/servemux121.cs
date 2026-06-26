// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

// This file implements ServeMux behavior as in Go 1.21.
// The behavior is controlled by a GODEBUG setting.
// Most of this code is derived from commit 08e35cc334.
// Changes are minimal: aside from the different receiver type,
// they mostly involve renaming functions, usually by unexporting them.
// servemux121.go exists solely to provide a snapshot of
// the pre-Go 1.22 ServeMux implementation for backwards compatibility.
// Do not modify this file, it should remain frozen.
using godebug = @internal.godebug_package;
using url = net.url_package;
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;
using @internal;

partial class http_package {

internal static ж<godebug.Setting> httpmuxgo121 = godebug.New("httpmuxgo121"u8);

internal static bool use121;

// Read httpmuxgo121 once at startup, since dealing with changes to it during
// program execution is too complex and error-prone.
[GoInit] internal static void init() {
    if (httpmuxgo121.Value() == "1"u8) {
        use121 = true;
        httpmuxgo121.IncNonDefault();
    }
}

// serveMux121 holds the state of a ServeMux needed for Go 1.21 behavior.
[GoType] partial struct serveMux121 {
    internal sync_package.RWMutex mu;
    internal map<@string, muxEntry> m;
    internal slice<muxEntry> es; // slice of entries sorted from longest to shortest.
    internal bool hosts;       // whether any patterns contain hostnames
}

[GoType] partial struct muxEntry {
    internal ΔHandler h;
    internal @string pattern;
}

// Formerly ServeMux.Handle.
[GoRecv] internal static void handle(this ref serveMux121 mux, @string pattern, ΔHandler handler) => func((defer, _) => {
    mux.mu.Lock();
    defer(mux.mu.Unlock);
    if (pattern == ""u8) {
        throw panic("http: invalid pattern");
    }
    if (handler == default!) {
        throw panic("http: nil handler");
    }
    {
        var (_, exist) = mux.m[pattern]; if (exist) {
            throw panic("http: multiple registrations for "u8 + pattern);
        }
    }
    if (mux.m == default!) {
        mux.m = new map<@string, muxEntry>();
    }
    var e = new muxEntry(h: handler, pattern: pattern);
    mux.m[pattern] = e;
    if (pattern[len(pattern) - 1] == (rune)'/') {
        mux.es = appendSorted(mux.es, e);
    }
    if (pattern[0] != (rune)'/') {
        mux.hosts = true;
    }
});

internal static slice<muxEntry> appendSorted(slice<muxEntry> es, muxEntry e) {
    nint n = len(es);
    nint i = sort.Search(n, 
    var eʗ1 = e;
    var esʗ1 = es;
    (nint i) => len(esʗ1[iΔ1].pattern) < len(eʗ1.pattern));
    if (i == n) {
        return append(es, e);
    }
    // we now know that i points at where we want to insert
    es = append(es, new muxEntry(nil));
    // try to grow the slice in place, any entry works.
    copy(es[(int)(i + 1)..], es[(int)(i)..]);
    // Move shorter entries down
    es[i] = e;
    return es;
}

// Formerly ServeMux.HandleFunc.
[GoRecv] internal static void handleFunc(this ref serveMux121 mux, @string pattern, http.Request) handler) {
    if (handler == default!) {
        throw panic("http: nil handler");
    }
    mux.handle(pattern, ((HandlerFunc)handler));
}

// Formerly ServeMux.Handler.
[GoRecv] internal static (ΔHandler h, @string pattern) findHandler(this ref serveMux121 mux, ж<Request> Ꮡr) {
    ΔHandler h = default!;
    @string pattern = default!;

    ref var r = ref Ꮡr.val;
    // CONNECT requests are not canonicalized.
    if (r.Method == "CONNECT"u8) {
        // If r.URL.Path is /tree and its handler is not registered,
        // the /tree -> /tree/ redirect applies to CONNECT requests
        // but the path canonicalization does not.
        {
            var (u, ok) = mux.redirectToPathSlash(r.URL.Host, r.URL.Path, r.URL); if (ok) {
                return (RedirectHandler(u.String(), StatusMovedPermanently), (~u).Path);
            }
        }
        return mux.handler(r.Host, r.URL.Path);
    }
    // All other requests have any port stripped and path cleaned
    // before passing to mux.handler.
    @string host = stripHostPort(r.Host);
    @string path = cleanPath(r.URL.Path);
    // If the given path is /tree and its handler is not registered,
    // redirect for /tree/.
    {
        var (u, ok) = mux.redirectToPathSlash(host, path, r.URL); if (ok) {
            return (RedirectHandler(u.String(), StatusMovedPermanently), (~u).Path);
        }
    }
    if (path != r.URL.Path) {
        (_, pattern) = mux.handler(host, path);
        var u = Ꮡ(new url.URL(Path: path, RawQuery: r.URL.RawQuery));
        return (RedirectHandler(u.String(), StatusMovedPermanently), pattern);
    }
    return mux.handler(host, r.URL.Path);
}

// handler is the main implementation of findHandler.
// The path is known to be in canonical form, except for CONNECT methods.
[GoRecv] internal static (ΔHandler h, @string pattern) handler(this ref serveMux121 mux, @string host, @string path) => func((defer, _) => {
    ΔHandler h = default!;
    @string pattern = default!;

    mux.mu.RLock();
    defer(mux.mu.RUnlock);
    // Host-specific pattern takes precedence over generic ones
    if (mux.hosts) {
        (h, pattern) = mux.match(host + path);
    }
    if (h == default!) {
        (h, pattern) = mux.match(path);
    }
    if (h == default!) {
        (h, pattern) = (NotFoundHandler(), ""u8);
    }
    return (h, pattern);
});

// Find a handler on a handler map given a path string.
// Most-specific (longest) pattern wins.
[GoRecv] internal static (ΔHandler h, @string pattern) match(this ref serveMux121 mux, @string path) {
    ΔHandler h = default!;
    @string pattern = default!;

    // Check for exact match first.
    var (v, ok) = mux.m[path];
    if (ok) {
        return (v.h, v.pattern);
    }
    // Check for longest valid match.  mux.es contains all patterns
    // that end in / sorted from longest to shortest.
    foreach (var (_, e) in mux.es) {
        if (strings.HasPrefix(path, e.pattern)) {
            return (e.h, e.pattern);
        }
    }
    return (default!, "");
}

// redirectToPathSlash determines if the given path needs appending "/" to it.
// This occurs when a handler for path + "/" was already registered, but
// not for path itself. If the path needs appending to, it creates a new
// URL, setting the path to u.Path + "/" and returning true to indicate so.
[GoRecv] internal static (ж<url.URL>, bool) redirectToPathSlash(this ref serveMux121 mux, @string host, @string path, ж<url.URL> Ꮡu) {
    ref var u = ref Ꮡu.val;

    mux.mu.RLock();
    var shouldRedirect = mux.shouldRedirectRLocked(host, path);
    mux.mu.RUnlock();
    if (!shouldRedirect) {
        return (Ꮡu, false);
    }
    path = path + "/"u8;
    Ꮡu = Ꮡ(new url.URL(Path: path, RawQuery: u.RawQuery)); u = ref Ꮡu.val;
    return (Ꮡu, true);
}

// shouldRedirectRLocked reports whether the given path and host should be redirected to
// path+"/". This should happen if a handler is registered for path+"/" but
// not path -- see comments at ServeMux.
[GoRecv] internal static bool shouldRedirectRLocked(this ref serveMux121 mux, @string host, @string path) {
    var p = new @string[]{path, host + path}.slice();
    foreach (var (_, c) in p) {
        {
            var (_, exist) = mux.m[c]; if (exist) {
                return false;
            }
        }
    }
    nint n = len(path);
    if (n == 0) {
        return false;
    }
    foreach (var (_, c) in p) {
        {
            var (_, exist) = mux.m[c + "/"u8]; if (exist) {
                return path[n - 1] != (rune)'/';
            }
        }
    }
    return false;
}

} // end http_package
