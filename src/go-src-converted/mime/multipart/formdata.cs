// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package multipart -- go2cs converted at 2022 March 06 22:21:09 UTC
// import "mime/multipart" ==> using multipart = go.mime.multipart_package
// Original source: C:\Program Files\Go\src\mime\multipart\formdata.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;
using math = go.math_package;
using textproto = go.net.textproto_package;
using os = go.os_package;
using System;


namespace go.mime;

public static partial class multipart_package {

    // ErrMessageTooLarge is returned by ReadForm if the message form
    // data is too large to be processed.
public static var ErrMessageTooLarge = errors.New("multipart: message too large");

// TODO(adg,bradfitz): find a way to unify the DoS-prevention strategy here
// with that of the http package's ParseForm.

// ReadForm parses an entire multipart message whose parts have
// a Content-Disposition of "form-data".
// It stores up to maxMemory bytes + 10MB (reserved for non-file parts)
// in memory. File parts which can't be stored in memory will be stored on
// disk in temporary files.
// It returns ErrMessageTooLarge if all non-file parts can't be stored in
// memory.
private static (ptr<Form>, error) ReadForm(this ptr<Reader> _addr_r, long maxMemory) {
    ptr<Form> _p0 = default!;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    return _addr_r.readForm(maxMemory)!;
}

private static (ptr<Form>, error) readForm(this ptr<Reader> _addr_r, long maxMemory) => func((defer, _, _) => {
    ptr<Form> _ = default!;
    error err = default!;
    ref Reader r = ref _addr_r.val;

    ptr<Form> form = addr(new Form(make(map[string][]string),make(map[string][]*FileHeader)));
    defer(() => {
        if (err != null) {
            form.RemoveAll();
        }
    }()); 

    // Reserve an additional 10 MB for non-file parts.
    var maxValueBytes = maxMemory + int64(10 << 20);
    if (maxValueBytes <= 0) {
        if (maxMemory < 0) {
            maxValueBytes = 0;
        }
        else
 {
            maxValueBytes = math.MaxInt64;
        }
    }
    while (true) {
        var (p, err) = r.NextPart();
        if (err == io.EOF) {
            break;
        }
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        var name = p.FormName();
        if (name == "") {
            continue;
        }
        var filename = p.FileName();

        ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);

        if (filename == "") { 
            // value, store as string in memory
            var (n, err) = io.CopyN(_addr_b, p, maxValueBytes + 1);
            if (err != null && err != io.EOF) {
                return (_addr_null!, error.As(err)!);
            }

            maxValueBytes -= n;
            if (maxValueBytes < 0) {
                return (_addr_null!, error.As(ErrMessageTooLarge)!);
            }

            form.Value[name] = append(form.Value[name], b.String());
            continue;

        }
        ptr<FileHeader> fh = addr(new FileHeader(Filename:filename,Header:p.Header,));
        (n, err) = io.CopyN(_addr_b, p, maxMemory + 1);
        if (err != null && err != io.EOF) {
            return (_addr_null!, error.As(err)!);
        }
        if (n > maxMemory) { 
            // too big, write to disk and flush buffer
            var (file, err) = os.CreateTemp("", "multipart-");
            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            var (size, err) = io.Copy(file, io.MultiReader(_addr_b, p));
            {
                var cerr = file.Close();

                if (err == null) {
                    err = cerr;
                }

            }

            if (err != null) {
                os.Remove(file.Name());
                return (_addr_null!, error.As(err)!);
            }

            fh.tmpfile = file.Name();
            fh.Size = size;

        }
        else
 {
            fh.content = b.Bytes();
            fh.Size = int64(len(fh.content));
            maxMemory -= n;
            maxValueBytes -= n;
        }
        form.File[name] = append(form.File[name], fh);

    }

    return (_addr_form!, error.As(null!)!);

});

// Form is a parsed multipart form.
// Its File parts are stored either in memory or on disk,
// and are accessible via the *FileHeader's Open method.
// Its Value parts are stored as strings.
// Both are keyed by field name.
public partial struct Form {
    public map<@string, slice<@string>> Value;
    public map<@string, slice<ptr<FileHeader>>> File;
}

// RemoveAll removes any temporary files associated with a Form.
private static error RemoveAll(this ptr<Form> _addr_f) {
    ref Form f = ref _addr_f.val;

    error err = default!;
    foreach (var (_, fhs) in f.File) {
        foreach (var (_, fh) in fhs) {
            if (fh.tmpfile != "") {
                var e = os.Remove(fh.tmpfile);
                if (e != null && err == null) {
                    err = error.As(e)!;
                }
            }
        }
    }    return error.As(err)!;

}

// A FileHeader describes a file part of a multipart request.
public partial struct FileHeader {
    public @string Filename;
    public textproto.MIMEHeader Header;
    public long Size;
    public slice<byte> content;
    public @string tmpfile;
}

// Open opens and returns the FileHeader's associated File.
private static (File, error) Open(this ptr<FileHeader> _addr_fh) {
    File _p0 = default;
    error _p0 = default!;
    ref FileHeader fh = ref _addr_fh.val;

    {
        var b = fh.content;

        if (b != null) {
            var r = io.NewSectionReader(bytes.NewReader(b), 0, int64(len(b)));
            return (new sectionReadCloser(r), error.As(null!)!);
        }
    }

    return os.Open(fh.tmpfile);

}

// File is an interface to access the file part of a multipart message.
// Its contents may be either stored in memory or on disk.
// If stored on disk, the File's underlying concrete type will be an *os.File.
public partial interface File {
}

// helper types to turn a []byte into a File

private partial struct sectionReadCloser {
    public ref ptr<io.SectionReader> SectionReader> => ref SectionReader>_ptr;
}

private static error Close(this sectionReadCloser rc) {
    return error.As(null!)!;
}

} // end multipart_package
