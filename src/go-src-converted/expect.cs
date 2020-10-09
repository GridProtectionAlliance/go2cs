// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package packagestest -- go2cs converted at 2020 October 09 06:02:30 UTC
// import "golang.org/x/tools/go/packages/packagestest" ==> using packagestest = go.golang.org.x.tools.go.packages.packagestest_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\packagestest\expect.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using regexp = go.regexp_package;
using strings = go.strings_package;

using expect = go.golang.org.x.tools.go.expect_package;
using packages = go.golang.org.x.tools.go.packages_package;
using span = go.golang.org.x.tools.@internal.span_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace packages
{
    public static partial class packagestest_package
    {
        private static readonly @string markMethod = (@string)"mark";
        private static readonly @string eofIdentifier = (@string)"EOF";


        // Expect invokes the supplied methods for all expectation notes found in
        // the exported source files.
        //
        // All exported go source files are parsed to collect the expectation
        // notes.
        // See the documentation for expect.Parse for how the notes are collected
        // and parsed.
        //
        // The methods are supplied as a map of name to function, and those functions
        // will be matched against the expectations by name.
        // Notes with no matching function will be skipped, and functions with no
        // matching notes will not be invoked.
        // If there are no registered markers yet, a special pass will be run first
        // which adds any markers declared with @mark(Name, pattern) or @name. These
        // call the Mark method to add the marker to the global set.
        // You can register the "mark" method to override these in your own call to
        // Expect. The bound Mark function is usable directly in your method map, so
        //    exported.Expect(map[string]interface{}{"mark": exported.Mark})
        // replicates the built in behavior.
        //
        // Method invocation
        //
        // When invoking a method the expressions in the parameter list need to be
        // converted to values to be passed to the method.
        // There are a very limited set of types the arguments are allowed to be.
        //   expect.Note : passed the Note instance being evaluated.
        //   string : can be supplied either a string literal or an identifier.
        //   int : can only be supplied an integer literal.
        //   *regexp.Regexp : can only be supplied a regular expression literal
        //   token.Pos : has a file position calculated as described below.
        //   token.Position : has a file position calculated as described below.
        //   expect.Range: has a start and end position as described below.
        //   interface{} : will be passed any value
        //
        // Position calculation
        //
        // There is some extra handling when a parameter is being coerced into a
        // token.Pos, token.Position or Range type argument.
        //
        // If the parameter is an identifier, it will be treated as the name of an
        // marker to look up (as if markers were global variables).
        //
        // If it is a string or regular expression, then it will be passed to
        // expect.MatchBefore to look up a match in the line at which it was declared.
        //
        // It is safe to call this repeatedly with different method sets, but it is
        // not safe to call it concurrently.
        private static error Expect(this ptr<Exported> _addr_e, object methods)
        {
            ref Exported e = ref _addr_e.val;

            {
                var err__prev1 = err;

                var err = e.getNotes();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = e.getMarkers();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            err = default!;
            var ms = make_map<@string, method>(len(methods));
            foreach (var (name, f) in methods)
            {
                method mi = new method(f:reflect.ValueOf(f));
                mi.converters = make_slice<converter>(mi.f.Type().NumIn());
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < len(mi.converters); i++)
                    {
                        mi.converters[i], err = e.buildConverter(mi.f.Type().In(i));
                        if (err != null)
                        {
                            return error.As(fmt.Errorf("invalid method %v: %v", name, err))!;
                        }

                    }


                    i = i__prev2;
                }
                ms[name] = mi;

            }
            foreach (var (_, n) in e.notes)
            {
                if (n.Args == null)
                { 
                    // simple identifier form, convert to a call to mark
                    n = addr(new expect.Note(Pos:n.Pos,Name:markMethod,Args:[]interface{}{n.Name,n.Name},));

                }

                var (mi, ok) = ms[n.Name];
                if (!ok)
                {
                    continue;
                }

                var @params = make_slice<reflect.Value>(len(mi.converters));
                var args = n.Args;
                {
                    long i__prev2 = i;

                    foreach (var (__i, __convert) in mi.converters)
                    {
                        i = __i;
                        convert = __convert;
                        params[i], args, err = convert(n, args);
                        if (err != null)
                        {
                            return error.As(fmt.Errorf("%v: %v", e.ExpectFileSet.Position(n.Pos), err))!;
                        }

                    }

                    i = i__prev2;
                }

                if (len(args) > 0L)
                {
                    return error.As(fmt.Errorf("%v: unwanted args got %+v extra", e.ExpectFileSet.Position(n.Pos), args))!;
                } 
                //TODO: catch the error returned from the method
                mi.f.Call(params);

            }
            return error.As(null!)!;

        }

        // Range is a type alias for span.Range for backwards compatibility, prefer
        // using span.Range directly.
        public partial struct Range // : span.Range
        {
        }

        // Mark adds a new marker to the known set.
        private static void Mark(this ptr<Exported> _addr_e, @string name, Range r)
        {
            ref Exported e = ref _addr_e.val;

            if (e.markers == null)
            {
                e.markers = make_map<@string, span.Range>();
            }

            e.markers[name] = r;

        }

        private static error getNotes(this ptr<Exported> _addr_e)
        {
            ref Exported e = ref _addr_e.val;

            if (e.notes != null)
            {
                return error.As(null!)!;
            }

            ptr<expect.Note> notes = new slice<ptr<expect.Note>>(new ptr<expect.Note>[] {  });
            slice<@string> dirs = default;
            foreach (var (_, module) in e.written)
            {
                {
                    var filename__prev2 = filename;

                    foreach (var (_, __filename) in module)
                    {
                        filename = __filename;
                        dirs = append(dirs, filepath.Dir(filename));
                    }

                    filename = filename__prev2;
                }
            }
            {
                var filename__prev1 = filename;

                foreach (var (__filename) in e.Config.Overlay)
                {
                    filename = __filename;
                    dirs = append(dirs, filepath.Dir(filename));
                }

                filename = filename__prev1;
            }

            var (pkgs, err) = packages.Load(e.Config, dirs);
            if (err != null)
            {
                return error.As(fmt.Errorf("unable to load packages for directories %s: %v", dirs, err))!;
            }

            var seen = make();
            foreach (var (_, pkg) in pkgs)
            {
                {
                    var filename__prev2 = filename;

                    foreach (var (_, __filename) in pkg.GoFiles)
                    {
                        filename = __filename;
                        var (content, err) = e.FileContents(filename);
                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        var (l, err) = expect.Parse(e.ExpectFileSet, filename, content);
                        if (err != null)
                        {
                            return error.As(fmt.Errorf("failed to extract expectations: %v", err))!;
                        }

                        foreach (var (_, note) in l)
                        {
                            var pos = e.ExpectFileSet.Position(note.Pos);
                            {
                                var (_, ok) = seen[pos];

                                if (ok)
                                {
                                    continue;
                                }

                            }

                            notes = append(notes, note);
                            seen[pos] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

                        }

                    }

                    filename = filename__prev2;
                }
            }
            {
                (_, ok) = e.written[e.primary];

                if (!ok)
                {
                    e.notes = notes;
                    return error.As(null!)!;
                } 
                // Check go.mod markers regardless of mode, we need to do this so that our marker count
                // matches the counts in the summary.txt.golden file for the test directory.

            } 
            // Check go.mod markers regardless of mode, we need to do this so that our marker count
            // matches the counts in the summary.txt.golden file for the test directory.
            {
                var (gomod, found) = e.written[e.primary]["go.mod"];

                if (found)
                { 
                    // If we are in Modules mode, then we need to check the contents of the go.mod.temp.
                    if (e.Exporter == Modules)
                    {
                        gomod += ".temp";
                    }

                    (l, err) = goModMarkers(_addr_e, gomod);
                    if (err != null)
                    {
                        return error.As(fmt.Errorf("failed to extract expectations for go.mod: %v", err))!;
                    }

                    notes = append(notes, l);

                }

            }

            e.notes = notes;
            return error.As(null!)!;

        }

        private static (slice<ptr<expect.Note>>, error) goModMarkers(ptr<Exported> _addr_e, @string gomod)
        {
            slice<ptr<expect.Note>> _p0 = default;
            error _p0 = default!;
            ref Exported e = ref _addr_e.val;

            {
                var (_, err) = os.Stat(gomod);

                if (os.IsNotExist(err))
                { 
                    // If there is no go.mod file, we want to be able to continue.
                    return (null, error.As(null!)!);

                }

            }

            var (content, err) = e.FileContents(gomod);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (e.Exporter == GOPATH)
            {
                return expect.Parse(e.ExpectFileSet, gomod, content);
            }

            gomod = strings.TrimSuffix(gomod, ".temp"); 
            // If we are in Modules mode, copy the original contents file back into go.mod
            {
                var err = ioutil.WriteFile(gomod, content, 0644L);

                if (err != null)
                {
                    return (null, error.As(null!)!);
                }

            }

            return expect.Parse(e.ExpectFileSet, gomod, content);

        }

        private static error getMarkers(this ptr<Exported> _addr_e)
        {
            ref Exported e = ref _addr_e.val;

            if (e.markers != null)
            {
                return error.As(null!)!;
            } 
            // set markers early so that we don't call getMarkers again from Expect
            e.markers = make_map<@string, span.Range>();
            return error.As(e.Expect())!;

        }

        private static var noteType = reflect.TypeOf((expect.Note.val)(null));        private static var identifierType = reflect.TypeOf(expect.Identifier(""));        private static var posType = reflect.TypeOf(token.Pos(0L));        private static var positionType = reflect.TypeOf(new token.Position());        private static var rangeType = reflect.TypeOf(new span.Range());        private static var spanType = reflect.TypeOf(new span.Span());        private static var fsetType = reflect.TypeOf((token.FileSet.val)(null));        private static var regexType = reflect.TypeOf((regexp.Regexp.val)(null));        private static var exportedType = reflect.TypeOf((Exported.val)(null));

        // converter converts from a marker's argument parsed from the comment to
        // reflect values passed to the method during Invoke.
        // It takes the args remaining, and returns the args it did not consume.
        // This allows a converter to consume 0 args for well known types, or multiple
        // args for compound types.
        public delegate  error) converter(ptr<expect.Note>,  slice<object>,  (reflect.Value,  slice<object>);

        // method is used to track information about Invoke methods that is expensive to
        // calculate so that we can work it out once rather than per marker.
        private partial struct method
        {
            public reflect.Value f; // the reflect value of the passed in method
            public slice<converter> converters; // the parameter converters for the method
        }

        // buildConverter works out what function should be used to go from an ast expressions to a reflect
        // value of the type expected by a method.
        // It is called when only the target type is know, it returns converters that are flexible across
        // all supported expression types for that target type.
        private static (converter, error) buildConverter(this ptr<Exported> _addr_e, reflect.Type pt)
        {
            converter _p0 = default;
            error _p0 = default!;
            ref Exported e = ref _addr_e.val;


            if (pt == noteType) 
                return ((n, args) =>
                {
                    return (reflect.ValueOf(n), error.As(args)!, null);
                }, error.As(null!)!);
            else if (pt == fsetType) 
                return ((n, args) =>
                {
                    return (reflect.ValueOf(e.ExpectFileSet), error.As(args)!, null);
                }, error.As(null!)!);
            else if (pt == exportedType) 
                return ((n, args) =>
                {
                    return (reflect.ValueOf(e), error.As(args)!, null);
                }, error.As(null!)!);
            else if (pt == posType) 
                return ((n, args) =>
                {
                    var (r, remains, err) = e.rangeConverter(n, args);
                    if (err != null)
                    {
                        return (new reflect.Value(), error.As(null!)!, err);
                    }

                    return (reflect.ValueOf(r.Start), error.As(remains)!, null);

                }, error.As(null!)!);
            else if (pt == positionType) 
                return ((n, args) =>
                {
                    (r, remains, err) = e.rangeConverter(n, args);
                    if (err != null)
                    {
                        return (new reflect.Value(), error.As(null!)!, err);
                    }

                    return (reflect.ValueOf(e.ExpectFileSet.Position(r.Start)), error.As(remains)!, null);

                }, error.As(null!)!);
            else if (pt == rangeType) 
                return ((n, args) =>
                {
                    (r, remains, err) = e.rangeConverter(n, args);
                    if (err != null)
                    {
                        return (new reflect.Value(), error.As(null!)!, err);
                    }

                    return (reflect.ValueOf(r), error.As(remains)!, null);

                }, error.As(null!)!);
            else if (pt == spanType) 
                return ((n, args) =>
                {
                    (r, remains, err) = e.rangeConverter(n, args);
                    if (err != null)
                    {
                        return (new reflect.Value(), error.As(null!)!, err);
                    }

                    var (spn, err) = r.Span();
                    if (err != null)
                    {
                        return (new reflect.Value(), error.As(null!)!, err);
                    }

                    return (reflect.ValueOf(spn), error.As(remains)!, null);

                }, error.As(null!)!);
            else if (pt == identifierType) 
                return ((n, args) =>
                {
                    if (len(args) < 1L)
                    {
                        return (new reflect.Value(), error.As(null!)!, fmt.Errorf("missing argument"));
                    }

                    var arg = args[0L];
                    args = args[1L..];
                    switch (arg.type())
                    {
                        case expect.Identifier arg:
                            return (reflect.ValueOf(arg), error.As(args)!, null);
                            break;
                        default:
                        {
                            var arg = arg.type();
                            return (new reflect.Value(), error.As(null!)!, fmt.Errorf("cannot convert %v to string", arg));
                            break;
                        }
                    }

                }, error.As(null!)!);
            else if (pt == regexType) 
                return ((n, args) =>
                {
                    if (len(args) < 1L)
                    {
                        return (new reflect.Value(), error.As(null!)!, fmt.Errorf("missing argument"));
                    }

                    arg = args[0L];
                    args = args[1L..];
                    {
                        ptr<regexp.Regexp> (_, ok) = arg._<ptr<regexp.Regexp>>();

                        if (!ok)
                        {
                            return (new reflect.Value(), error.As(null!)!, fmt.Errorf("cannot convert %v to *regexp.Regexp", arg));
                        }

                    }

                    return (reflect.ValueOf(arg), error.As(args)!, null);

                }, error.As(null!)!);
            else if (pt.Kind() == reflect.String) 
                return ((n, args) =>
                {
                    if (len(args) < 1L)
                    {
                        return (new reflect.Value(), error.As(null!)!, fmt.Errorf("missing argument"));
                    }

                    arg = args[0L];
                    args = args[1L..];
                    switch (arg.type())
                    {
                        case expect.Identifier arg:
                            return (reflect.ValueOf(string(arg)), error.As(args)!, null);
                            break;
                        case @string arg:
                            return (reflect.ValueOf(arg), error.As(args)!, null);
                            break;
                        default:
                        {
                            var arg = arg.type();
                            return (new reflect.Value(), error.As(null!)!, fmt.Errorf("cannot convert %v to string", arg));
                            break;
                        }
                    }

                }, error.As(null!)!);
            else if (pt.Kind() == reflect.Int64) 
                return ((n, args) =>
                {
                    if (len(args) < 1L)
                    {
                        return (new reflect.Value(), error.As(null!)!, fmt.Errorf("missing argument"));
                    }

                    arg = args[0L];
                    args = args[1L..];
                    switch (arg.type())
                    {
                        case long arg:
                            return (reflect.ValueOf(arg), error.As(args)!, null);
                            break;
                        default:
                        {
                            var arg = arg.type();
                            return (new reflect.Value(), error.As(null!)!, fmt.Errorf("cannot convert %v to int", arg));
                            break;
                        }
                    }

                }, error.As(null!)!);
            else if (pt.Kind() == reflect.Bool) 
                return ((n, args) =>
                {
                    if (len(args) < 1L)
                    {
                        return (new reflect.Value(), error.As(null!)!, fmt.Errorf("missing argument"));
                    }

                    arg = args[0L];
                    args = args[1L..];
                    bool (b, ok) = arg._<bool>();
                    if (!ok)
                    {
                        return (new reflect.Value(), error.As(null!)!, fmt.Errorf("cannot convert %v to bool", arg));
                    }

                    return (reflect.ValueOf(b), error.As(args)!, null);

                }, error.As(null!)!);
            else if (pt.Kind() == reflect.Slice) 
                return ((n, args) =>
                {
                    var (converter, err) = e.buildConverter(pt.Elem());
                    if (err != null)
                    {
                        return (new reflect.Value(), error.As(null!)!, err);
                    }

                    var result = reflect.MakeSlice(reflect.SliceOf(pt.Elem()), 0L, len(args));
                    foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in args)
                    {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
                        var (value, remains, err) = converter(n, args);
                        if (err != null)
                        {
                            return (new reflect.Value(), error.As(null!)!, err);
                        }

                        result = reflect.Append(result, value);
                        args = remains;

                    }
                    return (result, error.As(args)!, null);

                }, error.As(null!)!);
            else 
                if (pt.Kind() == reflect.Interface && pt.NumMethod() == 0L)
                {
                    return ((n, args) =>
                    {
                        if (len(args) < 1L)
                        {
                            return (new reflect.Value(), error.As(null!)!, fmt.Errorf("missing argument"));
                        }

                        return (reflect.ValueOf(args[0L]), error.As(args[1L..])!, null);

                    }, error.As(null!)!);

                }

                return (null, error.As(fmt.Errorf("param has unexpected type %v (kind %v)", pt, pt.Kind()))!);
            
        }

        private static (span.Range, slice<object>, error) rangeConverter(this ptr<Exported> _addr_e, ptr<expect.Note> _addr_n, slice<object> args)
        {
            span.Range _p0 = default;
            slice<object> _p0 = default;
            error _p0 = default!;
            ref Exported e = ref _addr_e.val;
            ref expect.Note n = ref _addr_n.val;

            if (len(args) < 1L)
            {
                return (new span.Range(), null, error.As(fmt.Errorf("missing argument"))!);
            }

            var arg = args[0L];
            args = args[1L..];
            switch (arg.type())
            {
                case expect.Identifier arg:

                    if (arg == eofIdentifier) 
                        // end of file identifier, look up the current file
                        var f = e.ExpectFileSet.File(n.Pos);
                        var eof = f.Pos(f.Size());
                        return (new span.Range(FileSet:e.ExpectFileSet,Start:eof,End:token.NoPos), args, error.As(null!)!);
                    else 
                        // look up an marker by name
                        var (mark, ok) = e.markers[string(arg)];
                        if (!ok)
                        {
                            return (new span.Range(), null, error.As(fmt.Errorf("cannot find marker %v", arg))!);
                        }

                        return (mark, args, error.As(null!)!);
                                        break;
                case @string arg:
                    var (start, end, err) = expect.MatchBefore(e.ExpectFileSet, e.FileContents, n.Pos, arg);
                    if (err != null)
                    {
                        return (new span.Range(), null, error.As(err)!);
                    }

                    if (start == token.NoPos)
                    {
                        return (new span.Range(), null, error.As(fmt.Errorf("%v: pattern %s did not match", e.ExpectFileSet.Position(n.Pos), arg))!);
                    }

                    return (new span.Range(FileSet:e.ExpectFileSet,Start:start,End:end), args, error.As(null!)!);
                    break;
                case ptr<regexp.Regexp> arg:
                    (start, end, err) = expect.MatchBefore(e.ExpectFileSet, e.FileContents, n.Pos, arg);
                    if (err != null)
                    {
                        return (new span.Range(), null, error.As(err)!);
                    }

                    if (start == token.NoPos)
                    {
                        return (new span.Range(), null, error.As(fmt.Errorf("%v: pattern %s did not match", e.ExpectFileSet.Position(n.Pos), arg))!);
                    }

                    return (new span.Range(FileSet:e.ExpectFileSet,Start:start,End:end), args, error.As(null!)!);
                    break;
                default:
                {
                    var arg = arg.type();
                    return (new span.Range(), null, error.As(fmt.Errorf("cannot convert %v to pos", arg))!);
                    break;
                }
            }

        }
    }
}}}}}}
