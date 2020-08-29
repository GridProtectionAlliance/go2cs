// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// package registry -- go2cs converted at 2020 August 29 08:22:40 UTC
// import "internal/syscall/windows/registry" ==> using registry = go.@internal.syscall.windows.registry_package
// Original source: C:\Go\src\internal\syscall\windows\registry\value.go
using errors = go.errors_package;
using io = go.io_package;
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall {
namespace windows
{
    public static unsafe partial class registry_package
    {
 
        // Registry value types.
        public static readonly long NONE = 0L;
        public static readonly long SZ = 1L;
        public static readonly long EXPAND_SZ = 2L;
        public static readonly long BINARY = 3L;
        public static readonly long DWORD = 4L;
        public static readonly long DWORD_BIG_ENDIAN = 5L;
        public static readonly long LINK = 6L;
        public static readonly long MULTI_SZ = 7L;
        public static readonly long RESOURCE_LIST = 8L;
        public static readonly long FULL_RESOURCE_DESCRIPTOR = 9L;
        public static readonly long RESOURCE_REQUIREMENTS_LIST = 10L;
        public static readonly long QWORD = 11L;

 
        // ErrShortBuffer is returned when the buffer was too short for the operation.
        public static var ErrShortBuffer = syscall.ERROR_MORE_DATA;        public static var ErrNotExist = syscall.ERROR_FILE_NOT_FOUND;        public static var ErrUnexpectedType = errors.New("unexpected key value type");

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
        public static (long, uint, error) GetValue(this Key k, @string name, slice<byte> buf)
        {
            var (pname, err) = syscall.UTF16PtrFromString(name);
            if (err != null)
            {
                return (0L, 0L, err);
            }
            ref byte pbuf = default;
            if (len(buf) > 0L)
            {
                pbuf = (byte.Value)(@unsafe.Pointer(ref buf[0L]));
            }
            var l = uint32(len(buf));
            err = syscall.RegQueryValueEx(syscall.Handle(k), pname, null, ref valtype, pbuf, ref l);
            if (err != null)
            {
                return (int(l), valtype, err);
            }
            return (int(l), valtype, null);
        }

        public static (slice<byte>, uint, error) getValue(this Key k, @string name, slice<byte> buf)
        {
            var (p, err) = syscall.UTF16PtrFromString(name);
            if (err != null)
            {
                return (null, 0L, err);
            }
            uint t = default;
            var n = uint32(len(buf));
            while (true)
            {
                err = syscall.RegQueryValueEx(syscall.Handle(k), p, null, ref t, (byte.Value)(@unsafe.Pointer(ref buf[0L])), ref n);
                if (err == null)
                {
                    return (buf[..n], t, null);
                }
                if (err != syscall.ERROR_MORE_DATA)
                {
                    return (null, 0L, err);
                }
                if (n <= uint32(len(buf)))
                {
                    return (null, 0L, err);
                }
                buf = make_slice<byte>(n);
            }

        }

        // GetStringValue retrieves the string value for the specified
        // value name associated with an open key k. It also returns the value's type.
        // If value does not exist, GetStringValue returns ErrNotExist.
        // If value is not SZ or EXPAND_SZ, it will return the correct value
        // type and ErrUnexpectedType.
        public static (@string, uint, error) GetStringValue(this Key k, @string name)
        {
            var (data, typ, err2) = k.getValue(name, make_slice<byte>(64L));
            if (err2 != null)
            {
                return ("", typ, err2);
            }

            if (typ == SZ || typ == EXPAND_SZ)             else 
                return ("", typ, ErrUnexpectedType);
                        if (len(data) == 0L)
            {
                return ("", typ, null);
            }
            ref array<ushort> u = new ptr<ref array<ushort>>(@unsafe.Pointer(ref data[0L]))[..];
            return (syscall.UTF16ToString(u), typ, null);
        }

        // GetMUIStringValue retrieves the localized string value for
        // the specified value name associated with an open key k.
        // If the value name doesn't exist or the localized string value
        // can't be resolved, GetMUIStringValue returns ErrNotExist.
        // GetMUIStringValue panics if the system doesn't support
        // regLoadMUIString; use LoadRegLoadMUIString to check if
        // regLoadMUIString is supported before calling this function.
        public static (@string, error) GetMUIStringValue(this Key k, @string name)
        {
            var (pname, err) = syscall.UTF16PtrFromString(name);
            if (err != null)
            {
                return ("", err);
            }
            var buf = make_slice<ushort>(1024L);
            uint buflen = default;
            ref ushort pdir = default;

            err = regLoadMUIString(syscall.Handle(k), pname, ref buf[0L], uint32(len(buf)), ref buflen, 0L, pdir);
            if (err == syscall.ERROR_FILE_NOT_FOUND)
            { // Try fallback path

                // Try to resolve the string value using the system directory as
                // a DLL search path; this assumes the string value is of the form
                // @[path]\dllname,-strID but with no path given, e.g. @tzres.dll,-320.

                // This approach works with tzres.dll but may have to be revised
                // in the future to allow callers to provide custom search paths.

                @string s = default;
                s, err = ExpandString("%SystemRoot%\\system32\\");
                if (err != null)
                {
                    return ("", err);
                }
                pdir, err = syscall.UTF16PtrFromString(s);
                if (err != null)
                {
                    return ("", err);
                }
                err = regLoadMUIString(syscall.Handle(k), pname, ref buf[0L], uint32(len(buf)), ref buflen, 0L, pdir);
            }
            while (err == syscall.ERROR_MORE_DATA)
            { // Grow buffer if needed
                if (buflen <= uint32(len(buf)))
                {
                    break; // Buffer not growing, assume race; break
                }
                buf = make_slice<ushort>(buflen);
                err = regLoadMUIString(syscall.Handle(k), pname, ref buf[0L], uint32(len(buf)), ref buflen, 0L, pdir);
            }


            if (err != null)
            {
                return ("", err);
            }
            return (syscall.UTF16ToString(buf), null);
        }

        // ExpandString expands environment-variable strings and replaces
        // them with the values defined for the current user.
        // Use ExpandString to expand EXPAND_SZ strings.
        public static (@string, error) ExpandString(@string value)
        {
            if (value == "")
            {
                return ("", null);
            }
            var (p, err) = syscall.UTF16PtrFromString(value);
            if (err != null)
            {
                return ("", err);
            }
            var r = make_slice<ushort>(100L);
            while (true)
            {
                var (n, err) = expandEnvironmentStrings(p, ref r[0L], uint32(len(r)));
                if (err != null)
                {
                    return ("", err);
                }
                if (n <= uint32(len(r)))
                {
                    ref array<ushort> u = new ptr<ref array<ushort>>(@unsafe.Pointer(ref r[0L]))[..];
                    return (syscall.UTF16ToString(u), null);
                }
                r = make_slice<ushort>(n);
            }

        }

        // GetStringsValue retrieves the []string value for the specified
        // value name associated with an open key k. It also returns the value's type.
        // If value does not exist, GetStringsValue returns ErrNotExist.
        // If value is not MULTI_SZ, it will return the correct value
        // type and ErrUnexpectedType.
        public static (slice<@string>, uint, error) GetStringsValue(this Key k, @string name)
        {
            var (data, typ, err2) = k.getValue(name, make_slice<byte>(64L));
            if (err2 != null)
            {
                return (null, typ, err2);
            }
            if (typ != MULTI_SZ)
            {
                return (null, typ, ErrUnexpectedType);
            }
            if (len(data) == 0L)
            {
                return (null, typ, null);
            }
            ref array<ushort> p = new ptr<ref array<ushort>>(@unsafe.Pointer(ref data[0L]))[..len(data) / 2L];
            if (len(p) == 0L)
            {
                return (null, typ, null);
            }
            if (p[len(p) - 1L] == 0L)
            {
                p = p[..len(p) - 1L]; // remove terminating null
            }
            val = make_slice<@string>(0L, 5L);
            long from = 0L;
            foreach (var (i, c) in p)
            {
                if (c == 0L)
                {
                    val = append(val, string(utf16.Decode(p[from..i])));
                    from = i + 1L;
                }
            }
            return (val, typ, null);
        }

        // GetIntegerValue retrieves the integer value for the specified
        // value name associated with an open key k. It also returns the value's type.
        // If value does not exist, GetIntegerValue returns ErrNotExist.
        // If value is not DWORD or QWORD, it will return the correct value
        // type and ErrUnexpectedType.
        public static (ulong, uint, error) GetIntegerValue(this Key k, @string name)
        {
            var (data, typ, err2) = k.getValue(name, make_slice<byte>(8L));
            if (err2 != null)
            {
                return (0L, typ, err2);
            }

            if (typ == DWORD) 
                if (len(data) != 4L)
                {
                    return (0L, typ, errors.New("DWORD value is not 4 bytes long"));
                }
                return (uint64(@unsafe.Pointer(ref data[0L]).Value), DWORD, null);
            else if (typ == QWORD) 
                if (len(data) != 8L)
                {
                    return (0L, typ, errors.New("QWORD value is not 8 bytes long"));
                }
                return (uint64(@unsafe.Pointer(ref data[0L]).Value), QWORD, null);
            else 
                return (0L, typ, ErrUnexpectedType);
                    }

        // GetBinaryValue retrieves the binary value for the specified
        // value name associated with an open key k. It also returns the value's type.
        // If value does not exist, GetBinaryValue returns ErrNotExist.
        // If value is not BINARY, it will return the correct value
        // type and ErrUnexpectedType.
        public static (slice<byte>, uint, error) GetBinaryValue(this Key k, @string name)
        {
            var (data, typ, err2) = k.getValue(name, make_slice<byte>(64L));
            if (err2 != null)
            {
                return (null, typ, err2);
            }
            if (typ != BINARY)
            {
                return (null, typ, ErrUnexpectedType);
            }
            return (data, typ, null);
        }

        public static error setValue(this Key k, @string name, uint valtype, slice<byte> data)
        {
            var (p, err) = syscall.UTF16PtrFromString(name);
            if (err != null)
            {
                return error.As(err);
            }
            if (len(data) == 0L)
            {
                return error.As(regSetValueEx(syscall.Handle(k), p, 0L, valtype, null, 0L));
            }
            return error.As(regSetValueEx(syscall.Handle(k), p, 0L, valtype, ref data[0L], uint32(len(data))));
        }

        // SetDWordValue sets the data and type of a name value
        // under key k to value and DWORD.
        public static error SetDWordValue(this Key k, @string name, uint value)
        {
            return error.As(k.setValue(name, DWORD, new ptr<ref array<byte>>(@unsafe.Pointer(ref value))[..]));
        }

        // SetQWordValue sets the data and type of a name value
        // under key k to value and QWORD.
        public static error SetQWordValue(this Key k, @string name, ulong value)
        {
            return error.As(k.setValue(name, QWORD, new ptr<ref array<byte>>(@unsafe.Pointer(ref value))[..]));
        }

        public static error setStringValue(this Key k, @string name, uint valtype, @string value)
        {
            var (v, err) = syscall.UTF16FromString(value);
            if (err != null)
            {
                return error.As(err);
            }
            ref array<byte> buf = new ptr<ref array<byte>>(@unsafe.Pointer(ref v[0L]))[..len(v) * 2L];
            return error.As(k.setValue(name, valtype, buf));
        }

        // SetStringValue sets the data and type of a name value
        // under key k to value and SZ. The value must not contain a zero byte.
        public static error SetStringValue(this Key k, @string name, @string value)
        {
            return error.As(k.setStringValue(name, SZ, value));
        }

        // SetExpandStringValue sets the data and type of a name value
        // under key k to value and EXPAND_SZ. The value must not contain a zero byte.
        public static error SetExpandStringValue(this Key k, @string name, @string value)
        {
            return error.As(k.setStringValue(name, EXPAND_SZ, value));
        }

        // SetStringsValue sets the data and type of a name value
        // under key k to value and MULTI_SZ. The value strings
        // must not contain a zero byte.
        public static error SetStringsValue(this Key k, @string name, slice<@string> value)
        {
            @string ss = "";
            foreach (var (_, s) in value)
            {
                for (long i = 0L; i < len(s); i++)
                {
                    if (s[i] == 0L)
                    {
                        return error.As(errors.New("string cannot have 0 inside"));
                    }
                }

                ss += s + "\x00";
            }
            var v = utf16.Encode((slice<int>)ss + "\x00");
            ref array<byte> buf = new ptr<ref array<byte>>(@unsafe.Pointer(ref v[0L]))[..len(v) * 2L];
            return error.As(k.setValue(name, MULTI_SZ, buf));
        }

        // SetBinaryValue sets the data and type of a name value
        // under key k to value and BINARY.
        public static error SetBinaryValue(this Key k, @string name, slice<byte> value)
        {
            return error.As(k.setValue(name, BINARY, value));
        }

        // DeleteValue removes a named value from the key k.
        public static error DeleteValue(this Key k, @string name)
        {
            return error.As(regDeleteValue(syscall.Handle(k), syscall.StringToUTF16Ptr(name)));
        }

        // ReadValueNames returns the value names of key k.
        // The parameter n controls the number of returned names,
        // analogous to the way os.File.Readdirnames works.
        public static (slice<@string>, error) ReadValueNames(this Key k, long n)
        {
            var (ki, err) = k.Stat();
            if (err != null)
            {
                return (null, err);
            }
            var names = make_slice<@string>(0L, ki.ValueCount);
            var buf = make_slice<ushort>(ki.MaxValueNameLen + 1L); // extra room for terminating null character
loopItems:
            for (var i = uint32(0L); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                if (n > 0L)
                {
                    if (len(names) == n)
                    {
                        return (names, null);
                    }
                }
                var l = uint32(len(buf));
                while (true)
                {
                    var err = regEnumValue(syscall.Handle(k), i, ref buf[0L], ref l, null, null, null, null);
                    if (err == null)
                    {
                        break;
                    }
                    if (err == syscall.ERROR_MORE_DATA)
                    { 
                        // Double buffer size and try again.
                        l = uint32(2L * len(buf));
                        buf = make_slice<ushort>(l);
                        continue;
                    }
                    if (err == _ERROR_NO_MORE_ITEMS)
                    {
                        _breakloopItems = true;
                        break;
                    }
                    return (names, err);
                }

                names = append(names, syscall.UTF16ToString(buf[..l]));
            }
            if (n > len(names))
            {
                return (names, io.EOF);
            }
            return (names, null);
        }
    }
}}}}
