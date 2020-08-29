// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,cgo darwin,cgo

// package plugin -- go2cs converted at 2020 August 29 10:11:11 UTC
// import "plugin" ==> using plugin = go.plugin_package
// Original source: C:\Go\src\plugin\plugin_dlopen.go
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
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class plugin_package
    {
        // avoid a dependency on strings
        private static long lastIndexByte(@string s, byte c)
        {
            for (var i = len(s) - 1L; i >= 0L; i--)
            {
                if (s[i] == c)
                {
                    return i;
                }
            }
            return -1L;
        }

        private static (ref Plugin, error) open(@string name)
        {
            var cPath = make_slice<byte>(C.PATH_MAX + 1L);
            var cRelName = make_slice<byte>(len(name) + 1L);
            copy(cRelName, name);
            if (C.realpath((C.@char.Value)(@unsafe.Pointer(ref cRelName[0L])), (C.@char.Value)(@unsafe.Pointer(ref cPath[0L]))) == null)
            {
                return (null, errors.New("plugin.Open(\"" + name + "\"): realpath failed"));
            }
            var filepath = C.GoString((C.@char.Value)(@unsafe.Pointer(ref cPath[0L])));

            pluginsMu.Lock();
            {
                var p__prev1 = p;

                var p = plugins[filepath];

                if (p != null)
                {
                    pluginsMu.Unlock();
                    if (p.err != "")
                    {
                        return (null, errors.New("plugin.Open(\"" + name + "\"): " + p.err + " (previous failure)"));
                    }
                    p.loaded.Receive();
                    return (p, null);
                }

                p = p__prev1;

            }
            ref C.char cErr = default;
            var h = C.pluginOpen((C.@char.Value)(@unsafe.Pointer(ref cPath[0L])), ref cErr);
            if (h == 0L)
            {
                pluginsMu.Unlock();
                return (null, errors.New("plugin.Open(\"" + name + "\"): " + C.GoString(cErr)));
            } 
            // TODO(crawshaw): look for plugin note, confirm it is a Go plugin
            // and it was built with the correct toolchain.
            if (len(name) > 3L && name[len(name) - 3L..] == ".so")
            {
                name = name[..len(name) - 3L];
            }
            if (plugins == null)
            {
                plugins = make_map<@string, ref Plugin>();
            }
            var (pluginpath, syms, errstr) = lastmoduleinit();
            if (errstr != "")
            {
                plugins[filepath] = ref new Plugin(pluginpath:pluginpath,err:errstr,);
                pluginsMu.Unlock();
                return (null, errors.New("plugin.Open(\"" + name + "\"): " + errstr));
            } 
            // This function can be called from the init function of a plugin.
            // Drop a placeholder in the map so subsequent opens can wait on it.
            p = ref new Plugin(pluginpath:pluginpath,loaded:make(chanstruct{}),);
            plugins[filepath] = p;
            pluginsMu.Unlock();

            var initStr = make_slice<byte>(len(pluginpath) + 6L);
            copy(initStr, pluginpath);
            copy(initStr[len(pluginpath)..], ".init");

            var initFuncPC = C.pluginLookup(h, (C.@char.Value)(@unsafe.Pointer(ref initStr[0L])), ref cErr);
            if (initFuncPC != null)
            {
                var initFuncP = ref initFuncPC;
                *(*Action) initFunc = @unsafe.Pointer(ref initFuncP).Value;
                initFunc();
            } 

            // Fill out the value of each plugin symbol.
            foreach (var (symName, sym) in syms)
            {
                var isFunc = symName[0L] == '.';
                if (isFunc)
                {
                    delete(syms, symName);
                    symName = symName[1L..];
                }
                var fullName = pluginpath + "." + symName;
                var cname = make_slice<byte>(len(fullName) + 1L);
                copy(cname, fullName);

                p = C.pluginLookup(h, (C.@char.Value)(@unsafe.Pointer(ref cname[0L])), ref cErr);
                if (p == null)
                {
                    return (null, errors.New("plugin.Open(\"" + name + "\"): could not find symbol " + symName + ": " + C.GoString(cErr)));
                }
                ref array<unsafe.Pointer> valp = new ptr<ref array<unsafe.Pointer>>(@unsafe.Pointer(ref sym));
                if (isFunc)
                {
                    (valp.Value)[1L] = @unsafe.Pointer(ref p);
                }
                else
                {
                    (valp.Value)[1L] = p;
                } 
                // we can't add to syms during iteration as we'll end up processing
                // some symbols twice with the inability to tell if the symbol is a function
                updatedSyms[symName] = sym;
            }
            p.syms = updatedSyms;

            close(p.loaded);
            return (p, null);
        }

        private static (Symbol, error) lookup(ref Plugin p, @string symName)
        {
            {
                var s = p.syms[symName];

                if (s != null)
                {
                    return (s, null);
                }

            }
            return (null, errors.New("plugin: symbol " + symName + " not found in plugin " + p.pluginpath));
        }

        private static sync.Mutex pluginsMu = default;        private static map<@string, ref Plugin> plugins = default;

        // lastmoduleinit is defined in package runtime
        private static (@string, object, @string) lastmoduleinit()
;
    }
}
