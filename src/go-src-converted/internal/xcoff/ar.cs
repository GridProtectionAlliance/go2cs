// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xcoff -- go2cs converted at 2022 March 06 22:41:01 UTC
// import "internal/xcoff" ==> using xcoff = go.@internal.xcoff_package
// Original source: C:\Program Files\Go\src\internal\xcoff\ar.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using System;


namespace go.@internal;

public static partial class xcoff_package {

public static readonly nuint SAIAMAG = 0x8;
public static readonly @string AIAFMAG = "`\n";
public static readonly @string AIAMAG = "<aiaff>\n";
public static readonly @string AIAMAGBIG = "<bigaf>\n"; 

// Sizeof
public static readonly nuint FL_HSZ_BIG = 0x80;
public static readonly nuint AR_HSZ_BIG = 0x70;


private partial struct bigarFileHeader {
    public array<byte> Flmagic; // Archive magic string
    public array<byte> Flmemoff; // Member table offset
    public array<byte> Flgstoff; // 32-bits global symtab offset
    public array<byte> Flgst64off; // 64-bits global symtab offset
    public array<byte> Flfstmoff; // First member offset
    public array<byte> Fllstmoff; // Last member offset
    public array<byte> Flfreeoff; // First member on free list offset
}

private partial struct bigarMemberHeader {
    public array<byte> Arsize; // File member size
    public array<byte> Arnxtmem; // Next member pointer
    public array<byte> Arprvmem; // Previous member pointer
    public array<byte> Ardate; // File member date
    public array<byte> Aruid; // File member uid
    public array<byte> Argid; // File member gid
    public array<byte> Armode; // File member mode (octal)
    public array<byte> Arnamlen; // File member name length
// _ar_nam is removed because it's easier to get name without it.
}

// Archive represents an open AIX big archive.
public partial struct Archive {
    public ref ArchiveHeader ArchiveHeader => ref ArchiveHeader_val;
    public slice<ptr<Member>> Members;
    public io.Closer closer;
}

// MemberHeader holds information about a big archive file header
public partial struct ArchiveHeader {
    public @string magic;
}

// Member represents a member of an AIX big archive.
public partial struct Member {
    public ref MemberHeader MemberHeader => ref MemberHeader_val;
    public ptr<io.SectionReader> sr;
}

// MemberHeader holds information about a big archive member
public partial struct MemberHeader {
    public @string Name;
    public ulong Size;
}

// OpenArchive opens the named archive using os.Open and prepares it for use
// as an AIX big archive.
public static (ptr<Archive>, error) OpenArchive(@string name) {
    ptr<Archive> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (arch, err) = NewArchive(f);
    if (err != null) {
        f.Close();
        return (_addr_null!, error.As(err)!);
    }
    arch.closer = f;
    return (_addr_arch!, error.As(null!)!);

}

// Close closes the Archive.
// If the Archive was created using NewArchive directly instead of OpenArchive,
// Close has no effect.
private static error Close(this ptr<Archive> _addr_a) {
    ref Archive a = ref _addr_a.val;

    error err = default!;
    if (a.closer != null) {
        err = error.As(a.closer.Close())!;
        a.closer = null;
    }
    return error.As(err)!;

}

// NewArchive creates a new Archive for accessing an AIX big archive in an underlying reader.
public static (ptr<Archive>, error) NewArchive(io.ReaderAt r) {
    ptr<Archive> _p0 = default!;
    error _p0 = default!;

    Func<slice<byte>, (long, error)> parseDecimalBytes = b => {
        return _addr_strconv.ParseInt(strings.TrimSpace(string(b)), 10, 64)!;
    };
    var sr = io.NewSectionReader(r, 0, 1 << 63 - 1); 

    // Read File Header
    array<byte> magic = new array<byte>(SAIAMAG);
    {
        var err__prev1 = err;

        var (_, err) = sr.ReadAt(magic[..], 0);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }


    ptr<Archive> arch = @new<Archive>();

    if (string(magic[..]) == AIAMAGBIG) 
        arch.magic = string(magic[..]);
    else if (string(magic[..]) == AIAMAG) 
        return (_addr_null!, error.As(fmt.Errorf("small AIX archive not supported"))!);
    else 
        return (_addr_null!, error.As(fmt.Errorf("unrecognised archive magic: 0x%x", magic))!);
        ref bigarFileHeader fhdr = ref heap(out ptr<bigarFileHeader> _addr_fhdr);
    {
        var err__prev1 = err;

        (_, err) = sr.Seek(0, os.SEEK_SET);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        var err = binary.Read(sr, binary.BigEndian, _addr_fhdr);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }


    var (off, err) = parseDecimalBytes(fhdr.Flfstmoff[..]);
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("error parsing offset of first member in archive header(%q); %v", fhdr, err))!);
    }
    if (off == 0) { 
        // Occurs if the archive is empty.
        return (_addr_arch!, error.As(null!)!);

    }
    var (lastoff, err) = parseDecimalBytes(fhdr.Fllstmoff[..]);
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("error parsing offset of first member in archive header(%q); %v", fhdr, err))!);
    }
    while (true) { 
        // Read Member Header
        // The member header is normally 2 bytes larger. But it's easier
        // to read the name if the header is read without _ar_nam.
        // However, AIAFMAG must be read afterward.
        {
            var err__prev1 = err;

            (_, err) = sr.Seek(off, os.SEEK_SET);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev1;

        }


        ref bigarMemberHeader mhdr = ref heap(out ptr<bigarMemberHeader> _addr_mhdr);
        {
            var err__prev1 = err;

            err = binary.Read(sr, binary.BigEndian, _addr_mhdr);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev1;

        }


        ptr<Member> member = @new<Member>();
        arch.Members = append(arch.Members, member);

        var (size, err) = parseDecimalBytes(mhdr.Arsize[..]);
        if (err != null) {
            return (_addr_null!, error.As(fmt.Errorf("error parsing size in member header(%q); %v", mhdr, err))!);
        }
        member.Size = uint64(size); 

        // Read name
        var (namlen, err) = parseDecimalBytes(mhdr.Arnamlen[..]);
        if (err != null) {
            return (_addr_null!, error.As(fmt.Errorf("error parsing name length in member header(%q); %v", mhdr, err))!);
        }
        var name = make_slice<byte>(namlen);
        {
            var err__prev1 = err;

            err = binary.Read(sr, binary.BigEndian, name);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev1;

        }

        member.Name = string(name);

        var fileoff = off + AR_HSZ_BIG + namlen;
        if (fileoff & 1 != 0) {
            fileoff++;
            {
                var err__prev2 = err;

                (_, err) = sr.Seek(1, os.SEEK_CUR);

                if (err != null) {
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev2;

            }

        }
        ref array<byte> fmag = ref heap(new array<byte>(2), out ptr<array<byte>> _addr_fmag);
        {
            var err__prev1 = err;

            err = binary.Read(sr, binary.BigEndian, _addr_fmag);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev1;

        }

        if (string(fmag[..]) != AIAFMAG) {
            return (_addr_null!, error.As(fmt.Errorf("AIAFMAG not found after member header"))!);
        }
        fileoff += 2; // Add the two bytes of AIAFMAG
        member.sr = io.NewSectionReader(sr, fileoff, size);

        if (off == lastoff) {
            break;
        }
        off, err = parseDecimalBytes(mhdr.Arnxtmem[..]);
        if (err != null) {
            return (_addr_null!, error.As(fmt.Errorf("error parsing offset of first member in archive header(%q); %v", fhdr, err))!);
        }
    }

    return (_addr_arch!, error.As(null!)!);


}

// GetFile returns the XCOFF file defined by member name.
// FIXME: This doesn't work if an archive has two members with the same
// name which can occur if a archive has both 32-bits and 64-bits files.
private static (ptr<File>, error) GetFile(this ptr<Archive> _addr_arch, @string name) {
    ptr<File> _p0 = default!;
    error _p0 = default!;
    ref Archive arch = ref _addr_arch.val;

    foreach (var (_, mem) in arch.Members) {
        if (mem.Name == name) {
            return _addr_NewFile(mem.sr)!;
        }
    }    return (_addr_null!, error.As(fmt.Errorf("unknown member %s in archive", name))!);


}

} // end xcoff_package
