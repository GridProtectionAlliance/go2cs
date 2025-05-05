// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bytes = bytes_package;
using errors = errors_package;
using godebug = @internal.godebug_package;
using io = io_package;
using math = math_package;
using textproto = net.textproto_package;
using os = os_package;
using strconv = strconv_package;
using @internal;
using net;

partial class multipart_package {

// ErrMessageTooLarge is returned by ReadForm if the message form
// data is too large to be processed.
public static error ErrMessageTooLarge = errors.New("multipart: message too large"u8);

// TODO(adg,bradfitz): find a way to unify the DoS-prevention strategy here
// with that of the http package's ParseForm.

// ReadForm parses an entire multipart message whose parts have
// a Content-Disposition of "form-data".
// It stores up to maxMemory bytes + 10MB (reserved for non-file parts)
// in memory. File parts which can't be stored in memory will be stored on
// disk in temporary files.
// It returns [ErrMessageTooLarge] if all non-file parts can't be stored in
// memory.
[GoRecv] public static (ж<Form>, error) ReadForm(this ref Reader r, int64 maxMemory) {
    return r.readForm(maxMemory);
}

internal static ж<godebug.Setting> multipartfiles = godebug.New("#multipartfiles"u8);   // TODO: document and remove #
internal static ж<godebug.Setting> multipartmaxparts = godebug.New("multipartmaxparts"u8);

// os.File.ReadFrom will allocate its own copy buffer if we let io.Copy use it.
[GoType("dyn")] partial struct readForm_writerOnly {
    public partial ref io_package.Writer Writer { get; }
}

[GoRecv] internal static (ж<Form> _, error err) readForm(this ref Reader r, int64 maxMemory) => func((defer, _) => {
    error err = default!;

    var form = Ꮡ(new Form(new map<@string, slice<@string>>(), new map<@string, slice<ж<FileHeader>>>()));
    ж<os.File> file = default!;
    int64 fileOff = default!;
    nint numDiskFiles = 0;
    var combineFiles = true;
    if (multipartfiles.Value() == "distinct"u8) {
        combineFiles = false;
    }
    // multipartfiles.IncNonDefault() // TODO: uncomment after documenting
    nint maxParts = 1000;
    {
        @string s = multipartmaxparts.Value(); if (s != ""u8) {
            {
                var (v, errΔ1) = strconv.Atoi(s); if (errΔ1 == default! && v >= 0) {
                    maxParts = v;
                    multipartmaxparts.IncNonDefault();
                }
            }
        }
    }
    var maxHeaders = maxMIMEHeaders();
    var fileʗ1 = file;
    var formʗ1 = form;
    defer(() => {
        if (fileʗ1 != nil) {
            {
                var cerr = fileʗ1.Close(); if (err == default!) {
                    err = cerr;
                }
            }
        }
        if (combineFiles && numDiskFiles > 1) {
            foreach (var (_, fhs) in (~formʗ1).File) {
                foreach (var (_, fh) in fhs) {
                    fh.val.tmpshared = true;
                }
            }
        }
        if (err != default!) {
            formʗ1.RemoveAll();
            if (fileʗ1 != nil) {
                os.Remove(fileʗ1.Name());
            }
        }
    });
    // maxFileMemoryBytes is the maximum bytes of file data we will store in memory.
    // Data past this limit is written to disk.
    // This limit strictly applies to content, not metadata (filenames, MIME headers, etc.),
    // since metadata is always stored in memory, not disk.
    //
    // maxMemoryBytes is the maximum bytes we will store in memory, including file content,
    // non-file part values, metadata, and map entry overhead.
    //
    // We reserve an additional 10 MB in maxMemoryBytes for non-file data.
    //
    // The relationship between these parameters, as well as the overly-large and
    // unconfigurable 10 MB added on to maxMemory, is unfortunate but difficult to change
    // within the constraints of the API as documented.
    var maxFileMemoryBytes = maxMemory;
    if (maxFileMemoryBytes == math.MaxInt64) {
        maxFileMemoryBytes--;
    }
    var maxMemoryBytes = maxMemory + ((int64)(10 << (int)(20)));
    if (maxMemoryBytes <= 0) {
        if (maxMemory < 0){
            maxMemoryBytes = 0;
        } else {
            maxMemoryBytes = math.MaxInt64;
        }
    }
    slice<byte> copyBuf = default!;
    while (ᐧ) {
        (p, errΔ2) = r.nextPart(false, maxMemoryBytes, maxHeaders);
        if (AreEqual(errΔ2, io.EOF)) {
            break;
        }
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        if (maxParts <= 0) {
            return (default!, ErrMessageTooLarge);
        }
        maxParts--;
        @string name = p.FormName();
        if (name == ""u8) {
            continue;
        }
        @string filename = p.FileName();
        // Multiple values for the same key (one map entry, longer slice) are cheaper
        // than the same number of values for different keys (many map entries), but
        // using a consistent per-value cost for overhead is simpler.
        static readonly UntypedInt mapEntryOverhead = 200;
        maxMemoryBytes -= ((int64)len(name));
        maxMemoryBytes -= mapEntryOverhead;
        if (maxMemoryBytes < 0) {
            // We can't actually take this path, since nextPart would already have
            // rejected the MIME headers for being too large. Check anyway.
            return (default!, ErrMessageTooLarge);
        }
        ref var b = ref heap(new bytes_package.Buffer(), out var Ꮡb);
        if (filename == ""u8) {
            // value, store as string in memory
            var (n, errΔ3) = io.CopyN(~Ꮡb, ~p, maxMemoryBytes + 1);
            if (errΔ3 != default! && !AreEqual(errΔ3, io.EOF)) {
                return (default!, errΔ3);
            }
            maxMemoryBytes -= n;
            if (maxMemoryBytes < 0) {
                return (default!, ErrMessageTooLarge);
            }
            (~form).Value[name] = append((~form).Value[name], b.String());
            continue;
        }
        // file, store in memory or on disk
        static readonly UntypedInt fileHeaderSize = 100;
        maxMemoryBytes -= mimeHeaderSize((~p).Header);
        maxMemoryBytes -= mapEntryOverhead;
        maxMemoryBytes -= fileHeaderSize;
        if (maxMemoryBytes < 0) {
            return (default!, ErrMessageTooLarge);
        }
        foreach (var (_, v) in (~p).Header) {
            maxHeaders -= ((int64)len(v));
        }
        var fh = Ꮡ(new FileHeader(
            Filename: filename,
            Header: (~p).Header
        ));
        var (n, errΔ2) = io.CopyN(~Ꮡb, ~p, maxFileMemoryBytes + 1);
        if (errΔ2 != default! && !AreEqual(errΔ2, io.EOF)) {
            return (default!, errΔ2);
        }
        if (n > maxFileMemoryBytes){
            if (file == nil) {
                (file, errΔ2) = os.CreateTemp(r.tempDir, "multipart-"u8);
                if (errΔ2 != default!) {
                    return (default!, errΔ2);
                }
            }
            numDiskFiles++;
            {
                var (_, errΔ4) = file.Write(b.Bytes()); if (errΔ4 != default!) {
                    return (default!, errΔ4);
                }
            }
            if (copyBuf == default!) {
                copyBuf = new slice<byte>(32 * 1024);
            }
            // same buffer size as io.Copy uses
            var (remainingSize, errΔ5) = io.CopyBuffer(new writerOnly(file), ~p, copyBuf);
            if (errΔ5 != default!) {
                return (default!, errΔ5);
            }
            fh.val.tmpfile = file.Name();
            fh.val.Size = ((int64)b.Len()) + remainingSize;
            fh.val.tmpoff = fileOff;
            fileOff += fh.val.Size;
            if (!combineFiles) {
                {
                    var errΔ6 = file.Close(); if (errΔ6 != default!) {
                        return (default!, errΔ6);
                    }
                }
                file = default!;
            }
        } else {
            fh.val.content = b.Bytes();
            fh.val.Size = ((int64)len((~fh).content));
            maxFileMemoryBytes -= n;
            maxMemoryBytes -= n;
        }
        (~form).File[name] = append((~form).File[name], fh);
    }
    return (form, default!);
});

internal static int64 /*size*/ mimeHeaderSize(textproto.MIMEHeader h) {
    int64 size = default!;

    size = 400;
    foreach (var (k, vs) in h) {
        size += ((int64)len(k));
        size += 200;
        // map entry overhead
        foreach (var (_, v) in vs) {
            size += ((int64)len(v));
        }
    }
    return size;
}

// Form is a parsed multipart form.
// Its File parts are stored either in memory or on disk,
// and are accessible via the [*FileHeader]'s Open method.
// Its Value parts are stored as strings.
// Both are keyed by field name.
[GoType] partial struct Form {
    public map<@string, slice<@string>> Value;
    public map<@string, slice<ж<FileHeader>>> File;
}

// RemoveAll removes any temporary files associated with a [Form].
[GoRecv] public static error RemoveAll(this ref Form f) {
    error err = default!;
    foreach (var (_, fhs) in f.File) {
        foreach (var (_, fh) in fhs) {
            if ((~fh).tmpfile != ""u8) {
                var e = os.Remove((~fh).tmpfile);
                if (e != default! && !errors.Is(e, os.ErrNotExist) && err == default!) {
                    err = e;
                }
            }
        }
    }
    return err;
}

// A FileHeader describes a file part of a multipart request.
[GoType] partial struct FileHeader {
    public @string Filename;
    public net.textproto_package.MIMEHeader Header;
    public int64 Size;
    internal slice<byte> content;
    internal @string tmpfile;
    internal int64 tmpoff;
    internal bool tmpshared;
}

// Open opens and returns the [FileHeader]'s associated File.
[GoRecv] public static (File, error) Open(this ref FileHeader fh) {
    {
        var b = fh.content; if (b != default!) {
            var r = io.NewSectionReader(~bytes.NewReader(b), 0, ((int64)len(b)));
            return (new sectionReadCloser(r, default!), default!);
        }
    }
    if (fh.tmpshared) {
        (f, err) = os.Open(fh.tmpfile);
        if (err != default!) {
            return (default!, err);
        }
        var r = io.NewSectionReader(~f, fh.tmpoff, fh.Size);
        return (new sectionReadCloser(r, f), default!);
    }
    return os.Open(fh.tmpfile);
}

// File is an interface to access the file part of a multipart message.
// Its contents may be either stored in memory or on disk.
// If stored on disk, the File's underlying concrete type will be an *os.File.
[GoType] partial interface File :
    io.Reader,
    io.ReaderAt,
    io.Seeker,
    io.Closer
{
}

// helper types to turn a []byte into a File
[GoType] partial struct sectionReadCloser {
    public partial ref ж<io_package.SectionReader> SectionReader { get; }
    public partial ref io_package.Closer Closer { get; }
}

internal static error Close(this sectionReadCloser rc) {
    if (rc.Closer != default!) {
        return rc.Closer.Close();
    }
    return default!;
}

} // end multipart_package
