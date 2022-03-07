// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fork, exec, wait, etc.

// package windows -- go2cs converted at 2022 March 06 23:30:34 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\exec_windows.go
using errorspkg = go.errors_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

    // EscapeArg rewrites command line argument s as prescribed
    // in http://msdn.microsoft.com/en-us/library/ms880421.
    // This function returns "" (2 double quotes) if s is empty.
    // Alternatively, these transformations are done:
    // - every back slash (\) is doubled, but only if immediately
    //   followed by double quote (");
    // - every double quote (") is escaped by back slash (\);
    // - finally, s is wrapped with double quotes (arg -> "arg"),
    //   but only if there is space or tab inside s.
public static @string EscapeArg(@string s) {
    if (len(s) == 0) {
        return "\"\"";
    }
    var n = len(s);
    var hasSpace = false;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(s); i++) {
            switch (s[i]) {
                case '"': 

                case '\\': 
                    n++;
                    break;
                case ' ': 

                case '\t': 
                    hasSpace = true;
                    break;
            }

        }

        i = i__prev1;
    }
    if (hasSpace) {
        n += 2;
    }
    if (n == len(s)) {
        return s;
    }
    var qs = make_slice<byte>(n);
    nint j = 0;
    if (hasSpace) {
        qs[j] = '"';
        j++;
    }
    nint slashes = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i < len(s); i++) {
            switch (s[i]) {
                case '\\': 
                    slashes++;
                    qs[j] = s[i];
                    break;
                case '"': 
                    while (slashes > 0) {
                        qs[j] = '\\';
                        j++;
                        slashes--;
                    }
                    qs[j] = '\\';
                    j++;
                    qs[j] = s[i];

                    break;
                default: 
                    slashes = 0;
                    qs[j] = s[i];
                    break;
            }
            j++;

        }

        i = i__prev1;
    }
    if (hasSpace) {
        while (slashes > 0) {
            qs[j] = '\\';
            j++;
            slashes--;
        }
        qs[j] = '"';
        j++;

    }
    return string(qs[..(int)j]);

}

public static void CloseOnExec(Handle fd) {
    SetHandleInformation(Handle(fd), HANDLE_FLAG_INHERIT, 0);
}

// FullPath retrieves the full path of the specified file.
public static (@string, error) FullPath(@string name) {
    @string path = default;
    error err = default!;

    var (p, err) = UTF16PtrFromString(name);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var n = uint32(100);
    while (true) {
        var buf = make_slice<ushort>(n);
        n, err = GetFullPathName(p, uint32(len(buf)), _addr_buf[0], null);
        if (err != null) {
            return ("", error.As(err)!);
        }
        if (n <= uint32(len(buf))) {
            return (UTF16ToString(buf[..(int)n]), error.As(null!)!);
        }
    }

}

// NewProcThreadAttributeList allocates a new ProcThreadAttributeList, with the requested maximum number of attributes.
public static (ptr<ProcThreadAttributeList>, error) NewProcThreadAttributeList(uint maxAttrCount) {
    ptr<ProcThreadAttributeList> _p0 = default!;
    error _p0 = default!;

    ref System.UIntPtr size = ref heap(out ptr<System.UIntPtr> _addr_size);
    var err = initializeProcThreadAttributeList(null, maxAttrCount, 0, _addr_size);
    if (err != ERROR_INSUFFICIENT_BUFFER) {
        if (err == null) {
            return (_addr_null!, error.As(errorspkg.New("unable to query buffer size from InitializeProcThreadAttributeList"))!);
        }
        return (_addr_null!, error.As(err)!);

    }
    const var psize = @unsafe.Sizeof(uintptr(0)); 
    // size is guaranteed to be ≥1 by InitializeProcThreadAttributeList.
 
    // size is guaranteed to be ≥1 by InitializeProcThreadAttributeList.
    var al = (ProcThreadAttributeList.val)(@unsafe.Pointer(_addr_make_slice<unsafe.Pointer>((size + psize - 1) / psize)[0]));
    err = initializeProcThreadAttributeList(al, maxAttrCount, 0, _addr_size);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_al!, error.As(err)!);

}

// Update modifies the ProcThreadAttributeList using UpdateProcThreadAttribute.
private static error Update(this ptr<ProcThreadAttributeList> _addr_al, System.UIntPtr attribute, uint flags, unsafe.Pointer value, System.UIntPtr size, unsafe.Pointer prevValue, ptr<System.UIntPtr> _addr_returnedSize) {
    ref ProcThreadAttributeList al = ref _addr_al.val;
    ref System.UIntPtr returnedSize = ref _addr_returnedSize.val;

    return error.As(updateProcThreadAttribute(al, flags, attribute, value, size, prevValue, returnedSize))!;
}

// Delete frees ProcThreadAttributeList's resources.
private static void Delete(this ptr<ProcThreadAttributeList> _addr_al) {
    ref ProcThreadAttributeList al = ref _addr_al.val;

    deleteProcThreadAttributeList(al);
}

} // end windows_package
