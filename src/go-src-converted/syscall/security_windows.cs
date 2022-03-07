// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:26:46 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\security_windows.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

public static readonly nuint STANDARD_RIGHTS_REQUIRED = 0xf0000;
public static readonly nuint STANDARD_RIGHTS_READ = 0x20000;
public static readonly nuint STANDARD_RIGHTS_WRITE = 0x20000;
public static readonly nuint STANDARD_RIGHTS_EXECUTE = 0x20000;
public static readonly nuint STANDARD_RIGHTS_ALL = 0x1F0000;


public static readonly nint NameUnknown = 0;
public static readonly nint NameFullyQualifiedDN = 1;
public static readonly nint NameSamCompatible = 2;
public static readonly nint NameDisplay = 3;
public static readonly nint NameUniqueId = 6;
public static readonly nint NameCanonical = 7;
public static readonly nint NameUserPrincipal = 8;
public static readonly nint NameCanonicalEx = 9;
public static readonly nint NameServicePrincipal = 10;
public static readonly nint NameDnsDomain = 12;


// This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
// https://blogs.msdn.com/b/drnick/archive/2007/12/19/windows-and-upn-format-credentials.aspx
//sys    TranslateName(accName *uint16, accNameFormat uint32, desiredNameFormat uint32, translatedName *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.TranslateNameW
//sys    GetUserNameEx(nameFormat uint32, nameBuffre *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.GetUserNameExW

