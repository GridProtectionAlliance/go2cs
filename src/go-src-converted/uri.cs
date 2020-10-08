// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package span -- go2cs converted at 2020 October 08 04:54:40 UTC
// import "golang.org/x/tools/internal/span" ==> using span = go.golang.org.x.tools.@internal.span_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\span\uri.go
using fmt = go.fmt_package;
using url = go.net.url_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal
{
    public static partial class span_package
    {
        private static readonly @string fileScheme = (@string)"file";

        // URI represents the full URI for a file.


        // URI represents the full URI for a file.
        public partial struct URI // : @string
        {
        }

        public static bool IsFile(this URI uri)
        {
            return strings.HasPrefix(string(uri), "file://");
        }

        // Filename returns the file path for the given URI.
        // It is an error to call this on a URI that is not a valid filename.
        public static @string Filename(this URI uri) => func((_, panic, __) =>
        {
            var (filename, err) = filename(uri);
            if (err != null)
            {
                panic(err);
            }

            return filepath.FromSlash(filename);

        });

        private static (@string, error) filename(URI uri)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (uri == "")
            {
                return ("", error.As(null!)!);
            }

            var (u, err) = url.ParseRequestURI(string(uri));
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            if (u.Scheme != fileScheme)
            {
                return ("", error.As(fmt.Errorf("only file URIs are supported, got %q from %q", u.Scheme, uri))!);
            } 
            // If the URI is a Windows URI, we trim the leading "/" and lowercase
            // the drive letter, which will never be case sensitive.
            if (isWindowsDriveURIPath(u.Path))
            {
                u.Path = strings.ToUpper(string(u.Path[1L])) + u.Path[2L..];
            }

            return (u.Path, error.As(null!)!);

        }

        public static URI URIFromURI(@string s) => func((_, panic, __) =>
        {
            if (!strings.HasPrefix(s, "file:///"))
            {
                return URI(s);
            } 

            // Even though the input is a URI, it may not be in canonical form. VS Code
            // in particular over-escapes :, @, etc. Unescape and re-encode to canonicalize.
            var (path, err) = url.PathUnescape(s[len("file://")..]);
            if (err != null)
            {
                panic(err);
            } 

            // File URIs from Windows may have lowercase drive letters.
            // Since drive letters are guaranteed to be case insensitive,
            // we change them to uppercase to remain consistent.
            // For example, file:///c:/x/y/z becomes file:///C:/x/y/z.
            if (isWindowsDriveURIPath(path))
            {
                path = path[..1L] + strings.ToUpper(string(path[1L])) + path[2L..];
            }

            url.URL u = new url.URL(Scheme:fileScheme,Path:path);
            return URI(u.String());

        });

        public static long CompareURI(URI a, URI b)
        {
            if (equalURI(a, b))
            {
                return 0L;
            }

            if (a < b)
            {
                return -1L;
            }

            return 1L;

        }

        private static bool equalURI(URI a, URI b)
        {
            if (a == b)
            {
                return true;
            } 
            // If we have the same URI basename, we may still have the same file URIs.
            if (!strings.EqualFold(path.Base(string(a)), path.Base(string(b))))
            {
                return false;
            }

            var (fa, err) = filename(a);
            if (err != null)
            {
                return false;
            }

            var (fb, err) = filename(b);
            if (err != null)
            {
                return false;
            } 
            // Stat the files to check if they are equal.
            var (infoa, err) = os.Stat(filepath.FromSlash(fa));
            if (err != null)
            {
                return false;
            }

            var (infob, err) = os.Stat(filepath.FromSlash(fb));
            if (err != null)
            {
                return false;
            }

            return os.SameFile(infoa, infob);

        }

        // URIFromPath returns a span URI for the supplied file path.
        // It will always have the file scheme.
        public static URI URIFromPath(@string path)
        {
            if (path == "")
            {
                return "";
            } 
            // Handle standard library paths that contain the literal "$GOROOT".
            // TODO(rstambler): The go/packages API should allow one to determine a user's $GOROOT.
            const @string prefix = (@string)"$GOROOT";

            if (len(path) >= len(prefix) && strings.EqualFold(prefix, path[..len(prefix)]))
            {
                var suffix = path[len(prefix)..];
                path = runtime.GOROOT() + suffix;
            }

            if (!isWindowsDrivePath(path))
            {
                {
                    var (abs, err) = filepath.Abs(path);

                    if (err == null)
                    {
                        path = abs;
                    }

                }

            } 
            // Check the file path again, in case it became absolute.
            if (isWindowsDrivePath(path))
            {
                path = "/" + strings.ToUpper(string(path[0L])) + path[1L..];
            }

            path = filepath.ToSlash(path);
            url.URL u = new url.URL(Scheme:fileScheme,Path:path,);
            return URI(u.String());

        }

        // isWindowsDrivePath returns true if the file path is of the form used by
        // Windows. We check if the path begins with a drive letter, followed by a ":".
        // For example: C:/x/y/z.
        private static bool isWindowsDrivePath(@string path)
        {
            if (len(path) < 3L)
            {
                return false;
            }

            return unicode.IsLetter(rune(path[0L])) && path[1L] == ':';

        }

        // isWindowsDriveURI returns true if the file URI is of the format used by
        // Windows URIs. The url.Parse package does not specially handle Windows paths
        // (see golang/go#6027). We check if the URI path has a drive prefix (e.g. "/C:").
        private static bool isWindowsDriveURIPath(@string uri)
        {
            if (len(uri) < 4L)
            {
                return false;
            }

            return uri[0L] == '/' && unicode.IsLetter(rune(uri[1L])) && uri[2L] == ':';

        }
    }
}}}}}
