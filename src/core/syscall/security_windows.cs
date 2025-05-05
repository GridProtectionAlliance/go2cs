// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class syscall_package {

public static readonly UntypedInt STANDARD_RIGHTS_REQUIRED = /* 0xf0000 */ 983040;
public static readonly UntypedInt STANDARD_RIGHTS_READ = /* 0x20000 */ 131072;
public static readonly UntypedInt STANDARD_RIGHTS_WRITE = /* 0x20000 */ 131072;
public static readonly UntypedInt STANDARD_RIGHTS_EXECUTE = /* 0x20000 */ 131072;
public static readonly UntypedInt STANDARD_RIGHTS_ALL = /* 0x1F0000 */ 2031616;

public static readonly UntypedInt NameUnknown = 0;
public static readonly UntypedInt NameFullyQualifiedDN = 1;
public static readonly UntypedInt NameSamCompatible = 2;
public static readonly UntypedInt NameDisplay = 3;
public static readonly UntypedInt NameUniqueId = 6;
public static readonly UntypedInt NameCanonical = 7;
public static readonly UntypedInt NameUserPrincipal = 8;
public static readonly UntypedInt NameCanonicalEx = 9;
public static readonly UntypedInt NameServicePrincipal = 10;
public static readonly UntypedInt NameDnsDomain = 12;

// This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
// https://learn.microsoft.com/en-gb/archive/blogs/drnick/windows-and-upn-format-credentials
//sys	TranslateName(accName *uint16, accNameFormat uint32, desiredNameFormat uint32, translatedName *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.TranslateNameW
//sys	GetUserNameEx(nameFormat uint32, nameBuffre *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.GetUserNameExW

