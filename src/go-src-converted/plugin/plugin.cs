// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package plugin implements loading and symbol resolution of Go plugins.
//
// A plugin is a Go main package with exported functions and variables that
// has been built with:
//
//    go build -buildmode=plugin
//
// When a plugin is first opened, the init functions of all packages not
// already part of the program are called. The main function is not run.
// A plugin is only initialized once, and cannot be closed.
//
// Currently plugins are only supported on Linux, FreeBSD, and macOS.
// Please report any issues.
// package plugin -- go2cs converted at 2022 March 06 23:36:29 UTC
// import "plugin" ==> using plugin = go.plugin_package
// Original source: C:\Program Files\Go\src\plugin\plugin.go


namespace go;

public static partial class plugin_package {

    // Plugin is a loaded Go plugin.
public partial struct Plugin {
    public @string pluginpath;
    public @string err; // set if plugin failed to load
    public channel<object> loaded; // closed when loaded
}

// Open opens a Go plugin.
// If a path has already been opened, then the existing *Plugin is returned.
// It is safe for concurrent use by multiple goroutines.
public static (ptr<Plugin>, error) Open(@string path) {
    ptr<Plugin> _p0 = default!;
    error _p0 = default!;

    return _addr_open(path)!;
}

// Lookup searches for a symbol named symName in plugin p.
// A symbol is any exported variable or function.
// It reports an error if the symbol is not found.
// It is safe for concurrent use by multiple goroutines.
private static (Symbol, error) Lookup(this ptr<Plugin> _addr_p, @string symName) {
    Symbol _p0 = default;
    error _p0 = default!;
    ref Plugin p = ref _addr_p.val;

    return lookup(p, symName);
}

// A Symbol is a pointer to a variable or function.
//
// For example, a plugin defined as
//
//    package main
//
//    import "fmt"
//
//    var V int
//
//    func F() { fmt.Printf("Hello, number %d\n", V) }
//
// may be loaded with the Open function and then the exported package
// symbols V and F can be accessed
//
//    p, err := plugin.Open("plugin_name.so")
//    if err != nil {
//        panic(err)
//    }
//    v, err := p.Lookup("V")
//    if err != nil {
//        panic(err)
//    }
//    f, err := p.Lookup("F")
//    if err != nil {
//        panic(err)
//    }
//    *v.(*int) = 7
//    f.(func())() // prints "Hello, number 7"
public partial interface Symbol {
}

} // end plugin_package
