// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testlog provides a back-channel communication path
// between tests and package os, so that cmd/go can see which
// environment variables and files a test consults.
namespace go.@internal;

using atomic = sync.atomic_package;
using sync;

partial class testlog_package {

// Interface is the interface required of test loggers.
// The os package will invoke the interface's methods to indicate that
// it is inspecting the given environment variables or files.
// Multiple goroutines may call these methods simultaneously.
[GoType] partial interface Interface {
    void Getenv(@string key);
    void Stat(@string file);
    void Open(@string file);
    void Chdir(@string dir);
}

// logger is the current logger Interface.
// We use an atomic.Value in case test startup
// is racing with goroutines started during init.
// That must not cause a race detector failure,
// although it will still result in limited visibility
// into exactly what those goroutines do.
internal static atomic.Value logger;

// SetLogger sets the test logger implementation for the current process.
// It must be called only once, at process startup.
public static void SetLogger(Interface impl) {
    if (logger.Load() != default!) {
        throw panic("testlog: SetLogger must be called only once");
    }
    logger.Store(Ꮡ(impl));
}

// Logger returns the current test logger implementation.
// It returns nil if there is no logger.
public static Interface Logger() {
    var impl = logger.Load();
    if (impl == default!) {
        return default!;
    }
    return impl._<Interface.val>().val;
}

// Getenv calls Logger().Getenv, if a logger has been set.
public static void Getenv(@string name) {
    {
        var log = Logger(); if (log != default!) {
            log.Getenv(name);
        }
    }
}

// Open calls Logger().Open, if a logger has been set.
public static void Open(@string name) {
    {
        var log = Logger(); if (log != default!) {
            log.Open(name);
        }
    }
}

// Stat calls Logger().Stat, if a logger has been set.
public static void Stat(@string name) {
    {
        var log = Logger(); if (log != default!) {
            log.Stat(name);
        }
    }
}

} // end testlog_package