// TranslateAccountName converts a directory service
// object name from one format to another.
public static (@string, error) TranslateAccountName(@string username, uint32 from, uint32 to, nint initSize) {
    (u, e) = UTF16PtrFromString(username);
    if (e != default!) {
        return ("", e);
    }
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = ((uint32)50);
    while (ᐧ) {
        var b = new slice<uint16>(n);
        e = TranslateName(u, from, to, Ꮡ(b, 0), Ꮡn);
        if (e == default!) {
            return (UTF16ToString(b[..(int)(n)]), default!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", e);
        }
        if (n <= ((uint32)len(b))) {
            return ("", e);
        }
    }
}

public static readonly UntypedInt NetSetupUnknownStatus = iota;
public static readonly UntypedInt NetSetupUnjoined = 1;
public static readonly UntypedInt NetSetupWorkgroupName = 2;
public static readonly UntypedInt NetSetupDomainName = 3;

[GoType] partial struct UserInfo10 {
    public ж<uint16> Name;
    public ж<uint16> Comment;
    public ж<uint16> UsrComment;
    public ж<uint16> FullName;
}

//sys	NetUserGetInfo(serverName *uint16, userName *uint16, level uint32, buf **byte) (neterr error) = netapi32.NetUserGetInfo
//sys	NetGetJoinInformation(server *uint16, name **uint16, bufType *uint32) (neterr error) = netapi32.NetGetJoinInformation
//sys	NetApiBufferFree(buf *byte) (neterr error) = netapi32.NetApiBufferFree
public static readonly UntypedInt SidTypeUser = /* 1 + iota */ 1;
public static readonly UntypedInt SidTypeGroup = 2;
public static readonly UntypedInt SidTypeDomain = 3;
public static readonly UntypedInt SidTypeAlias = 4;
public static readonly UntypedInt SidTypeWellKnownGroup = 5;
public static readonly UntypedInt SidTypeDeletedAccount = 6;
public static readonly UntypedInt SidTypeInvalid = 7;
public static readonly UntypedInt SidTypeUnknown = 8;
public static readonly UntypedInt SidTypeComputer = 9;
public static readonly UntypedInt SidTypeLabel = 10;

//sys	LookupAccountSid(systemName *uint16, sid *SID, name *uint16, nameLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountSidW
//sys	LookupAccountName(systemName *uint16, accountName *uint16, sid *SID, sidLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountNameW
//sys	ConvertSidToStringSid(sid *SID, stringSid **uint16) (err error) = advapi32.ConvertSidToStringSidW
//sys	ConvertStringSidToSid(stringSid *uint16, sid **SID) (err error) = advapi32.ConvertStringSidToSidW
//sys	GetLengthSid(sid *SID) (len uint32) = advapi32.GetLengthSid
//sys	CopySid(destSidLen uint32, destSid *SID, srcSid *SID) (err error) = advapi32.CopySid

// The security identifier (SID) structure is a variable-length
// structure used to uniquely identify users or groups.
[GoType] partial struct SID {
}

// StringToSid converts a string-format security identifier
// sid into a valid, functional sid.
public static (ж<SID>, error) StringToSid(@string s) => func((defer, _) => {
    ж<SID> sid = default!;
    (p, e) = UTF16PtrFromString(s);
    if (e != default!) {
        return (default!, e);
    }
    e = ConvertStringSidToSid(p, Ꮡ(sid));
    if (e != default!) {
        return (default!, e);
    }
    deferǃ(LocalFree, ((ΔHandle)new @unsafe.Pointer(sid)), defer);
    return sid.Copy();
});

// LookupSID retrieves a security identifier sid for the account
// and the name of the domain on which the account was found.
// System specify target computer to search.
public static (ж<SID> sid, @string domain, uint32 accType, error err) LookupSID(@string system, @string account) {
    ж<SID> sid = default!;
    @string domain = default!;
    uint32 accType = default!;
    error err = default!;

    if (len(account) == 0) {
        return (default!, "", 0, EINVAL);
    }
    (acc, e) = UTF16PtrFromString(account);
    if (e != default!) {
        return (default!, "", 0, e);
    }
    ж<uint16> sys = default!;
    if (len(system) > 0) {
        (sys, e) = UTF16PtrFromString(system);
        if (e != default!) {
            return (default!, "", 0, e);
        }
    }
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = ((uint32)50);
    ref var dn = ref heap<uint32>(out var Ꮡdn);
    dn = ((uint32)50);
    while (ᐧ) {
        var b = new slice<byte>(n);
        var db = new slice<uint16>(dn);
        sid = (ж<SID>)(uintptr)(new @unsafe.Pointer(Ꮡ(b, 0)));
        e = LookupAccountName(sys, acc, sid, Ꮡn, Ꮡ(db, 0), Ꮡdn, Ꮡ(accType));
        if (e == default!) {
            return (sid, UTF16ToString(db), accType, default!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return (default!, "", 0, e);
        }
        if (n <= ((uint32)len(b))) {
            return (default!, "", 0, e);
        }
    }
}

// String converts sid to a string format
// suitable for display, storage, or transmission.
[GoRecv] public static (@string, error) String(this ref SID sid) => func((defer, _) => {
    ж<uint16> s = default!;
    var e = ConvertSidToStringSid(sid, Ꮡ(s));
    if (e != default!) {
        return ("", e);
    }
    deferǃ(LocalFree, ((ΔHandle)new @unsafe.Pointer(s)), defer);
    return (utf16PtrToString(s), default!);
});

// Len returns the length, in bytes, of a valid security identifier sid.
[GoRecv] public static nint Len(this ref SID sid) {
    return ((nint)GetLengthSid(sid));
}

// Copy creates a duplicate of security identifier sid.
[GoRecv] public static (ж<SID>, error) Copy(this ref SID sid) {
    var b = new slice<byte>(sid.Len());
    var sid2 = (ж<SID>)(uintptr)(new @unsafe.Pointer(Ꮡ(b, 0)));
    var e = CopySid(((uint32)len(b)), sid2, sid);
    if (e != default!) {
        return (default!, e);
    }
    return (sid2, default!);
}

// LookupAccount retrieves the name of the account for this sid
// and the name of the first domain on which this sid is found.
// System specify target computer to search for.
[GoRecv] public static (@string account, @string domain, uint32 accType, error err) LookupAccount(this ref SID sid, @string system) {
    @string account = default!;
    @string domain = default!;
    uint32 accType = default!;
    error err = default!;

    ж<uint16> sys = default!;
    if (len(system) > 0) {
        (sys, err) = UTF16PtrFromString(system);
        if (err != default!) {
            return ("", "", 0, err);
        }
    }
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = ((uint32)50);
    ref var dn = ref heap<uint32>(out var Ꮡdn);
    dn = ((uint32)50);
    while (ᐧ) {
        var b = new slice<uint16>(n);
        var db = new slice<uint16>(dn);
        var e = LookupAccountSid(sys, sid, Ꮡ(b, 0), Ꮡn, Ꮡ(db, 0), Ꮡdn, Ꮡ(accType));
        if (e == default!) {
            return (UTF16ToString(b), UTF16ToString(db), accType, default!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", "", 0, e);
        }
        if (n <= ((uint32)len(b))) {
            return ("", "", 0, e);
        }
    }
}

public static readonly UntypedInt TOKEN_ASSIGN_PRIMARY = /* 1 << iota */ 1;
public static readonly UntypedInt TOKEN_DUPLICATE = 2;
public static readonly UntypedInt TOKEN_IMPERSONATE = 4;
public static readonly UntypedInt TOKEN_QUERY = 8;
public static readonly UntypedInt TOKEN_QUERY_SOURCE = 16;
public static readonly UntypedInt TOKEN_ADJUST_PRIVILEGES = 32;
public static readonly UntypedInt TOKEN_ADJUST_GROUPS = 64;
public static readonly UntypedInt TOKEN_ADJUST_DEFAULT = 128;
public static readonly UntypedInt TOKEN_ADJUST_SESSIONID = 256;
public static readonly UntypedInt TOKEN_ALL_ACCESS = /* STANDARD_RIGHTS_REQUIRED |
	TOKEN_ASSIGN_PRIMARY |
	TOKEN_DUPLICATE |
	TOKEN_IMPERSONATE |
	TOKEN_QUERY |
	TOKEN_QUERY_SOURCE |
	TOKEN_ADJUST_PRIVILEGES |
	TOKEN_ADJUST_GROUPS |
	TOKEN_ADJUST_DEFAULT |
	TOKEN_ADJUST_SESSIONID */ 983551;
public static readonly UntypedInt TOKEN_READ = /* STANDARD_RIGHTS_READ | TOKEN_QUERY */ 131080;
public static readonly UntypedInt TOKEN_WRITE = /* STANDARD_RIGHTS_WRITE |
	TOKEN_ADJUST_PRIVILEGES |
	TOKEN_ADJUST_GROUPS |
	TOKEN_ADJUST_DEFAULT */ 131296;
public static readonly UntypedInt TOKEN_EXECUTE = /* STANDARD_RIGHTS_EXECUTE */ 131072;

public static readonly UntypedInt TokenUser = /* 1 + iota */ 1;
public static readonly UntypedInt TokenGroups = 2;
public static readonly UntypedInt TokenPrivileges = 3;
public static readonly UntypedInt TokenOwner = 4;
public static readonly UntypedInt TokenPrimaryGroup = 5;
public static readonly UntypedInt TokenDefaultDacl = 6;
public static readonly UntypedInt TokenSource = 7;
public static readonly UntypedInt TokenType = 8;
public static readonly UntypedInt TokenImpersonationLevel = 9;
public static readonly UntypedInt TokenStatistics = 10;
public static readonly UntypedInt TokenRestrictedSids = 11;
public static readonly UntypedInt TokenSessionId = 12;
public static readonly UntypedInt TokenGroupsAndPrivileges = 13;
public static readonly UntypedInt TokenSessionReference = 14;
public static readonly UntypedInt TokenSandBoxInert = 15;
public static readonly UntypedInt TokenAuditPolicy = 16;
public static readonly UntypedInt TokenOrigin = 17;
public static readonly UntypedInt TokenElevationType = 18;
public static readonly UntypedInt TokenLinkedToken = 19;
public static readonly UntypedInt TokenElevation = 20;
public static readonly UntypedInt TokenHasRestrictions = 21;
public static readonly UntypedInt TokenAccessInformation = 22;
public static readonly UntypedInt TokenVirtualizationAllowed = 23;
public static readonly UntypedInt TokenVirtualizationEnabled = 24;
public static readonly UntypedInt TokenIntegrityLevel = 25;
public static readonly UntypedInt TokenUIAccess = 26;
public static readonly UntypedInt TokenMandatoryPolicy = 27;
public static readonly UntypedInt TokenLogonSid = 28;
public static readonly UntypedInt MaxTokenInfoClass = 29;

[GoType] partial struct SIDAndAttributes {
    public ж<SID> Sid;
    public uint32 Attributes;
}

[GoType] partial struct Tokenuser {
    public SIDAndAttributes User;
}

[GoType] partial struct Tokenprimarygroup {
    public ж<SID> PrimaryGroup;
}

[GoType("num:uintptr")] partial struct Token;

//sys	OpenProcessToken(h Handle, access uint32, token *Token) (err error) = advapi32.OpenProcessToken
//sys	GetTokenInformation(t Token, infoClass uint32, info *byte, infoLen uint32, returnedLen *uint32) (err error) = advapi32.GetTokenInformation
//sys	GetUserProfileDirectory(t Token, dir *uint16, dirLen *uint32) (err error) = userenv.GetUserProfileDirectoryW

// OpenCurrentProcessToken opens the access token
// associated with current process.
public static (Token, error) OpenCurrentProcessToken() {
    var (p, e) = GetCurrentProcess();
    if (e != default!) {
        return (0, e);
    }
    ref var t = ref heap(new Token(), out var Ꮡt);
    e = OpenProcessToken(p, TOKEN_QUERY, Ꮡt);
    if (e != default!) {
        return (0, e);
    }
    return (t, default!);
}

// Close releases access to access token.
public static error Close(this Token t) {
    return CloseHandle(((ΔHandle)t));
}

// getInfo retrieves a specified type of information about an access token.
internal static (@unsafe.Pointer, error) getInfo(this Token t, uint32 @class, nint initSize) {
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = ((uint32)initSize);
    while (ᐧ) {
        var b = new slice<byte>(n);
        var e = GetTokenInformation(t, @class, Ꮡ(b, 0), ((uint32)len(b)), Ꮡn);
        if (e == default!) {
            return (new @unsafe.Pointer(Ꮡ(b, 0)), default!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return (default!, e);
        }
        if (n <= ((uint32)len(b))) {
            return (default!, e);
        }
    }
}

// GetTokenUser retrieves access token t user account information.
public static (ж<Tokenuser>, error) GetTokenUser(this Token t) {
    var (i, e) = t.getInfo(TokenUser, 50);
    if (e != default!) {
        return (default!, e);
    }
    return ((ж<Tokenuser>)(uintptr)(i), default!);
}

// GetTokenPrimaryGroup retrieves access token t primary group information.
// A pointer to a SID structure representing a group that will become
// the primary group of any objects created by a process using this access token.
public static (ж<Tokenprimarygroup>, error) GetTokenPrimaryGroup(this Token t) {
    var (i, e) = t.getInfo(TokenPrimaryGroup, 50);
    if (e != default!) {
        return (default!, e);
    }
    return ((ж<Tokenprimarygroup>)(uintptr)(i), default!);
}

// GetUserProfileDirectory retrieves path to the
// root directory of the access token t user's profile.
public static (@string, error) GetUserProfileDirectory(this Token t) {
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = ((uint32)100);
    while (ᐧ) {
        var b = new slice<uint16>(n);
        var e = GetUserProfileDirectory(t, Ꮡ(b, 0), Ꮡn);
        if (e == default!) {
            return (UTF16ToString(b), default!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", e);
        }
        if (n <= ((uint32)len(b))) {
            return ("", e);
        }
    }
}

} // end syscall_package
