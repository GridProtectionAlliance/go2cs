// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// package registry -- go2cs converted at 2022 March 06 22:13:16 UTC
// import "internal/syscall/windows/registry" ==> using registry = go.@internal.syscall.windows.registry_package
// Original source: C:\Program Files\Go\src\internal\syscall\windows\registry\value.go
using errors = go.errors_package;
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal.syscall.windows;

public static partial class registry_package {

 
// Registry value types.
public static readonly nint NONE = 0;
public static readonly nint SZ = 1;
public static readonly nint EXPAND_SZ = 2;
public static readonly nint BINARY = 3;
public static readonly nint DWORD = 4;
public static readonly nint DWORD_BIG_ENDIAN = 5;
public static readonly nint LINK = 6;
public static readonly nint MULTI_SZ = 7;
public static readonly nint RESOURCE_LIST = 8;
public static readonly nint FULL_RESOURCE_DESCRIPTOR = 9;
public static readonly nint RESOURCE_REQUIREMENTS_LIST = 10;
public static readonly nint QWORD = 11;


 
// ErrShortBuffer is returned when the buffer was too short for the operation.
public static var ErrShortBuffer = syscall.ERROR_MORE_DATA;public static var ErrNotExist = syscall.ERROR_FILE_NOT_FOUND;public static var ErrUnexpectedType = errors.New("unexpected key value type");

// GetValue retrieves the type and data for the specified value associated
// with an open key k. It fills up buffer buf and returns the retrieved
// byte count n. If buf is too small to fit the stored value it returns
// ErrShortBuffer error along with the required buffer size n.
// If no buffer is provided, it returns true and actual buffer size n.
// If no buffer is provided, GetValue returns the value's type only.
// If the value does not exist, the error returned is ErrNotExist.
//
// GetValue is a low level function. If value's type is known, use the appropriate
// Get*Value function instead.
public static (nint, uint, error) GetValue(this Key k, @string name, slice<byte> buf) {
    nint n = default;
    uint valtype = default;
    error err = default!;

    var (pname, err) = syscall.UTF16PtrFromString(name);
    if (err != null) {
        return (0, 0, error.As(err)!);
    }
    ptr<byte> pbuf;
    if (len(buf) > 0) {
        pbuf = (byte.val)(@unsafe.Pointer(_addr_buf[0]));
    }
    ref var l = ref heap(uint32(len(buf)), out ptr<var> _addr_l);
    err = syscall.RegQueryValueEx(syscall.Handle(k), pname, null, _addr_valtype, pbuf, _addr_l);
    if (err != null) {
        return (int(l), valtype, error.As(err)!);
    }
    return (int(l), valtype, error.As(null!)!);

}

public static (slice<byte>, uint, error) getValue(this Key k, @string name, slice<byte> buf) {
    slice<byte> date = default;
    uint valtype = default;
    error err = default!;

    var (p, err) = syscall.UTF16PtrFromString(name);
    if (err != null) {
        return (null, 0, error.As(err)!);
    }
    ref uint t = ref heap(out ptr<uint> _addr_t);
    ref var n = ref heap(uint32(len(buf)), out ptr<var> _addr_n);
    while (true) {
        err = syscall.RegQueryValueEx(syscall.Handle(k), p, null, _addr_t, (byte.val)(@unsafe.Pointer(_addr_buf[0])), _addr_n);
        if (err == null) {
            return (buf[..(int)n], t, error.As(null!)!);
        }
        if (err != syscall.ERROR_MORE_DATA) {
            return (null, 0, error.As(err)!);
        }
        if (n <= uint32(len(buf))) {
            return (null, 0, error.As(err)!);
        }
        buf = make_slice<byte>(n);

    }

}

// GetStringValue retrieves the string value for the specified
// value name associated with an open key k. It also returns the value's type.
// If value does not exist, GetStringValue returns ErrNotExist.
// If value is not SZ or EXPAND_SZ, it will return the correct value
// type and ErrUnexpectedType.
public static (@string, uint, error) GetStringValue(this Key k, @string name) {
    @string val = default;
    uint valtype = default;
    error err = default!;

    var (data, typ, err2) = k.getValue(name, make_slice<byte>(64));
    if (err2 != null) {
        return ("", typ, error.As(err2)!);
    }

    if (typ == SZ || typ == EXPAND_SZ)     else 
        return ("", typ, error.As(ErrUnexpectedType)!);
        if (len(data) == 0) {
        return ("", typ, error.As(null!)!);
    }
    ptr<array<ushort>> u = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(_addr_data[0])).slice(-1, len(data) / 2, len(data) / 2);
    return (syscall.UTF16ToString(u), typ, error.As(null!)!);

}

