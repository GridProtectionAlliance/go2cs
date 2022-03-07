// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (linux && cgo) || (darwin && cgo) || (freebsd && cgo)
// +build linux,cgo darwin,cgo freebsd,cgo

// package plugin -- go2cs converted at 2022 March 06 23:36:30 UTC
// import "plugin" ==> using plugin = go.plugin_package
// Original source: C:\Program Files\Go\src\plugin\plugin_dlopen.go
/*
#cgo linux LDFLAGS: -ldl
#include <dlfcn.h>
#include <limits.h>
#include <stdlib.h>
#include <stdint.h>

#include <stdio.h>

static uintptr_t pluginOpen(const char* path, char** err) {
    void* h = dlopen(path, RTLD_NOW|RTLD_GLOBAL);
    if (h == NULL) {
        *err = (char*)dlerror();
    }
    return (uintptr_t)h;
}

static void* pluginLookup(uintptr_t h, const char* name, char** err) {
    void* r = dlsym((void*)h, name);
    if (r == NULL) {
        *err = (char*)dlerror();
    }
    return r;
}
*/
using C = go.C_package;/*
#cgo linux LDFLAGS: -ldl
#include <dlfcn.h>
#include <limits.h>
#include <stdlib.h>
#include <stdint.h>

#include <stdio.h>

static uintptr_t pluginOpen(const char* path, char** err) {
    void* h = dlopen(path, RTLD_NOW|RTLD_GLOBAL);
    if (h == NULL) {
        *err = (char*)dlerror();
    }
    return (uintptr_t)h;
}

static void* pluginLookup(uintptr_t h, const char* name, char** err) {
    void* r = dlsym((void*)h, name);
    if (r == NULL) {
        *err = (char*)dlerror();
    }
    return r;
}
*/


using errors = go.errors_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class plugin_package {

private static (ptr<Plugin>, error) open(@string name) {
    ptr<Plugin> _p0 = default!;
    error _p0 = default!;

    var cPath = make_slice<byte>(C.PATH_MAX + 1);
    var cRelName = make_slice<byte>(len(name) + 1);
    copy(cRelName, name);
    if (C.realpath((C.@char.val)(@unsafe.Pointer(_addr_cRelName[0])), (C.@char.val)(@unsafe.Pointer(_addr_cPath[0]))) == null) {
        return (_addr_null!, error.As(errors.New("plugin.Open(\"" + name + "\"): realpath failed"))!);
    }
    var filepath = C.GoString((C.@char.val)(@unsafe.Pointer(_addr_cPath[0])));

    pluginsMu.Lock();
    {
        var p__prev1 = p;

        ref var p = ref heap(plugins[filepath], out ptr<var> _addr_p);

        if (p != null) {
            pluginsMu.Unlock();
            if (p.err != "") {
                return (_addr_null!, error.As(errors.New("plugin.Open(\"" + name + "\"): " + p.err + " (previous failure)"))!);
            }
            p.loaded.Receive();
            return (_addr_p!, error.As(null!)!);

        }
        p = p__prev1;

    }

    ptr<C.char> cErr;
    var h = C.pluginOpen((C.@char.val)(@unsafe.Pointer(_addr_cPath[0])), _addr_cErr);
    if (h == 0) {
        pluginsMu.Unlock();
        return (_addr_null!, error.As(errors.New("plugin.Open(\"" + name + "\"): " + C.GoString(cErr)))!);
    }
    if (len(name) > 3 && name[(int)len(name) - 3..] == ".so") {
        name = name[..(int)len(name) - 3];
    }
    if (plugins == null) {
        plugins = make_map<@string, ptr<Plugin>>();
    }
    var (pluginpath, syms, errstr) = lastmoduleinit();
    if (errstr != "") {
        plugins[filepath] = addr(new Plugin(pluginpath:pluginpath,err:errstr,));
        pluginsMu.Unlock();
        return (_addr_null!, error.As(errors.New("plugin.Open(\"" + name + "\"): " + errstr))!);
    }
    p = addr(new Plugin(pluginpath:pluginpath,loaded:make(chanstruct{}),));
    plugins[filepath] = p;
    pluginsMu.Unlock();

    var initStr = make_slice<byte>(len(pluginpath) + len("..inittask") + 1); // +1 for terminating NUL
    copy(initStr, pluginpath);
    copy(initStr[(int)len(pluginpath)..], "..inittask");

    var initTask = C.pluginLookup(h, (C.@char.val)(@unsafe.Pointer(_addr_initStr[0])), _addr_cErr);
    if (initTask != null) {
        doInit(initTask);
    }
    foreach (var (symName, sym) in syms) {
        var isFunc = symName[0] == '.';
        if (isFunc) {
            delete(syms, symName);
            symName = symName[(int)1..];
        }
        var fullName = pluginpath + "." + symName;
        var cname = make_slice<byte>(len(fullName) + 1);
        copy(cname, fullName);

        p = C.pluginLookup(h, (C.@char.val)(@unsafe.Pointer(_addr_cname[0])), _addr_cErr);
        if (p == null) {
            return (_addr_null!, error.As(errors.New("plugin.Open(\"" + name + "\"): could not find symbol " + symName + ": " + C.GoString(cErr)))!);
        }
        ptr<array<unsafe.Pointer>> valp = new ptr<ptr<array<unsafe.Pointer>>>(@unsafe.Pointer(_addr_sym));
        if (isFunc) {
            (valp.val)[1] = @unsafe.Pointer(_addr_p);
        }
        else
 {
            (valp.val)[1] = p;
        }
        updatedSyms[symName] = sym;

    }    p.syms = updatedSyms;

    close(p.loaded);
    return (_addr_p!, error.As(null!)!);

}

private static (Symbol, error) lookup(ptr<Plugin> _addr_p, @string symName) {
    Symbol _p0 = default;
    error _p0 = default!;
    ref Plugin p = ref _addr_p.val;

    {
        var s = p.syms[symName];

        if (s != null) {
            return (s, error.As(null!)!);
        }
    }

    return (null, error.As(errors.New("plugin: symbol " + symName + " not found in plugin " + p.pluginpath))!);

}

private static sync.Mutex pluginsMu = default;private static map<@string, ptr<Plugin>> plugins = default;

// lastmoduleinit is defined in package runtime
private static (@string, object, @string) lastmoduleinit();

// doInit is defined in package runtime
//go:linkname doInit runtime.doInit
private static void doInit(unsafe.Pointer t); // t should be a *runtime.initTask

} // end plugin_package
