// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ioutil implements some I/O utility functions.
// package ioutil -- go2cs converted at 2020 August 29 08:21:55 UTC
// import "io/ioutil" ==> using ioutil = go.io.ioutil_package
// Original source: C:\Go\src\io\ioutil\ioutil.go
using bytes = go.bytes_package;
using io = go.io_package;
using os = go.os_package;
using sort = go.sort_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace io
{
    public static partial class ioutil_package
    {
        // readAll reads from r until an error or EOF and returns the data it read
        // from the internal buffer allocated with a specified capacity.
        private static (slice<byte>, error) readAll(io.Reader r, long capacity) => func((defer, panic, _) =>
        {
            bytes.Buffer buf = default; 
            // If the buffer overflows, we will get bytes.ErrTooLarge.
            // Return that as an error. Any other panic remains.
            defer(() =>
            {
                var e = recover();
                if (e == null)
                {
                    return;
                }
                {
                    error (panicErr, ok) = e._<error>();

                    if (ok && panicErr == bytes.ErrTooLarge)
                    {
                        err = panicErr;
                    }
                    else
                    {
                        panic(e);
                    }
                }
            }());
            if (int64(int(capacity)) == capacity)
            {
                buf.Grow(int(capacity));
            }
            _, err = buf.ReadFrom(r);
            return (buf.Bytes(), err);
        });

        // ReadAll reads from r until an error or EOF and returns the data it read.
        // A successful call returns err == nil, not err == EOF. Because ReadAll is
        // defined to read from src until EOF, it does not treat an EOF from Read
        // as an error to be reported.
        public static (slice<byte>, error) ReadAll(io.Reader r)
        {
            return readAll(r, bytes.MinRead);
        }

        // ReadFile reads the file named by filename and returns the contents.
        // A successful call returns err == nil, not err == EOF. Because ReadFile
        // reads the whole file, it does not treat an EOF from Read as an error
        // to be reported.
        public static (slice<byte>, error) ReadFile(@string filename) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(filename);
            if (err != null)
            {
                return (null, err);
            }
            defer(f.Close()); 
            // It's a good but not certain bet that FileInfo will tell us exactly how much to
            // read, so let's try it but be prepared for the answer to be wrong.
            long n = bytes.MinRead;

            {
                var (fi, err) = f.Stat();

                if (err == null)
                { 
                    // As initial capacity for readAll, use Size + a little extra in case Size
                    // is zero, and to avoid another allocation after Read has filled the
                    // buffer. The readAll call will read into its allocated internal buffer
                    // cheaply. If the size was wrong, we'll either waste some space off the end
                    // or reallocate as needed, but in the overwhelmingly common case we'll get
                    // it just right.
                    {
                        var size = fi.Size() + bytes.MinRead;

                        if (size > n)
                        {
                            n = size;
                        }

                    }
                }

            }
            return readAll(f, n);
        });

        // WriteFile writes data to a file named by filename.
        // If the file does not exist, WriteFile creates it with permissions perm;
        // otherwise WriteFile truncates it before writing.
        public static error WriteFile(@string filename, slice<byte> data, os.FileMode perm)
        {
            var (f, err) = os.OpenFile(filename, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, perm);
            if (err != null)
            {
                return error.As(err);
            }
            var (n, err) = f.Write(data);
            if (err == null && n < len(data))
            {
                err = io.ErrShortWrite;
            }
            {
                var err1 = f.Close();

                if (err == null)
                {
                    err = err1;
                }

            }
            return error.As(err);
        }

        // ReadDir reads the directory named by dirname and returns
        // a list of directory entries sorted by filename.
        public static (slice<os.FileInfo>, error) ReadDir(@string dirname)
        {
            var (f, err) = os.Open(dirname);
            if (err != null)
            {
                return (null, err);
            }
            var (list, err) = f.Readdir(-1L);
            f.Close();
            if (err != null)
            {
                return (null, err);
            }
            sort.Slice(list, (i, j) => list[i].Name() < list[j].Name());
            return (list, null);
        }

        private partial struct nopCloser : io.Reader
        {
            public ref io.Reader Reader => ref Reader_val;
        }

        private static error Close(this nopCloser _p0)
        {
            return error.As(null);
        }

        // NopCloser returns a ReadCloser with a no-op Close method wrapping
        // the provided Reader r.
        public static io.ReadCloser NopCloser(io.Reader r)
        {
            return new nopCloser(r);
        }

        private partial struct devNull // : long
        {
        }

        // devNull implements ReaderFrom as an optimization so io.Copy to
        // ioutil.Discard can avoid doing unnecessary work.
        private static io.ReaderFrom _ = devNull(0L);

        private static (long, error) Write(this devNull _p0, slice<byte> p)
        {
            return (len(p), null);
        }

        private static (long, error) WriteString(this devNull _p0, @string s)
        {
            return (len(s), null);
        }

        private static sync.Pool blackHolePool = new sync.Pool(New:func()interface{}{b:=make([]byte,8192)return&b},);

        private static (long, error) ReadFrom(this devNull _p0, io.Reader r)
        {
            ref slice<byte> bufp = blackHolePool.Get()._<ref slice<byte>>();
            long readSize = 0L;
            while (true)
            {
                readSize, err = r.Read(bufp.Value);
                n += int64(readSize);
                if (err != null)
                {
                    blackHolePool.Put(bufp);
                    if (err == io.EOF)
                    {
                        return (n, null);
                    }
                    return;
                }
            }

        }

        // Discard is an io.Writer on which all Write calls succeed
        // without doing anything.
        public static io.Writer Discard = devNull(0L);
    }
}}