// TranslateAccountName converts a directory service
// object name from one format to another.
public static (@string, error) TranslateAccountName(@string username, uint from, uint to, nint initSize) {
    @string _p0 = default;
    error _p0 = default!;

    var (u, e) = UTF16PtrFromString(username);
    if (e != null) {
        return ("", error.As(e)!);
    }
    ref var n = ref heap(uint32(50), out ptr<var> _addr_n);
    while (true) {
        var b = make_slice<ushort>(n);
        e = TranslateName(u, from, to, _addr_b[0], _addr_n);
        if (e == null) {
            return (UTF16ToString(b[..(int)n]), error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return ("", error.As(e)!);
        }
    }

}

 
// do not reorder
public static readonly var NetSetupUnknownStatus = iota;
public static readonly var NetSetupUnjoined = 0;
public static readonly var NetSetupWorkgroupName = 1;
public static readonly var NetSetupDomainName = 2;


public partial struct UserInfo10 {
    public ptr<ushort> Name;
    public ptr<ushort> Comment;
    public ptr<ushort> UsrComment;
    public ptr<ushort> FullName;
}

//sys    NetUserGetInfo(serverName *uint16, userName *uint16, level uint32, buf **byte) (neterr error) = netapi32.NetUserGetInfo
//sys    NetGetJoinInformation(server *uint16, name **uint16, bufType *uint32) (neterr error) = netapi32.NetGetJoinInformation
//sys    NetApiBufferFree(buf *byte) (neterr error) = netapi32.NetApiBufferFree

 
// do not reorder
public static readonly nint SidTypeUser = 1 + iota;
public static readonly var SidTypeGroup = 0;
public static readonly var SidTypeDomain = 1;
public static readonly var SidTypeAlias = 2;
public static readonly var SidTypeWellKnownGroup = 3;
public static readonly var SidTypeDeletedAccount = 4;
public static readonly var SidTypeInvalid = 5;
public static readonly var SidTypeUnknown = 6;
public static readonly var SidTypeComputer = 7;
public static readonly var SidTypeLabel = 8;


//sys    LookupAccountSid(systemName *uint16, sid *SID, name *uint16, nameLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountSidW
//sys    LookupAccountName(systemName *uint16, accountName *uint16, sid *SID, sidLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountNameW
//sys    ConvertSidToStringSid(sid *SID, stringSid **uint16) (err error) = advapi32.ConvertSidToStringSidW
//sys    ConvertStringSidToSid(stringSid *uint16, sid **SID) (err error) = advapi32.ConvertStringSidToSidW
//sys    GetLengthSid(sid *SID) (len uint32) = advapi32.GetLengthSid
//sys    CopySid(destSidLen uint32, destSid *SID, srcSid *SID) (err error) = advapi32.CopySid

// The security identifier (SID) structure is a variable-length
// structure used to uniquely identify users or groups.
public partial struct SID {
}

// StringToSid converts a string-format security identifier
// sid into a valid, functional sid.
public static (ptr<SID>, error) StringToSid(@string s) => func((defer, _, _) => {
    ptr<SID> _p0 = default!;
    error _p0 = default!;

    ptr<SID> sid;
    var (p, e) = UTF16PtrFromString(s);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    e = ConvertStringSidToSid(p, _addr_sid);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    defer(LocalFree((Handle)(@unsafe.Pointer(sid))));
    return _addr_sid.Copy()!;

});

// LookupSID retrieves a security identifier sid for the account
// and the name of the domain on which the account was found.
// System specify target computer to search.
public static (ptr<SID>, @string, uint, error) LookupSID(@string system, @string account) {
    ptr<SID> sid = default!;
    @string domain = default;
    uint accType = default;
    error err = default!;

    if (len(account) == 0) {
        return (_addr_null!, "", 0, error.As(EINVAL)!);
    }
    var (acc, e) = UTF16PtrFromString(account);
    if (e != null) {
        return (_addr_null!, "", 0, error.As(e)!);
    }
    ptr<ushort> sys;
    if (len(system) > 0) {
        sys, e = UTF16PtrFromString(system);
        if (e != null) {
            return (_addr_null!, "", 0, error.As(e)!);
        }
    }
    ref var n = ref heap(uint32(50), out ptr<var> _addr_n);
    ref var dn = ref heap(uint32(50), out ptr<var> _addr_dn);
    while (true) {
        var b = make_slice<byte>(n);
        var db = make_slice<ushort>(dn);
        sid = (SID.val)(@unsafe.Pointer(_addr_b[0]));
        e = LookupAccountName(sys, acc, sid, _addr_n, _addr_db[0], _addr_dn, _addr_accType);
        if (e == null) {
            return (_addr_sid!, UTF16ToString(db), accType, error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return (_addr_null!, "", 0, error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return (_addr_null!, "", 0, error.As(e)!);
        }
    }

}

// String converts sid to a string format
// suitable for display, storage, or transmission.
private static (@string, error) String(this ptr<SID> _addr_sid) => func((defer, _, _) => {
    @string _p0 = default;
    error _p0 = default!;
    ref SID sid = ref _addr_sid.val;

    ptr<ushort> s;
    var e = ConvertSidToStringSid(sid, _addr_s);
    if (e != null) {
        return ("", error.As(e)!);
    }
    defer(LocalFree((Handle)(@unsafe.Pointer(s))));
    return (utf16PtrToString(s), error.As(null!)!);

});

// Len returns the length, in bytes, of a valid security identifier sid.
private static nint Len(this ptr<SID> _addr_sid) {
    ref SID sid = ref _addr_sid.val;

    return int(GetLengthSid(sid));
}

// Copy creates a duplicate of security identifier sid.
private static (ptr<SID>, error) Copy(this ptr<SID> _addr_sid) {
    ptr<SID> _p0 = default!;
    error _p0 = default!;
    ref SID sid = ref _addr_sid.val;

    var b = make_slice<byte>(sid.Len());
    var sid2 = (SID.val)(@unsafe.Pointer(_addr_b[0]));
    var e = CopySid(uint32(len(b)), sid2, sid);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    return (_addr_sid2!, error.As(null!)!);

}

// LookupAccount retrieves the name of the account for this sid
// and the name of the first domain on which this sid is found.
// System specify target computer to search for.
private static (@string, @string, uint, error) LookupAccount(this ptr<SID> _addr_sid, @string system) {
    @string account = default;
    @string domain = default;
    uint accType = default;
    error err = default!;
    ref SID sid = ref _addr_sid.val;

    ptr<ushort> sys;
    if (len(system) > 0) {
        sys, err = UTF16PtrFromString(system);
        if (err != null) {
            return ("", "", 0, error.As(err)!);
        }
    }
    ref var n = ref heap(uint32(50), out ptr<var> _addr_n);
    ref var dn = ref heap(uint32(50), out ptr<var> _addr_dn);
    while (true) {
        var b = make_slice<ushort>(n);
        var db = make_slice<ushort>(dn);
        var e = LookupAccountSid(sys, sid, _addr_b[0], _addr_n, _addr_db[0], _addr_dn, _addr_accType);
        if (e == null) {
            return (UTF16ToString(b), UTF16ToString(db), accType, error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", "", 0, error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return ("", "", 0, error.As(e)!);
        }
    }

}

 
// do not reorder
public static readonly nint TOKEN_ASSIGN_PRIMARY = 1 << (int)(iota);
public static readonly var TOKEN_DUPLICATE = 0;
public static readonly var TOKEN_IMPERSONATE = 1;
public static readonly var TOKEN_QUERY = 2;
public static readonly var TOKEN_QUERY_SOURCE = 3;
public static readonly var TOKEN_ADJUST_PRIVILEGES = 4;
public static readonly var TOKEN_ADJUST_GROUPS = 5;
public static readonly var TOKEN_ADJUST_DEFAULT = 6;
public static readonly TOKEN_ALL_ACCESS TOKEN_ADJUST_SESSIONID = STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID;
public static readonly var TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;
public static readonly var TOKEN_WRITE = STANDARD_RIGHTS_WRITE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT;
public static readonly var TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;


 
// do not reorder
public static readonly nint TokenUser = 1 + iota;
public static readonly var TokenGroups = 0;
public static readonly var TokenPrivileges = 1;
public static readonly var TokenOwner = 2;
public static readonly var TokenPrimaryGroup = 3;
public static readonly var TokenDefaultDacl = 4;
public static readonly var TokenSource = 5;
public static readonly var TokenType = 6;
public static readonly var TokenImpersonationLevel = 7;
public static readonly var TokenStatistics = 8;
public static readonly var TokenRestrictedSids = 9;
public static readonly var TokenSessionId = 10;
public static readonly var TokenGroupsAndPrivileges = 11;
public static readonly var TokenSessionReference = 12;
public static readonly var TokenSandBoxInert = 13;
public static readonly var TokenAuditPolicy = 14;
public static readonly var TokenOrigin = 15;
public static readonly var TokenElevationType = 16;
public static readonly var TokenLinkedToken = 17;
public static readonly var TokenElevation = 18;
public static readonly var TokenHasRestrictions = 19;
public static readonly var TokenAccessInformation = 20;
public static readonly var TokenVirtualizationAllowed = 21;
public static readonly var TokenVirtualizationEnabled = 22;
public static readonly var TokenIntegrityLevel = 23;
public static readonly var TokenUIAccess = 24;
public static readonly var TokenMandatoryPolicy = 25;
public static readonly var TokenLogonSid = 26;
public static readonly var MaxTokenInfoClass = 27;


public partial struct SIDAndAttributes {
    public ptr<SID> Sid;
    public uint Attributes;
}

public partial struct Tokenuser {
    public SIDAndAttributes User;
}

public partial struct Tokenprimarygroup {
    public ptr<SID> PrimaryGroup;
}

//sys    OpenProcessToken(h Handle, access uint32, token *Token) (err error) = advapi32.OpenProcessToken
//sys    GetTokenInformation(t Token, infoClass uint32, info *byte, infoLen uint32, returnedLen *uint32) (err error) = advapi32.GetTokenInformation
//sys    GetUserProfileDirectory(t Token, dir *uint16, dirLen *uint32) (err error) = userenv.GetUserProfileDirectoryW
//sys    getSystemDirectory(dir *uint16, dirLen uint32) (len uint32, err error) = kernel32.GetSystemDirectoryW

// An access token contains the security information for a logon session.
// The system creates an access token when a user logs on, and every
// process executed on behalf of the user has a copy of the token.
// The token identifies the user, the user's groups, and the user's
// privileges. The system uses the token to control access to securable
// objects and to control the ability of the user to perform various
// system-related operations on the local computer.
public partial struct Token { // : Handle
}

// OpenCurrentProcessToken opens the access token
// associated with current process.
public static (Token, error) OpenCurrentProcessToken() {
    Token _p0 = default;
    error _p0 = default!;

    var (p, e) = GetCurrentProcess();
    if (e != null) {
        return (0, error.As(e)!);
    }
    ref Token t = ref heap(out ptr<Token> _addr_t);
    e = OpenProcessToken(p, TOKEN_QUERY, _addr_t);
    if (e != null) {
        return (0, error.As(e)!);
    }
    return (t, error.As(null!)!);

}

// Close releases access to access token.
public static error Close(this Token t) {
    return error.As(CloseHandle(Handle(t)))!;
}

// getInfo retrieves a specified type of information about an access token.
public static (unsafe.Pointer, error) getInfo(this Token t, uint @class, nint initSize) {
    unsafe.Pointer _p0 = default;
    error _p0 = default!;

    ref var n = ref heap(uint32(initSize), out ptr<var> _addr_n);
    while (true) {
        var b = make_slice<byte>(n);
        var e = GetTokenInformation(t, class, _addr_b[0], uint32(len(b)), _addr_n);
        if (e == null) {
            return (@unsafe.Pointer(_addr_b[0]), error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return (null, error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return (null, error.As(e)!);
        }
    }

}

// GetTokenUser retrieves access token t user account information.
public static (ptr<Tokenuser>, error) GetTokenUser(this Token t) {
    ptr<Tokenuser> _p0 = default!;
    error _p0 = default!;

    var (i, e) = t.getInfo(TokenUser, 50);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    return (_addr_(Tokenuser.val)(i)!, error.As(null!)!);

}

// GetTokenPrimaryGroup retrieves access token t primary group information.
// A pointer to a SID structure representing a group that will become
// the primary group of any objects created by a process using this access token.
public static (ptr<Tokenprimarygroup>, error) GetTokenPrimaryGroup(this Token t) {
    ptr<Tokenprimarygroup> _p0 = default!;
    error _p0 = default!;

    var (i, e) = t.getInfo(TokenPrimaryGroup, 50);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    return (_addr_(Tokenprimarygroup.val)(i)!, error.As(null!)!);

}

// GetUserProfileDirectory retrieves path to the
// root directory of the access token t user's profile.
public static (@string, error) GetUserProfileDirectory(this Token t) {
    @string _p0 = default;
    error _p0 = default!;

    ref var n = ref heap(uint32(100), out ptr<var> _addr_n);
    while (true) {
        var b = make_slice<ushort>(n);
        var e = GetUserProfileDirectory(t, _addr_b[0], _addr_n);
        if (e == null) {
            return (UTF16ToString(b), error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return ("", error.As(e)!);
        }
    }

}

} // end syscall_package