// GetMUIStringValue retrieves the localized string value for
// the specified value name associated with an open key k.
// If the value name doesn't exist or the localized string value
// can't be resolved, GetMUIStringValue returns ErrNotExist.
// GetMUIStringValue panics if the system doesn't support
// regLoadMUIString; use LoadRegLoadMUIString to check if
// regLoadMUIString is supported before calling this function.
public static (@string, error) GetMUIStringValue(this Key k, @string name) {
    @string _p0 = default;
    error _p0 = default!;

    var (pname, err) = syscall.UTF16PtrFromString(name);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var buf = make_slice<ushort>(1024);
    ref uint buflen = ref heap(out ptr<uint> _addr_buflen);
    ptr<ushort> pdir;

    err = regLoadMUIString(syscall.Handle(k), pname, _addr_buf[0], uint32(len(buf)), _addr_buflen, 0, pdir);
    if (err == syscall.ERROR_FILE_NOT_FOUND) { // Try fallback path

        // Try to resolve the string value using the system directory as
        // a DLL search path; this assumes the string value is of the form
        // @[path]\dllname,-strID but with no path given, e.g. @tzres.dll,-320.

        // This approach works with tzres.dll but may have to be revised
        // in the future to allow callers to provide custom search paths.

        @string s = default;
        s, err = ExpandString("%SystemRoot%\\system32\\");
        if (err != null) {
            return ("", error.As(err)!);
        }
        pdir, err = syscall.UTF16PtrFromString(s);
        if (err != null) {
            return ("", error.As(err)!);
        }
        err = regLoadMUIString(syscall.Handle(k), pname, _addr_buf[0], uint32(len(buf)), _addr_buflen, 0, pdir);

    }
    while (err == syscall.ERROR_MORE_DATA) { // Grow buffer if needed
        if (buflen <= uint32(len(buf))) {
            break; // Buffer not growing, assume race; break
        }
        buf = make_slice<ushort>(buflen);
        err = regLoadMUIString(syscall.Handle(k), pname, _addr_buf[0], uint32(len(buf)), _addr_buflen, 0, pdir);

    }

    if (err != null) {
        return ("", error.As(err)!);
    }
    return (syscall.UTF16ToString(buf), error.As(null!)!);

}

// ExpandString expands environment-variable strings and replaces
// them with the values defined for the current user.
// Use ExpandString to expand EXPAND_SZ strings.
public static (@string, error) ExpandString(@string value) {
    @string _p0 = default;
    error _p0 = default!;

    if (value == "") {
        return ("", error.As(null!)!);
    }
    var (p, err) = syscall.UTF16PtrFromString(value);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var r = make_slice<ushort>(100);
    while (true) {
        var (n, err) = expandEnvironmentStrings(p, _addr_r[0], uint32(len(r)));
        if (err != null) {
            return ("", error.As(err)!);
        }
        if (n <= uint32(len(r))) {
            return (syscall.UTF16ToString(r[..(int)n]), error.As(null!)!);
        }
        r = make_slice<ushort>(n);

    }

}

// GetStringsValue retrieves the []string value for the specified
// value name associated with an open key k. It also returns the value's type.
// If value does not exist, GetStringsValue returns ErrNotExist.
// If value is not MULTI_SZ, it will return the correct value
// type and ErrUnexpectedType.
public static (slice<@string>, uint, error) GetStringsValue(this Key k, @string name) {
    slice<@string> val = default;
    uint valtype = default;
    error err = default!;

    var (data, typ, err2) = k.getValue(name, make_slice<byte>(64));
    if (err2 != null) {
        return (null, typ, error.As(err2)!);
    }
    if (typ != MULTI_SZ) {
        return (null, typ, error.As(ErrUnexpectedType)!);
    }
    if (len(data) == 0) {
        return (null, typ, error.As(null!)!);
    }
    ptr<array<ushort>> p = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(_addr_data[0])).slice(-1, len(data) / 2, len(data) / 2);
    if (len(p) == 0) {
        return (null, typ, error.As(null!)!);
    }
    if (p[len(p) - 1] == 0) {
        p = p[..(int)len(p) - 1]; // remove terminating null
    }
    val = make_slice<@string>(0, 5);
    nint from = 0;
    foreach (var (i, c) in p) {
        if (c == 0) {
            val = append(val, string(utf16.Decode(p[(int)from..(int)i])));
            from = i + 1;
        }
    }    return (val, typ, error.As(null!)!);

}

// GetIntegerValue retrieves the integer value for the specified
// value name associated with an open key k. It also returns the value's type.
// If value does not exist, GetIntegerValue returns ErrNotExist.
// If value is not DWORD or QWORD, it will return the correct value
// type and ErrUnexpectedType.
public static (ulong, uint, error) GetIntegerValue(this Key k, @string name) {
    ulong val = default;
    uint valtype = default;
    error err = default!;

    var (data, typ, err2) = k.getValue(name, make_slice<byte>(8));
    if (err2 != null) {
        return (0, typ, error.As(err2)!);
    }

    if (typ == DWORD) 
        if (len(data) != 4) {
            return (0, typ, error.As(errors.New("DWORD value is not 4 bytes long"))!);
        }
        return (uint64(new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(_addr_data[0]))), DWORD, error.As(null!)!);
    else if (typ == QWORD) 
        if (len(data) != 8) {
            return (0, typ, error.As(errors.New("QWORD value is not 8 bytes long"))!);
        }
        return (uint64(new ptr<ptr<ptr<ulong>>>(@unsafe.Pointer(_addr_data[0]))), QWORD, error.As(null!)!);
    else 
        return (0, typ, error.As(ErrUnexpectedType)!);
    
}

