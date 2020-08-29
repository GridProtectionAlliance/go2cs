// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:04:48 UTC
// Original source: C:\Go\src\cmd\pack\pack.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        /*
        The archive format is:

        First, on a line by itself
            !<arch>

        Then zero or more file records. Each file record has a fixed-size one-line header
        followed by data bytes followed by an optional padding byte. The header is:

            %-16s%-12d%-6d%-6d%-8o%-10d`
            name mtime uid gid mode size

        (note the trailing backquote). The %-16s here means at most 16 *bytes* of
        the name, and if shorter, space padded on the right.
        */
        private static readonly @string usageMessage = "Usage: pack op file.a [name....]\nWhere op is one of cprtx optionally followed by " +
    "v for verbose output.\nFor compatibility with old Go build environments the op st" +
    "ring grc is\naccepted as a synonym for c.\n\nFor more information, run\n\tgo doc cmd/" +
    "pack";



        private static void usage()
        {
            fmt.Fprintln(os.Stderr, usageMessage);
            os.Exit(2L);
        }

        private static void Main()
        {
            log.SetFlags(0L);
            log.SetPrefix("pack: "); 
            // need "pack op archive" at least.
            if (len(os.Args) < 3L)
            {
                log.Print("not enough arguments");
                fmt.Fprintln(os.Stderr);
                usage();
            }
            setOp(os.Args[1L]);
            ref Archive ar = default;
            switch (op)
            {
                case 'p': 
                    ar = archive(os.Args[2L], os.O_RDONLY, os.Args[3L..]);
                    ar.scan(ar.printContents);
                    break;
                case 'r': 
                    ar = archive(os.Args[2L], os.O_RDWR, os.Args[3L..]);
                    ar.scan(ar.skipContents);
                    ar.addFiles();
                    break;
                case 'c': 
                    ar = archive(os.Args[2L], os.O_RDWR | os.O_TRUNC, os.Args[3L..]);
                    ar.addPkgdef();
                    ar.addFiles();
                    break;
                case 't': 
                    ar = archive(os.Args[2L], os.O_RDONLY, os.Args[3L..]);
                    ar.scan(ar.tableOfContents);
                    break;
                case 'x': 
                    ar = archive(os.Args[2L], os.O_RDONLY, os.Args[3L..]);
                    ar.scan(ar.extractContents);
                    break;
                default: 
                    log.Printf("invalid operation %q", os.Args[1L]);
                    fmt.Fprintln(os.Stderr);
                    usage();
                    break;
            }
            if (len(ar.files) > 0L)
            {
                log.Fatalf("file %q not in archive", ar.files[0L]);
            }
        }

        // The unusual ancestry means the arguments are not Go-standard.
        // These variables hold the decoded operation specified by the first argument.
        // op holds the operation we are doing (prtx).
        // verbose tells whether the 'v' option was specified.
        private static int op = default;        private static bool verbose = default;

        // setOp parses the operation string (first argument).
        private static void setOp(@string arg)
        { 
            // Recognize 'go tool pack grc' because that was the
            // formerly canonical way to build a new archive
            // from a set of input files. Accepting it keeps old
            // build systems working with both Go 1.2 and Go 1.3.
            if (arg == "grc")
            {
                arg = "c";
            }
            foreach (var (_, r) in arg)
            {
                switch (r)
                {
                    case 'c': 

                    case 'p': 

                    case 'r': 

                    case 't': 

                    case 'x': 
                        if (op != 0L)
                        { 
                            // At most one can be set.
                            usage();
                        }
                        op = r;
                        break;
                    case 'v': 
                        if (verbose)
                        { 
                            // Can be set only once.
                            usage();
                        }
                        verbose = true;
                        break;
                    default: 
                        usage();
                        break;
                }
            }
        }

        private static readonly @string arHeader = "!<arch>\n";
        private static readonly @string entryHeader = "%s%-12d%-6d%-6d%-8o%-10d`\n"; 
        // In entryHeader the first entry, the name, is always printed as 16 bytes right-padded.
        private static readonly long entryLen = 16L + 12L + 6L + 6L + 8L + 10L + 1L + 1L;
        private static readonly @string timeFormat = "Jan _2 15:04 2006";

        // An Archive represents an open archive file. It is always scanned sequentially
        // from start to end, without backing up.
        public partial struct Archive
        {
            public ptr<os.File> fd; // Open file descriptor.
            public slice<@string> files; // Explicit list of files to be processed.
            public long pad; // Padding bytes required at end of current archive file
            public bool matchAll; // match all files in archive
        }

        // archive opens (and if necessary creates) the named archive.
        private static ref Archive archive(@string name, long mode, slice<@string> files)
        { 
            // If the file exists, it must be an archive. If it doesn't exist, or if
            // we're doing the c command, indicated by O_TRUNC, truncate the archive.
            if (!existingArchive(name) || mode & os.O_TRUNC != 0L)
            {
                create(name);
                mode &= os.O_TRUNC;
            }
            var (fd, err) = os.OpenFile(name, mode, 0L);
            if (err != null)
            {
                log.Fatal(err);
            }
            checkHeader(fd);
            return ref new Archive(fd:fd,files:files,matchAll:len(files)==0,);
        }

        // create creates and initializes an archive that does not exist.
        private static void create(@string name)
        {
            var (fd, err) = os.Create(name);
            if (err != null)
            {
                log.Fatal(err);
            }
            _, err = fmt.Fprint(fd, arHeader);
            if (err != null)
            {
                log.Fatal(err);
            }
            fd.Close();
        }

        // existingArchive reports whether the file exists and is a valid archive.
        // If it exists but is not an archive, existingArchive will exit.
        private static bool existingArchive(@string name)
        {
            var (fd, err) = os.Open(name);
            if (err != null)
            {
                if (os.IsNotExist(err))
                {
                    return false;
                }
                log.Fatalf("cannot open file: %s", err);
            }
            checkHeader(fd);
            fd.Close();
            return true;
        }

        // checkHeader verifies the header of the file. It assumes the file
        // is positioned at 0 and leaves it positioned at the end of the header.
        private static void checkHeader(ref os.File fd)
        {
            var buf = make_slice<byte>(len(arHeader));
            var (_, err) = io.ReadFull(fd, buf);
            if (err != null || string(buf) != arHeader)
            {
                log.Fatalf("%s is not an archive: bad header", fd.Name());
            }
        }

        // An Entry is the internal representation of the per-file header information of one entry in the archive.
        public partial struct Entry
        {
            public @string name;
            public long mtime;
            public long uid;
            public long gid;
            public os.FileMode mode;
            public long size;
        }

        private static @string String(this ref Entry e)
        {
            return fmt.Sprintf("%s %6d/%-6d %12d %s %s", (e.mode & 0777L).String(), e.uid, e.gid, e.size, time.Unix(e.mtime, 0L).Format(timeFormat), e.name);
        }

        // readMetadata reads and parses the metadata for the next entry in the archive.
        private static ref Entry readMetadata(this ref Archive ar)
        {
            var buf = make_slice<byte>(entryLen);
            var (_, err) = io.ReadFull(ar.fd, buf);
            if (err == io.EOF)
            { 
                // No entries left.
                return null;
            }
            if (err != null || buf[entryLen - 2L] != '`' || buf[entryLen - 1L] != '\n')
            {
                log.Fatal("file is not an archive: bad entry");
            }
            ptr<Entry> entry = @new<Entry>();
            entry.name = strings.TrimRight(string(buf[..16L]), " ");
            if (len(entry.name) == 0L)
            {
                log.Fatal("file is not an archive: bad name");
            }
            buf = buf[16L..];
            var str = string(buf);
            Func<long, long, long, long> get = (width, @base, bitsize) =>
            {
                var (v, err) = strconv.ParseInt(strings.TrimRight(str[..width], " "), base, bitsize);
                if (err != null)
                {
                    log.Fatal("file is not an archive: bad number in entry: ", err);
                }
                str = str[width..];
                return v;
            } 
            // %-16s%-12d%-6d%-6d%-8o%-10d`
; 
            // %-16s%-12d%-6d%-6d%-8o%-10d`
            entry.mtime = get(12L, 10L, 64L);
            entry.uid = int(get(6L, 10L, 32L));
            entry.gid = int(get(6L, 10L, 32L));
            entry.mode = os.FileMode(get(8L, 8L, 32L));
            entry.size = get(10L, 10L, 64L);
            return entry;
        }

        // scan scans the archive and executes the specified action on each entry.
        // When action returns, the file offset is at the start of the next entry.
        private static void scan(this ref Archive ar, Action<ref Entry> action)
        {
            while (true)
            {
                var entry = ar.readMetadata();
                if (entry == null)
                {
                    break;
                }
                action(entry);
            }

        }

        // listEntry prints to standard output a line describing the entry.
        private static void listEntry(ref Archive ar, ref Entry entry, bool verbose)
        {
            if (verbose)
            {
                fmt.Fprintf(stdout, "%s\n", entry);
            }
            else
            {
                fmt.Fprintf(stdout, "%s\n", entry.name);
            }
        }

        // output copies the entry to the specified writer.
        private static void output(this ref Archive ar, ref Entry entry, io.Writer w)
        {
            var (n, err) = io.Copy(w, io.LimitReader(ar.fd, entry.size));
            if (err != null)
            {
                log.Fatal(err);
            }
            if (n != entry.size)
            {
                log.Fatal("short file");
            }
            if (entry.size & 1L == 1L)
            {
                var (_, err) = ar.fd.Seek(1L, io.SeekCurrent);
                if (err != null)
                {
                    log.Fatal(err);
                }
            }
        }

        // skip skips the entry without reading it.
        private static void skip(this ref Archive ar, ref Entry entry)
        {
            var size = entry.size;
            if (size & 1L == 1L)
            {
                size++;
            }
            var (_, err) = ar.fd.Seek(size, io.SeekCurrent);
            if (err != null)
            {
                log.Fatal(err);
            }
        }

        // match reports whether the entry matches the argument list.
        // If it does, it also drops the file from the to-be-processed list.
        private static bool match(this ref Archive ar, ref Entry entry)
        {
            if (ar.matchAll)
            {
                return true;
            }
            foreach (var (i, name) in ar.files)
            {
                if (entry.name == name)
                {
                    copy(ar.files[i..], ar.files[i + 1L..]);
                    ar.files = ar.files[..len(ar.files) - 1L];
                    return true;
                }
            }
            return false;
        }

        // addFiles adds files to the archive. The archive is known to be
        // sane and we are positioned at the end. No attempt is made
        // to check for existing files.
        private static void addFiles(this ref Archive ar)
        {
            if (len(ar.files) == 0L)
            {
                usage();
            }
            foreach (var (_, file) in ar.files)
            {
                if (verbose)
                {
                    fmt.Printf("%s\n", file);
                }
                var (fd, err) = os.Open(file);
                if (err != null)
                {
                    log.Fatal(err);
                }
                ar.addFile(fd);
            }
            ar.files = null;
        }

        // FileLike abstracts the few methods we need, so we can test without needing real files.
        public partial interface FileLike
        {
            error Name();
            error Stat();
            error Read(slice<byte> _p0);
            error Close();
        }

        // addFile adds a single file to the archive
        private static void addFile(this ref Archive _ar, FileLike fd) => func(_ar, (ref Archive ar, Defer defer, Panic _, Recover __) =>
        {
            defer(fd.Close()); 
            // Format the entry.
            // First, get its info.
            var (info, err) = fd.Stat();
            if (err != null)
            {
                log.Fatal(err);
            } 
            // mtime, uid, gid are all zero so repeated builds produce identical output.
            var mtime = int64(0L);
            long uid = 0L;
            long gid = 0L;
            ar.startFile(info.Name(), mtime, uid, gid, info.Mode(), info.Size());
            var (n64, err) = io.Copy(ar.fd, fd);
            if (err != null)
            {
                log.Fatal("writing file: ", err);
            }
            if (n64 != info.Size())
            {
                log.Fatalf("writing file: wrote %d bytes; file is size %d", n64, info.Size());
            }
            ar.endFile();
        });

        // startFile writes the archive entry header.
        private static void startFile(this ref Archive ar, @string name, long mtime, long uid, long gid, os.FileMode mode, long size)
        {
            var (n, err) = fmt.Fprintf(ar.fd, entryHeader, exactly16Bytes(name), mtime, uid, gid, mode, size);
            if (err != null || n != entryLen)
            {
                log.Fatal("writing entry header: ", err);
            }
            ar.pad = int(size & 1L);
        }

        // endFile writes the archive entry tail (a single byte of padding, if the file size was odd).
        private static void endFile(this ref Archive ar)
        {
            if (ar.pad != 0L)
            {
                var (_, err) = ar.fd.Write(new slice<byte>(new byte[] { 0 }));
                if (err != null)
                {
                    log.Fatal("writing archive: ", err);
                }
                ar.pad = 0L;
            }
        }

        // addPkgdef adds the __.PKGDEF file to the archive, copied
        // from the first Go object file on the file list, if any.
        // The archive is known to be empty.
        private static void addPkgdef(this ref Archive ar)
        {
            foreach (var (_, file) in ar.files)
            {
                var (pkgdef, err) = readPkgdef(file);
                if (err != null)
                {
                    continue;
                }
                if (verbose)
                {
                    fmt.Printf("__.PKGDEF # %s\n", file);
                }
                ar.startFile("__.PKGDEF", 0L, 0L, 0L, 0644L, int64(len(pkgdef)));
                _, err = ar.fd.Write(pkgdef);
                if (err != null)
                {
                    log.Fatal("writing __.PKGDEF: ", err);
                }
                ar.endFile();
                break;
            }
        }

        // readPkgdef extracts the __.PKGDEF data from a Go object file.
        private static (slice<byte>, error) readPkgdef(@string file) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(file);
            if (err != null)
            {
                return (null, err);
            }
            defer(f.Close()); 

            // Read from file, collecting header for __.PKGDEF.
            // The header is from the beginning of the file until a line
            // containing just "!". The first line must begin with "go object ".
            //
            // Note: It's possible for "\n!\n" to appear within the binary
            // package export data format. To avoid truncating the package
            // definition prematurely (issue 21703), we keep keep track of
            // how many "$$" delimiters we've seen.

            var rbuf = bufio.NewReader(f);
            bytes.Buffer wbuf = default;
            long markers = 0L;
            while (true)
            {
                var (line, err) = rbuf.ReadBytes('\n');
                if (err != null)
                {
                    return (null, err);
                }
                if (wbuf.Len() == 0L && !bytes.HasPrefix(line, (slice<byte>)"go object "))
                {
                    return (null, errors.New("not a Go object file"));
                }
                if (markers % 2L == 0L && bytes.Equal(line, (slice<byte>)"!\n"))
                {
                    break;
                }
                if (bytes.HasPrefix(line, (slice<byte>)"$$"))
                {
                    markers++;
                }
                wbuf.Write(line);
            }

            return (wbuf.Bytes(), null);
        });

        // exactly16Bytes truncates the string if necessary so it is at most 16 bytes long,
        // then pads the result with spaces to be exactly 16 bytes.
        // Fmt uses runes for its width calculation, but we need bytes in the entry header.
        private static @string exactly16Bytes(@string s)
        {
            while (len(s) > 16L)
            {
                var (_, wid) = utf8.DecodeLastRuneInString(s);
                s = s[..len(s) - wid];
            }

            const @string sixteenSpaces = "                ";

            s += sixteenSpaces[..16L - len(s)];
            return s;
        }

        // Finally, the actual commands. Each is an action.

        // can be modified for testing.
        private static io.Writer stdout = os.Stdout;

        // printContents implements the 'p' command.
        private static void printContents(this ref Archive ar, ref Entry entry)
        {
            if (ar.match(entry))
            {
                if (verbose)
                {
                    listEntry(ar, entry, false);
                }
                ar.output(entry, stdout);
            }
            else
            {
                ar.skip(entry);
            }
        }

        // skipContents implements the first part of the 'r' command.
        // It just scans the archive to make sure it's intact.
        private static void skipContents(this ref Archive ar, ref Entry entry)
        {
            ar.skip(entry);
        }

        // tableOfContents implements the 't' command.
        private static void tableOfContents(this ref Archive ar, ref Entry entry)
        {
            if (ar.match(entry))
            {
                listEntry(ar, entry, verbose);
            }
            ar.skip(entry);
        }

        // extractContents implements the 'x' command.
        private static void extractContents(this ref Archive ar, ref Entry entry)
        {
            if (ar.match(entry))
            {
                if (verbose)
                {
                    listEntry(ar, entry, false);
                }
                var (fd, err) = os.OpenFile(entry.name, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, entry.mode);
                if (err != null)
                {
                    log.Fatal(err);
                }
                ar.output(entry, fd);
                fd.Close();
            }
            else
            {
                ar.skip(entry);
            }
        }
    }
}
