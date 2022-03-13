// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// package registry -- go2cs converted at 2022 March 13 06:44:31 UTC
// import "internal/syscall/windows/registry" ==> using registry = go.@internal.syscall.windows.registry_package
// Original source: C:\Program Files\Go\src\internal\syscall\windows\registry\syscall.go
namespace go.@internal.syscall.windows;

using syscall = syscall_package;

public static partial class registry_package {

private static readonly nint _REG_OPTION_NON_VOLATILE = 0;

private static readonly nint _REG_CREATED_NEW_KEY = 1;
private static readonly nint _REG_OPENED_EXISTING_KEY = 2;

private static readonly syscall.Errno _ERROR_NO_MORE_ITEMS = 259;

public static error LoadRegLoadMUIString() {
    return error.As(procRegLoadMUIStringW.Find())!;
}

//sys    regCreateKeyEx(key syscall.Handle, subkey *uint16, reserved uint32, class *uint16, options uint32, desired uint32, sa *syscall.SecurityAttributes, result *syscall.Handle, disposition *uint32) (regerrno error) = advapi32.RegCreateKeyExW
//sys    regDeleteKey(key syscall.Handle, subkey *uint16) (regerrno error) = advapi32.RegDeleteKeyW
//sys    regSetValueEx(key syscall.Handle, valueName *uint16, reserved uint32, vtype uint32, buf *byte, bufsize uint32) (regerrno error) = advapi32.RegSetValueExW
//sys    regEnumValue(key syscall.Handle, index uint32, name *uint16, nameLen *uint32, reserved *uint32, valtype *uint32, buf *byte, buflen *uint32) (regerrno error) = advapi32.RegEnumValueW
//sys    regDeleteValue(key syscall.Handle, name *uint16) (regerrno error) = advapi32.RegDeleteValueW
//sys   regLoadMUIString(key syscall.Handle, name *uint16, buf *uint16, buflen uint32, buflenCopied *uint32, flags uint32, dir *uint16) (regerrno error) = advapi32.RegLoadMUIStringW

//sys    expandEnvironmentStrings(src *uint16, dst *uint16, size uint32) (n uint32, err error) = kernel32.ExpandEnvironmentStringsW

} // end registry_package
