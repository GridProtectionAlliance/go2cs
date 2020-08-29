// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package multipart -- go2cs converted at 2020 August 29 08:32:27 UTC
// import "mime/multipart" ==> using multipart = go.mime.multipart_package
// Original source: C:\Go\src\mime\multipart\formdata.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using textproto = go.net.textproto_package;
using os = go.os_package;
using static go.builtin;
using System;

namespace go {
namespace mime
{
    public static partial class multipart_package
    {
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
        private static (ref Form, error) ReadForm(this ref Reader r, long maxMemory)
        {
            return r.readForm(maxMemory);
        }

        private static (ref Form, error) readForm(this ref Reader _r, long maxMemory) => func(_r, (ref Reader r, Defer defer, Panic _, Recover __) =>
        {
            Form form = ref new Form(make(map[string][]string),make(map[string][]*FileHeader));
            defer(() =>
            {
                if (err != null)
                {
                    form.RemoveAll();
                }
            }()); 

            // Reserve an additional 10 MB for non-file parts.
            var maxValueBytes = maxMemory + int64(10L << (int)(20L));
            while (true)
            {
                var (p, err) = r.NextPart();
                if (err == io.EOF)
                {
                    break;
                }
                if (err != null)
                {
                    return (null, err);
                }
                var name = p.FormName();
                if (name == "")
                {
                    continue;
                }
                var filename = p.FileName();

                bytes.Buffer b = default;

                var (_, hasContentTypeHeader) = p.Header["Content-Type"];
                if (!hasContentTypeHeader && filename == "")
                { 
                    // value, store as string in memory
                    var (n, err) = io.CopyN(ref b, p, maxValueBytes + 1L);
                    if (err != null && err != io.EOF)
                    {
                        return (null, err);
                    }
                    maxValueBytes -= n;
                    if (maxValueBytes < 0L)
                    {
                        return (null, ErrMessageTooLarge);
                    }
                    form.Value[name] = append(form.Value[name], b.String());
                    continue;
                } 

                // file, store in memory or on disk
                FileHeader fh = ref new FileHeader(Filename:filename,Header:p.Header,);
                (n, err) = io.CopyN(ref b, p, maxMemory + 1L);
                if (err != null && err != io.EOF)
                {
                    return (null, err);
                }
                if (n > maxMemory)
                { 
                    // too big, write to disk and flush buffer
                    var (file, err) = ioutil.TempFile("", "multipart-");
                    if (err != null)
                    {
                        return (null, err);
                    }
                    var (size, err) = io.Copy(file, io.MultiReader(ref b, p));
                    {
                        var cerr = file.Close();

                        if (err == null)
                        {
                            err = cerr;
                        }

                    }
                    if (err != null)
                    {
                        os.Remove(file.Name());
                        return (null, err);
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


            return (form, null);
        });

        // Form is a parsed multipart form.
        // Its File parts are stored either in memory or on disk,
        // and are accessible via the *FileHeader's Open method.
        // Its Value parts are stored as strings.
        // Both are keyed by field name.
        public partial struct Form
        {
            public map<@string, slice<@string>> Value;
            public map<@string, slice<ref FileHeader>> File;
        }

        // RemoveAll removes any temporary files associated with a Form.
        private static error RemoveAll(this ref Form f)
        {
            error err = default;
            foreach (var (_, fhs) in f.File)
            {
                foreach (var (_, fh) in fhs)
                {
                    if (fh.tmpfile != "")
                    {
                        var e = os.Remove(fh.tmpfile);
                        if (e != null && err == null)
                        {
                            err = error.As(e);
                        }
                    }
                }
            }
            return error.As(err);
        }

        // A FileHeader describes a file part of a multipart request.
        public partial struct FileHeader
        {
            public @string Filename;
            public textproto.MIMEHeader Header;
            public long Size;
            public slice<byte> content;
            public @string tmpfile;
        }

        // Open opens and returns the FileHeader's associated File.
        private static (File, error) Open(this ref FileHeader fh)
        {
            {
                var b = fh.content;

                if (b != null)
                {
                    var r = io.NewSectionReader(bytes.NewReader(b), 0L, int64(len(b)));
                    return (new sectionReadCloser(r), null);
                }

            }
            return os.Open(fh.tmpfile);
        }

        // File is an interface to access the file part of a multipart message.
        // Its contents may be either stored in memory or on disk.
        // If stored on disk, the File's underlying concrete type will be an *os.File.
        public partial interface File : io.Reader, io.ReaderAt, io.Seeker, io.Closer
        {
        }

        // helper types to turn a []byte into a File

        private partial struct sectionReadCloser
        {
            public ref io.SectionReader SectionReader => ref SectionReader_ptr;
        }

        private static error Close(this sectionReadCloser rc)
        {
            return error.As(null);
        }
    }
}}
