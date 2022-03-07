// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// Package registry provides access to the Windows registry.
//
// Here is a simple example, opening a registry key and reading a string value from it.
//
//    k, err := registry.OpenKey(registry.LOCAL_MACHINE, `SOFTWARE\Microsoft\Windows NT\CurrentVersion`, registry.QUERY_VALUE)
//    if err != nil {
//        log.Fatal(err)
//    }
//    defer k.Close()
//
//    s, _, err := k.GetStringValue("SystemRoot")
//    if err != nil {
//        log.Fatal(err)
//    }
//    fmt.Printf("Windows system root is %q\n", s)
//
// NOTE: This package is a copy of golang.org/x/sys/windows/registry
// with KeyInfo.ModTime removed to prevent dependency cycles.
//
// package registry -- go2cs converted at 2022 March 06 22:13:14 UTC
// import "internal/syscall/windows/registry" ==> using registry = go.@internal.syscall.windows.registry_package
// Original source: C:\Program Files\Go\src\internal\syscall\windows\registry\key.go
using syscall = go.syscall_package;

namespace go.@internal.syscall.windows;

public static partial class registry_package {

 
// Registry key security and access rights.
// See https://msdn.microsoft.com/en-us/library/windows/desktop/ms724878.aspx
// for details.
public static readonly nuint ALL_ACCESS = 0xf003f;
public static readonly nuint CREATE_LINK = 0x00020;
public static readonly nuint CREATE_SUB_KEY = 0x00004;
public static readonly nuint ENUMERATE_SUB_KEYS = 0x00008;
public static readonly nuint EXECUTE = 0x20019;
public static readonly nuint NOTIFY = 0x00010;
public static readonly nuint QUERY_VALUE = 0x00001;
public static readonly nuint READ = 0x20019;
public static readonly nuint SET_VALUE = 0x00002;
public static readonly nuint WOW64_32KEY = 0x00200;
public static readonly nuint WOW64_64KEY = 0x00100;
public static readonly nuint WRITE = 0x20006;


// Key is a handle to an open Windows registry key.
// Keys can be obtained by calling OpenKey; there are
// also some predefined root keys such as CURRENT_USER.
// Keys can be used directly in the Windows API.
public partial struct Key { // : syscall.Handle
}

 
// Windows defines some predefined root keys that are always open.
// An application can use these keys as entry points to the registry.
// Normally these keys are used in OpenKey to open new keys,
// but they can also be used anywhere a Key is required.
public static readonly var CLASSES_ROOT = Key(syscall.HKEY_CLASSES_ROOT);
public static readonly var CURRENT_USER = Key(syscall.HKEY_CURRENT_USER);
public static readonly var LOCAL_MACHINE = Key(syscall.HKEY_LOCAL_MACHINE);
public static readonly var USERS = Key(syscall.HKEY_USERS);
public static readonly var CURRENT_CONFIG = Key(syscall.HKEY_CURRENT_CONFIG);


// Close closes open key k.
public static error Close(this Key k) {
    return error.As(syscall.RegCloseKey(syscall.Handle(k)))!;
}

// OpenKey opens a new key with path name relative to key k.
// It accepts any open key, including CURRENT_USER and others,
// and returns the new key and an error.
// The access parameter specifies desired access rights to the
// key to be opened.
public static (Key, error) OpenKey(Key k, @string path, uint access) {
    Key _p0 = default;
    error _p0 = default!;

    var (p, err) = syscall.UTF16PtrFromString(path);
    if (err != null) {
        return (0, error.As(err)!);
    }
    ref syscall.Handle subkey = ref heap(out ptr<syscall.Handle> _addr_subkey);
    err = syscall.RegOpenKeyEx(syscall.Handle(k), p, 0, access, _addr_subkey);
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (Key(subkey), error.As(null!)!);

}

// ReadSubKeyNames returns the names of subkeys of key k.
public static (slice<@string>, error) ReadSubKeyNames(this Key k) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    var names = make_slice<@string>(0); 
    // Registry key size limit is 255 bytes and described there:
    // https://msdn.microsoft.com/library/windows/desktop/ms724872.aspx
    var buf = make_slice<ushort>(256); //plus extra room for terminating zero byte
loopItems:
    for (var i = uint32(0); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
        ref var l = ref heap(uint32(len(buf)), out ptr<var> _addr_l);
        while (true) {
            var err = syscall.RegEnumKeyEx(syscall.Handle(k), i, _addr_buf[0], _addr_l, null, null, null, null);
            if (err == null) {
                break;
            }
            if (err == syscall.ERROR_MORE_DATA) { 
                // Double buffer size and try again.
                l = uint32(2 * len(buf));
                buf = make_slice<ushort>(l);
                continue;

            }

            if (err == _ERROR_NO_MORE_ITEMS) {
                _breakloopItems = true;
                break;
            }

            return (names, error.As(err)!);

        }
        names = append(names, syscall.UTF16ToString(buf[..(int)l]));

    }
    return (names, error.As(null!)!);

}

// CreateKey creates a key named path under open key k.
// CreateKey returns the new key and a boolean flag that reports
// whether the key already existed.
// The access parameter specifies the access rights for the key
// to be created.
public static (Key, bool, error) CreateKey(Key k, @string path, uint access) {
    Key newk = default;
    bool openedExisting = default;
    error err = default!;

    ref syscall.Handle h = ref heap(out ptr<syscall.Handle> _addr_h);
    ref uint d = ref heap(out ptr<uint> _addr_d);
    err = regCreateKeyEx(syscall.Handle(k), syscall.StringToUTF16Ptr(path), 0, null, _REG_OPTION_NON_VOLATILE, access, null, _addr_h, _addr_d);
    if (err != null) {
        return (0, false, error.As(err)!);
    }
    return (Key(h), d == _REG_OPENED_EXISTING_KEY, error.As(null!)!);

}

// DeleteKey deletes the subkey path of key k and its values.
public static error DeleteKey(Key k, @string path) {
    return error.As(regDeleteKey(syscall.Handle(k), syscall.StringToUTF16Ptr(path)))!;
}

// A KeyInfo describes the statistics of a key. It is returned by Stat.
public partial struct KeyInfo {
    public uint SubKeyCount;
    public uint MaxSubKeyLen; // size of the key's subkey with the longest name, in Unicode characters, not including the terminating zero byte
    public uint ValueCount;
    public uint MaxValueNameLen; // size of the key's longest value name, in Unicode characters, not including the terminating zero byte
    public uint MaxValueLen; // longest data component among the key's values, in bytes
    public syscall.Filetime lastWriteTime;
}

// Stat retrieves information about the open key k.
public static (ptr<KeyInfo>, error) Stat(this Key k) {
    ptr<KeyInfo> _p0 = default!;
    error _p0 = default!;

    ref KeyInfo ki = ref heap(out ptr<KeyInfo> _addr_ki);
    var err = syscall.RegQueryInfoKey(syscall.Handle(k), null, null, null, _addr_ki.SubKeyCount, _addr_ki.MaxSubKeyLen, null, _addr_ki.ValueCount, _addr_ki.MaxValueNameLen, _addr_ki.MaxValueLen, null, _addr_ki.lastWriteTime);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr__addr_ki!, error.As(null!)!);

}

} // end registry_package
