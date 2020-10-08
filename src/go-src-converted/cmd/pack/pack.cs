// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:42:09 UTC
// Original source: C:\Go\src\cmd\pack\pack.go
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
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
        private static readonly @string usageMessage = (@string)"Usage: pack op file.a [name....]\nWhere op is one of cprtx optionally followed by " +
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
            ptr<Archive> ar;
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

        private static readonly @string arHeader = (@string)"!<arch>\n";
        private static readonly @string entryHeader = (@string)"%s%-12d%-6d%-6d%-8o%-10d`\n"; 
        // In entryHeader the first entry, the name, is always printed as 16 bytes right-padded.
        private static readonly long entryLen = (long)16L + 12L + 6L + 6L + 8L + 10L + 1L + 1L;
        private static readonly @string timeFormat = (@string)"Jan _2 15:04 2006";


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
        private static ptr<Archive> archive(@string name, long mode, slice<@string> files)
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

            checkHeader(_addr_fd);
            return addr(new Archive(fd:fd,files:files,matchAll:len(files)==0,));

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

            checkHeader(_addr_fd);
            fd.Close();
            return true;

        }

        // checkHeader verifies the header of the file. It assumes the file
        // is positioned at 0 and leaves it positioned at the end of the header.
        private static void checkHeader(ptr<os.File> _addr_fd)
        {
            ref os.File fd = ref _addr_fd.val;

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

        private static @string String(this ptr<Entry> _addr_e)
        {
            ref Entry e = ref _addr_e.val;

            return fmt.Sprintf("%s %6d/%-6d %12d %s %s", (e.mode & 0777L).String(), e.uid, e.gid, e.size, time.Unix(e.mtime, 0L).Format(timeFormat), e.name);
        }

        // readMetadata reads and parses the metadata for the next entry in the archive.
        private static ptr<Entry> readMetadata(this ptr<Archive> _addr_ar)
        {
            ref Archive ar = ref _addr_ar.val;

            var buf = make_slice<byte>(entryLen);
            var (_, err) = io.ReadFull(ar.fd, buf);
            if (err == io.EOF)
            { 
                // No entries left.
                return _addr_null!;

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
                return _addr_v!;

            } 
            // %-16s%-12d%-6d%-6d%-8o%-10d`
; 
            // %-16s%-12d%-6d%-6d%-8o%-10d`
            entry.mtime = get(12L, 10L, 64L);
            entry.uid = int(get(6L, 10L, 32L));
            entry.gid = int(get(6L, 10L, 32L));
            entry.mode = os.FileMode(get(8L, 8L, 32L));
            entry.size = get(10L, 10L, 64L);
            return _addr_entry!;

        }

        // scan scans the archive and executes the specified action on each entry.
        // When action returns, the file offset is at the start of the next entry.
        private static void scan(this ptr<Archive> _addr_ar, Action<ptr<Entry>> action)
        {
            ref Archive ar = ref _addr_ar.val;

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
        private static void listEntry(ptr<Entry> _addr_entry, bool verbose)
        {
            ref Entry entry = ref _addr_entry.val;

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
        private static void output(this ptr<Archive> _addr_ar, ptr<Entry> _addr_entry, io.Writer w)
        {
            ref Archive ar = ref _addr_ar.val;
            ref Entry entry = ref _addr_entry.val;

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
        private static void skip(this ptr<Archive> _addr_ar, ptr<Entry> _addr_entry)
        {
            ref Archive ar = ref _addr_ar.val;
            ref Entry entry = ref _addr_entry.val;

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
        private static bool match(this ptr<Archive> _addr_ar, ptr<Entry> _addr_entry)
        {
            ref Archive ar = ref _addr_ar.val;
            ref Entry entry = ref _addr_entry.val;

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
        private static void addFiles(this ptr<Archive> _addr_ar)
        {
            ref Archive ar = ref _addr_ar.val;

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

                if (!isGoCompilerObjFile(file))
                {
                    var (fd, err) = os.Open(file);
                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    ar.addFile(fd);
                    continue;

                }

                var aro = archive(file, os.O_RDONLY, null);
                aro.scan(entry =>
                {
                    if (entry.name != "_go_.o")
                    {
                        aro.skip(entry);
                        return ;
                    }

                    ar.startFile(filepath.Base(file), 0L, 0L, 0L, 0644L, entry.size);
                    aro.output(entry, ar.fd);
                    ar.endFile();

                });

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
        private static void addFile(this ptr<Archive> _addr_ar, FileLike fd) => func((defer, _, __) =>
        {
            ref Archive ar = ref _addr_ar.val;

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
        private static void startFile(this ptr<Archive> _addr_ar, @string name, long mtime, long uid, long gid, os.FileMode mode, long size)
        {
            ref Archive ar = ref _addr_ar.val;

            var (n, err) = fmt.Fprintf(ar.fd, entryHeader, exactly16Bytes(name), mtime, uid, gid, mode, size);
            if (err != null || n != entryLen)
            {
                log.Fatal("writing entry header: ", err);
            }

            ar.pad = int(size & 1L);

        }

        // endFile writes the archive entry tail (a single byte of padding, if the file size was odd).
        private static void endFile(this ptr<Archive> _addr_ar)
        {
            ref Archive ar = ref _addr_ar.val;

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
        private static void addPkgdef(this ptr<Archive> _addr_ar)
        {
            ref Archive ar = ref _addr_ar.val;

            var done = false;
            foreach (var (_, file) in ar.files)
            {
                if (!isGoCompilerObjFile(file))
                {
                    continue;
                }

                var aro = archive(file, os.O_RDONLY, null);
                aro.scan(entry =>
                {
                    if (entry.name != "__.PKGDEF")
                    {
                        aro.skip(entry);
                        return ;
                    }

                    if (verbose)
                    {
                        fmt.Printf("__.PKGDEF # %s\n", file);
                    }

                    ar.startFile("__.PKGDEF", 0L, 0L, 0L, 0644L, entry.size);
                    aro.output(entry, ar.fd);
                    ar.endFile();
                    done = true;

                });
                if (done)
                {
                    break;
                }

            }

        }

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

            const @string sixteenSpaces = (@string)"                ";

            s += sixteenSpaces[..16L - len(s)];
            return s;

        }

        // Finally, the actual commands. Each is an action.

        // can be modified for testing.
        private static io.Writer stdout = os.Stdout;

        // printContents implements the 'p' command.
        private static void printContents(this ptr<Archive> _addr_ar, ptr<Entry> _addr_entry)
        {
            ref Archive ar = ref _addr_ar.val;
            ref Entry entry = ref _addr_entry.val;

            if (ar.match(entry))
            {
                if (verbose)
                {
                    listEntry(_addr_entry, false);
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
        private static void skipContents(this ptr<Archive> _addr_ar, ptr<Entry> _addr_entry)
        {
            ref Archive ar = ref _addr_ar.val;
            ref Entry entry = ref _addr_entry.val;

            ar.skip(entry);
        }

        // tableOfContents implements the 't' command.
        private static void tableOfContents(this ptr<Archive> _addr_ar, ptr<Entry> _addr_entry)
        {
            ref Archive ar = ref _addr_ar.val;
            ref Entry entry = ref _addr_entry.val;

            if (ar.match(entry))
            {
                listEntry(_addr_entry, verbose);
            }

            ar.skip(entry);

        }

        // extractContents implements the 'x' command.
        private static void extractContents(this ptr<Archive> _addr_ar, ptr<Entry> _addr_entry)
        {
            ref Archive ar = ref _addr_ar.val;
            ref Entry entry = ref _addr_entry.val;

            if (ar.match(entry))
            {
                if (verbose)
                {
                    listEntry(_addr_entry, false);
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

        // isGoCompilerObjFile reports whether file is an object file created
        // by the Go compiler.
        private static bool isGoCompilerObjFile(@string file)
        {
            var (fd, err) = os.Open(file);
            if (err != null)
            {
                log.Fatal(err);
            } 

            // Check for "!<arch>\n" header.
            var buf = make_slice<byte>(len(arHeader));
            _, err = io.ReadFull(fd, buf);
            if (err != null)
            {
                if (err == io.EOF)
                {
                    return false;
                }

                log.Fatal(err);

            }

            if (string(buf) != arHeader)
            {
                return false;
            } 

            // Check for exactly two entries: "__.PKGDEF" and "_go_.o".
            @string match = new slice<@string>(new @string[] { "__.PKGDEF", "_go_.o" });
            buf = make_slice<byte>(entryLen);
            while (true)
            {
                var (_, err) = io.ReadFull(fd, buf);
                if (err != null)
                {
                    if (err == io.EOF)
                    { 
                        // No entries left.
                        return true;

                    }

                    log.Fatal(err);

                }

                if (buf[entryLen - 2L] != '`' || buf[entryLen - 1L] != '\n')
                {
                    return false;
                }

                var name = strings.TrimRight(string(buf[..16L]), " ");
                while (true)
                {
                    if (len(match) == 0L)
                    {
                        return false;
                    }

                    @string next = default;
                    next = match[0L];
                    match = match[1L..];
                    if (name == next)
                    {
                        break;
                    }

                }


                var (size, err) = strconv.ParseInt(strings.TrimRight(string(buf[48L..58L]), " "), 10L, 64L);
                if (err != null)
                {
                    return false;
                }

                if (size & 1L != 0L)
                {
                    size++;
                }

                _, err = fd.Seek(size, io.SeekCurrent);
                if (err != null)
                {
                    log.Fatal(err);
                }

            }


        }
    }
}
