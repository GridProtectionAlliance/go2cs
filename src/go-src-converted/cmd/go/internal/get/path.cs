// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package get -- go2cs converted at 2020 October 08 04:36:52 UTC
// import "cmd/go/internal/get" ==> using get = go.cmd.go.@internal.get_package
// Original source: C:\Go\src\cmd\go\internal\get\path.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class get_package
    {
        // The following functions are copied verbatim from golang.org/x/mod/module/module.go,
        // with a change to additionally reject Windows short-names,
        // and one to accept arbitrary letters (golang.org/issue/29101).
        //
        // TODO(bcmills): After the call site for this function is backported,
        // consolidate this back down to a single copy.
        //
        // NOTE: DO NOT MERGE THESE UNTIL WE DECIDE ABOUT ARBITRARY LETTERS IN MODULE MODE.

        // CheckImportPath checks that an import path is valid.
        public static error CheckImportPath(@string path)
        {
            {
                var err = checkPath(path, false);

                if (err != null)
                {
                    return error.As(fmt.Errorf("malformed import path %q: %v", path, err))!;
                }
            }

            return error.As(null!)!;

        }

        // checkPath checks that a general path is valid.
        // It returns an error describing why but not mentioning path.
        // Because these checks apply to both module paths and import paths,
        // the caller is expected to add the "malformed ___ path %q: " prefix.
        // fileName indicates whether the final element of the path is a file name
        // (as opposed to a directory name).
        private static error checkPath(@string path, bool fileName)
        {
            if (!utf8.ValidString(path))
            {
                return error.As(fmt.Errorf("invalid UTF-8"))!;
            }

            if (path == "")
            {
                return error.As(fmt.Errorf("empty string"))!;
            }

            if (path[0L] == '-')
            {
                return error.As(fmt.Errorf("leading dash"))!;
            }

            if (strings.Contains(path, "//"))
            {
                return error.As(fmt.Errorf("double slash"))!;
            }

            if (path[len(path) - 1L] == '/')
            {
                return error.As(fmt.Errorf("trailing slash"))!;
            }

            long elemStart = 0L;
            foreach (var (i, r) in path)
            {
                if (r == '/')
                {
                    {
                        var err__prev2 = err;

                        var err = checkElem(path[elemStart..i], fileName);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev2;

                    }

                    elemStart = i + 1L;

                }

            }
            {
                var err__prev1 = err;

                err = checkElem(path[elemStart..], fileName);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            return error.As(null!)!;

        }

        // checkElem checks whether an individual path element is valid.
        // fileName indicates whether the element is a file name (not a directory name).
        private static error checkElem(@string elem, bool fileName)
        {
            if (elem == "")
            {
                return error.As(fmt.Errorf("empty path element"))!;
            }

            if (strings.Count(elem, ".") == len(elem))
            {
                return error.As(fmt.Errorf("invalid path element %q", elem))!;
            }

            if (elem[0L] == '.' && !fileName)
            {
                return error.As(fmt.Errorf("leading dot in path element"))!;
            }

            if (elem[len(elem) - 1L] == '.')
            {
                return error.As(fmt.Errorf("trailing dot in path element"))!;
            }

            var charOK = pathOK;
            if (fileName)
            {
                charOK = fileNameOK;
            }

            {
                var r__prev1 = r;

                foreach (var (_, __r) in elem)
                {
                    r = __r;
                    if (!charOK(r))
                    {
                        return error.As(fmt.Errorf("invalid char %q", r))!;
                    }

                } 

                // Windows disallows a bunch of path elements, sadly.
                // See https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file

                r = r__prev1;
            }

            var @short = elem;
            {
                var i = strings.Index(short, ".");

                if (i >= 0L)
                {
                    short = short[..i];
                }

            }

            foreach (var (_, bad) in badWindowsNames)
            {
                if (strings.EqualFold(bad, short))
                {
                    return error.As(fmt.Errorf("disallowed path element %q", elem))!;
                }

            } 

            // Reject path components that look like Windows short-names.
            // Those usually end in a tilde followed by one or more ASCII digits.
            {
                var tilde = strings.LastIndexByte(short, '~');

                if (tilde >= 0L && tilde < len(short) - 1L)
                {
                    var suffix = short[tilde + 1L..];
                    var suffixIsDigits = true;
                    {
                        var r__prev1 = r;

                        foreach (var (_, __r) in suffix)
                        {
                            r = __r;
                            if (r < '0' || r > '9')
                            {
                                suffixIsDigits = false;
                                break;
                            }

                        }

                        r = r__prev1;
                    }

                    if (suffixIsDigits)
                    {
                        return error.As(fmt.Errorf("trailing tilde and digits in path element"))!;
                    }

                }

            }


            return error.As(null!)!;

        }

        // pathOK reports whether r can appear in an import path element.
        //
        // NOTE: This function DIVERGES from module mode pathOK by accepting Unicode letters.
        private static bool pathOK(int r)
        {
            if (r < utf8.RuneSelf)
            {
                return r == '+' || r == '-' || r == '.' || r == '_' || r == '~' || '0' <= r && r <= '9' || 'A' <= r && r <= 'Z' || 'a' <= r && r <= 'z';
            }

            return unicode.IsLetter(r);

        }

        // fileNameOK reports whether r can appear in a file name.
        // For now we allow all Unicode letters but otherwise limit to pathOK plus a few more punctuation characters.
        // If we expand the set of allowed characters here, we have to
        // work harder at detecting potential case-folding and normalization collisions.
        // See note about "safe encoding" below.
        private static bool fileNameOK(int r)
        {
            if (r < utf8.RuneSelf)
            { 
                // Entire set of ASCII punctuation, from which we remove characters:
                //     ! " # $ % & ' ( ) * + , - . / : ; < = > ? @ [ \ ] ^ _ ` { | } ~
                // We disallow some shell special characters: " ' * < > ? ` |
                // (Note that some of those are disallowed by the Windows file system as well.)
                // We also disallow path separators / : and \ (fileNameOK is only called on path element characters).
                // We allow spaces (U+0020) in file names.
                const @string allowed = (@string)"!#$%&()+,-.=@[]^_{}~ ";

                if ('0' <= r && r <= '9' || 'A' <= r && r <= 'Z' || 'a' <= r && r <= 'z')
                {
                    return true;
                }

                for (long i = 0L; i < len(allowed); i++)
                {
                    if (rune(allowed[i]) == r)
                    {
                        return true;
                    }

                }

                return false;

            } 
            // It may be OK to add more ASCII punctuation here, but only carefully.
            // For example Windows disallows < > \, and macOS disallows :, so we must not allow those.
            return unicode.IsLetter(r);

        }

        // badWindowsNames are the reserved file path elements on Windows.
        // See https://docs.microsoft.com/en-us/windows/desktop/fileio/naming-a-file
        private static @string badWindowsNames = new slice<@string>(new @string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" });
    }
}}}}