// GetBinaryValue retrieves the binary value for the specified
// value name associated with an open key k. It also returns the value's type.
// If value does not exist, GetBinaryValue returns ErrNotExist.
// If value is not BINARY, it will return the correct value
// type and ErrUnexpectedType.
public static (slice<byte>, uint, error) GetBinaryValue(this Key k, @string name) {
    slice<byte> val = default;
    uint valtype = default;
    error err = default!;

    var (data, typ, err2) = k.getValue(name, make_slice<byte>(64));
    if (err2 != null) {
        return (null, typ, error.As(err2)!);
    }
    if (typ != BINARY) {
        return (null, typ, error.As(ErrUnexpectedType)!);
    }
    return (data, typ, error.As(null!)!);

}

public static error setValue(this Key k, @string name, uint valtype, slice<byte> data) {
    var (p, err) = syscall.UTF16PtrFromString(name);
    if (err != null) {
        return error.As(err)!;
    }
    if (len(data) == 0) {
        return error.As(regSetValueEx(syscall.Handle(k), p, 0, valtype, null, 0))!;
    }
    return error.As(regSetValueEx(syscall.Handle(k), p, 0, valtype, _addr_data[0], uint32(len(data))))!;

}

// SetDWordValue sets the data and type of a name value
// under key k to value and DWORD.
public static error SetDWordValue(this Key k, @string name, uint value) {
    return error.As(k.setValue(name, DWORD, new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_value))[..]))!;
}

// SetQWordValue sets the data and type of a name value
// under key k to value and QWORD.
public static error SetQWordValue(this Key k, @string name, ulong value) {
    return error.As(k.setValue(name, QWORD, new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_value))[..]))!;
}

public static error setStringValue(this Key k, @string name, uint valtype, @string value) {
    var (v, err) = syscall.UTF16FromString(value);
    if (err != null) {
        return error.As(err)!;
    }
    ptr<array<byte>> buf = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_v[0])).slice(-1, len(v) * 2, len(v) * 2);
    return error.As(k.setValue(name, valtype, buf))!;

}

// SetStringValue sets the data and type of a name value
// under key k to value and SZ. The value must not contain a zero byte.
public static error SetStringValue(this Key k, @string name, @string value) {
    return error.As(k.setStringValue(name, SZ, value))!;
}

// SetExpandStringValue sets the data and type of a name value
// under key k to value and EXPAND_SZ. The value must not contain a zero byte.
public static error SetExpandStringValue(this Key k, @string name, @string value) {
    return error.As(k.setStringValue(name, EXPAND_SZ, value))!;
}

// SetStringsValue sets the data and type of a name value
// under key k to value and MULTI_SZ. The value strings
// must not contain a zero byte.
public static error SetStringsValue(this Key k, @string name, slice<@string> value) {
    @string ss = "";
    foreach (var (_, s) in value) {
        for (nint i = 0; i < len(s); i++) {
            if (s[i] == 0) {
                return error.As(errors.New("string cannot have 0 inside"))!;
            }
        }
        ss += s + "\x00";
    }    var v = utf16.Encode((slice<int>)ss + "\x00");
    ptr<array<byte>> buf = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_v[0])).slice(-1, len(v) * 2, len(v) * 2);
    return error.As(k.setValue(name, MULTI_SZ, buf))!;
}

// SetBinaryValue sets the data and type of a name value
// under key k to value and BINARY.
public static error SetBinaryValue(this Key k, @string name, slice<byte> value) {
    return error.As(k.setValue(name, BINARY, value))!;
}

// DeleteValue removes a named value from the key k.
public static error DeleteValue(this Key k, @string name) {
    return error.As(regDeleteValue(syscall.Handle(k), syscall.StringToUTF16Ptr(name)))!;
}

// ReadValueNames returns the value names of key k.
public static (slice<@string>, error) ReadValueNames(this Key k) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    var (ki, err) = k.Stat();
    if (err != null) {
        return (null, error.As(err)!);
    }
    var names = make_slice<@string>(0, ki.ValueCount);
    var buf = make_slice<ushort>(ki.MaxValueNameLen + 1); // extra room for terminating null character
loopItems:
    for (var i = uint32(0); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
        ref var l = ref heap(uint32(len(buf)), out ptr<var> _addr_l);
        while (true) {
            var err = regEnumValue(syscall.Handle(k), i, _addr_buf[0], _addr_l, null, null, null, null);
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

} // end registry_package
