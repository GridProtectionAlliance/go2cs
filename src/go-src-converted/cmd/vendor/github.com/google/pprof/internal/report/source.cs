// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// package report -- go2cs converted at 2020 August 29 10:06:11 UTC
// import "cmd/vendor/github.com/google/pprof/internal/report" ==> using report = go.cmd.vendor.github.com.google.pprof.@internal.report_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\report\source.go
// This file contains routines related to the generation of annotated
// source listings.

using bufio = go.bufio_package;
using fmt = go.fmt_package;
using template = go.html.template_package;
using io = go.io_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using graph = go.github.com.google.pprof.@internal.graph_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class report_package
    {
        // printSource prints an annotated source listing, include all
        // functions with samples that match the regexp rpt.options.symbol.
        // The sources are sorted by function name and then by filename to
        // eliminate potential nondeterminism.
        private static error printSource(io.Writer w, ref Report rpt)
        {
            var o = rpt.options;
            var g = rpt.newGraph(null); 

            // Identify all the functions that match the regexp provided.
            // Group nodes for each matching function.
            graph.Nodes functions = default;
            var functionNodes = make_map<@string, graph.Nodes>();
            {
                var n__prev1 = n;

                foreach (var (_, __n) in g.Nodes)
                {
                    n = __n;
                    if (!o.Symbol.MatchString(n.Info.Name))
                    {
                        continue;
                    }
                    if (functionNodes[n.Info.Name] == null)
                    {
                        functions = append(functions, n);
                    }
                    functionNodes[n.Info.Name] = append(functionNodes[n.Info.Name], n);
                }
                n = n__prev1;
            }

            functions.Sort(graph.NameOrder);

            var sourcePath = o.SourcePath;
            if (sourcePath == "")
            {
                var (wd, err) = os.Getwd();
                if (err != null)
                {
                    return error.As(fmt.Errorf("Could not stat current dir: %v", err));
                }
                sourcePath = wd;
            }
            var reader = newSourceReader(sourcePath);

            fmt.Fprintf(w, "Total: %s\n", rpt.formatValue(rpt.total));
            {
                var fn__prev1 = fn;

                foreach (var (_, __fn) in functions)
                {
                    fn = __fn;
                    var name = fn.Info.Name; 

                    // Identify all the source files associated to this function.
                    // Group nodes for each source file.
                    graph.Nodes sourceFiles = default;
                    var fileNodes = make_map<@string, graph.Nodes>();
                    {
                        var n__prev2 = n;

                        foreach (var (_, __n) in functionNodes[name])
                        {
                            n = __n;
                            if (n.Info.File == "")
                            {
                                continue;
                            }
                            if (fileNodes[n.Info.File] == null)
                            {
                                sourceFiles = append(sourceFiles, n);
                            }
                            fileNodes[n.Info.File] = append(fileNodes[n.Info.File], n);
                        }
                        n = n__prev2;
                    }

                    if (len(sourceFiles) == 0L)
                    {
                        fmt.Fprintf(w, "No source information for %s\n", name);
                        continue;
                    }
                    sourceFiles.Sort(graph.FileOrder); 

                    // Print each file associated with this function.
                    foreach (var (_, fl) in sourceFiles)
                    {
                        var filename = fl.Info.File;
                        var fns = fileNodes[filename];
                        var (flatSum, cumSum) = fns.Sum();

                        var (fnodes, _, err) = getSourceFromFile(filename, reader, fns, 0L, 0L);
                        fmt.Fprintf(w, "ROUTINE ======================== %s in %s\n", name, filename);
                        fmt.Fprintf(w, "%10s %10s (flat, cum) %s of Total\n", rpt.formatValue(flatSum), rpt.formatValue(cumSum), percentage(cumSum, rpt.total));

                        if (err != null)
                        {
                            fmt.Fprintf(w, " Error: %v\n", err);
                            continue;
                        }
                        {
                            var fn__prev3 = fn;

                            foreach (var (_, __fn) in fnodes)
                            {
                                fn = __fn;
                                fmt.Fprintf(w, "%10s %10s %6d:%s\n", valueOrDot(fn.Flat, rpt), valueOrDot(fn.Cum, rpt), fn.Info.Lineno, fn.Info.Name);
                            }
                            fn = fn__prev3;
                        }

                    }
                }
                fn = fn__prev1;
            }

            return error.As(null);
        }

        // printWebSource prints an annotated source listing, include all
        // functions with samples that match the regexp rpt.options.symbol.
        private static error printWebSource(io.Writer w, ref Report rpt, plugin.ObjTool obj)
        {
            printHeader(w, rpt);
            {
                var err = PrintWebList(w, rpt, obj, -1L);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            printPageClosing(w);
            return error.As(null);
        }

        // PrintWebList prints annotated source listing of rpt to w.
        public static error PrintWebList(io.Writer w, ref Report rpt, plugin.ObjTool obj, long maxFiles)
        {
            var o = rpt.options;
            var g = rpt.newGraph(null); 

            // If the regexp source can be parsed as an address, also match
            // functions that land on that address.
            ref ulong address = default;
            {
                var (hex, err) = strconv.ParseUint(o.Symbol.String(), 0L, 64L);

                if (err == null)
                {
                    address = ref hex;
                }

            }

            var sourcePath = o.SourcePath;
            if (sourcePath == "")
            {
                var (wd, err) = os.Getwd();
                if (err != null)
                {
                    return error.As(fmt.Errorf("Could not stat current dir: %v", err));
                }
                sourcePath = wd;
            }
            var reader = newSourceReader(sourcePath);

            private partial struct fileFunction
            {
                public @string fileName;
                public @string functionName;
            } 

            // Extract interesting symbols from binary files in the profile and
            // classify samples per symbol.
            var symbols = symbolsFromBinaries(rpt.prof, g, o.Symbol, address, obj);
            var symNodes = nodesPerSymbol(g.Nodes, symbols); 

            // Identify sources associated to a symbol by examining
            // symbol samples. Classify samples per source file.
            var fileNodes = make_map<fileFunction, graph.Nodes>();
            if (len(symNodes) == 0L)
            {
                {
                    var n__prev1 = n;

                    foreach (var (_, __n) in g.Nodes)
                    {
                        n = __n;
                        if (n.Info.File == "" || !o.Symbol.MatchString(n.Info.Name))
                        {
                            continue;
                        }
                        fileFunction ff = new fileFunction(n.Info.File,n.Info.Name);
                        fileNodes[ff] = append(fileNodes[ff], n);
                    }
            else

                    n = n__prev1;
                }

            }            {
                {
                    var nodes__prev1 = nodes;

                    foreach (var (_, __nodes) in symNodes)
                    {
                        nodes = __nodes;
                        {
                            var n__prev2 = n;

                            foreach (var (_, __n) in nodes)
                            {
                                n = __n;
                                if (n.Info.File != "")
                                {
                                    ff = new fileFunction(n.Info.File,n.Info.Name);
                                    fileNodes[ff] = append(fileNodes[ff], n);
                                }
                            }

                            n = n__prev2;
                        }

                    }

                    nodes = nodes__prev1;
                }

            }
            if (len(fileNodes) == 0L)
            {
                return error.As(fmt.Errorf("No source information for %s", o.Symbol.String()));
            }
            var sourceFiles = make(graph.Nodes, 0L, len(fileNodes));
            {
                var nodes__prev1 = nodes;

                foreach (var (_, __nodes) in fileNodes)
                {
                    nodes = __nodes;
                    var sNode = nodes[0L].Value;
                    sNode.Flat, sNode.Cum = nodes.Sum();
                    sourceFiles = append(sourceFiles, ref sNode);
                } 

                // Limit number of files printed?

                nodes = nodes__prev1;
            }

            if (maxFiles < 0L)
            {
                sourceFiles.Sort(graph.FileOrder);
            }
            else
            {
                sourceFiles.Sort(graph.FlatNameOrder);
                if (maxFiles < len(sourceFiles))
                {
                    sourceFiles = sourceFiles[..maxFiles];
                }
            } 

            // Print each file associated with this function.
            {
                var n__prev1 = n;

                foreach (var (_, __n) in sourceFiles)
                {
                    n = __n;
                    ff = new fileFunction(n.Info.File,n.Info.Name);
                    var fns = fileNodes[ff];

                    var asm = assemblyPerSourceLine(symbols, fns, ff.fileName, obj);
                    var (start, end) = sourceCoordinates(asm);

                    var (fnodes, path, err) = getSourceFromFile(ff.fileName, reader, fns, start, end);
                    if (err != null)
                    {
                        fnodes, path = getMissingFunctionSource(ff.fileName, asm, start, end);
                    }
                    printFunctionHeader(w, ff.functionName, path, n.Flat, n.Cum, rpt);
                    foreach (var (_, fn) in fnodes)
                    {
                        printFunctionSourceLine(w, fn, asm[fn.Info.Lineno], reader, rpt);
                    }
                    printFunctionClosing(w);
                }

                n = n__prev1;
            }

            return error.As(null);
        }

        // sourceCoordinates returns the lowest and highest line numbers from
        // a set of assembly statements.
        private static (long, long) sourceCoordinates(map<long, slice<assemblyInstruction>> asm)
        {
            foreach (var (l) in asm)
            {
                if (start == 0L || l < start)
                {
                    start = l;
                }
                if (end == 0L || l > end)
                {
                    end = l;
                }
            }
            return (start, end);
        }

        // assemblyPerSourceLine disassembles the binary containing a symbol
        // and classifies the assembly instructions according to its
        // corresponding source line, annotating them with a set of samples.
        private static map<long, slice<assemblyInstruction>> assemblyPerSourceLine(slice<ref objSymbol> objSyms, graph.Nodes rs, @string src, plugin.ObjTool obj)
        {
            var assembly = make_map<long, slice<assemblyInstruction>>(); 
            // Identify symbol to use for this collection of samples.
            var o = findMatchingSymbol(objSyms, rs);
            if (o == null)
            {
                return assembly;
            } 

            // Extract assembly for matched symbol
            var (insts, err) = obj.Disasm(o.sym.File, o.sym.Start, o.sym.End);
            if (err != null)
            {
                return assembly;
            }
            var srcBase = filepath.Base(src);
            var anodes = annotateAssembly(insts, rs, o.@base);
            long lineno = 0L;
            long prevline = 0L;
            foreach (var (_, an) in anodes)
            { 
                // Do not rely solely on the line number produced by Disasm
                // since it is not what we want in the presence of inlining.
                //
                // E.g., suppose we are printing source code for F and this
                // instruction is from H where F called G called H and both
                // of those calls were inlined.  We want to use the line
                // number from F, not from H (which is what Disasm gives us).
                //
                // So find the outer-most linenumber in the source file.
                var found = false;
                {
                    var (frames, err) = o.file.SourceLine(an.address + o.@base);

                    if (err == null)
                    {
                        for (var i = len(frames) - 1L; i >= 0L; i--)
                        {
                            if (filepath.Base(frames[i].File) == srcBase)
                            {
                                for (var j = i - 1L; j >= 0L; j--)
                                {
                                    an.inlineCalls = append(an.inlineCalls, new callID(frames[j].File,frames[j].Line));
                                }

                                lineno = frames[i].Line;
                                found = true;
                                break;
                            }
                        }

                    }

                }
                if (!found && filepath.Base(an.file) == srcBase)
                {
                    lineno = an.line;
                }
                if (lineno != 0L)
                {
                    if (lineno != prevline)
                    { 
                        // This instruction starts a new block
                        // of contiguous instructions on this line.
                        an.startsBlock = true;
                    }
                    prevline = lineno;
                    assembly[lineno] = append(assembly[lineno], an);
                }
            }
            return assembly;
        }

        // findMatchingSymbol looks for the symbol that corresponds to a set
        // of samples, by comparing their addresses.
        private static ref objSymbol findMatchingSymbol(slice<ref objSymbol> objSyms, graph.Nodes ns)
        {
            foreach (var (_, n) in ns)
            {
                foreach (var (_, o) in objSyms)
                {
                    if (filepath.Base(o.sym.File) == filepath.Base(n.Info.Objfile) && o.sym.Start <= n.Info.Address - o.@base && n.Info.Address - o.@base <= o.sym.End)
                    {
                        return o;
                    }
                }
            }
            return null;
        }

        // printHeader prints the page header for a weblist report.
        private static void printHeader(io.Writer w, ref Report rpt)
        {
            fmt.Fprintln(w, "\n<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"UTF-8\">\n<title>Pprof listing</title" +
    ">");
            fmt.Fprintln(w, weblistPageCSS);
            fmt.Fprintln(w, weblistPageScript);
            fmt.Fprint(w, "</head>\n<body>\n\n");

            slice<@string> labels = default;
            foreach (var (_, l) in ProfileLabels(rpt))
            {
                labels = append(labels, template.HTMLEscapeString(l));
            }
            fmt.Fprintf(w, "<div class=\"legend\">%s<br>Total: %s</div>", strings.Join(labels, "<br>\n"), rpt.formatValue(rpt.total));
        }

        // printFunctionHeader prints a function header for a weblist report.
        private static void printFunctionHeader(io.Writer w, @string name, @string path, long flatSum, long cumSum, ref Report rpt)
        {
            fmt.Fprintf(w, "<h1>%s</h1>%s\n<pre onClick=\"pprof_toggle_asm(event)\">\n  Total:  %10s %10s (flat, " +
    "cum) %s\n", template.HTMLEscapeString(name), template.HTMLEscapeString(path), rpt.formatValue(flatSum), rpt.formatValue(cumSum), percentage(cumSum, rpt.total));
        }

        // printFunctionSourceLine prints a source line and the corresponding assembly.
        private static void printFunctionSourceLine(io.Writer w, ref graph.Node fn, slice<assemblyInstruction> assembly, ref sourceReader reader, ref Report rpt)
        {
            if (len(assembly) == 0L)
            {
                fmt.Fprintf(w, "<span class=line> %6d</span> <span class=nop>  %10s %10s %8s  %s </span>\n", fn.Info.Lineno, valueOrDot(fn.Flat, rpt), valueOrDot(fn.Cum, rpt), "", template.HTMLEscapeString(fn.Info.Name));
                return;
            }
            fmt.Fprintf(w, "<span class=line> %6d</span> <span class=deadsrc>  %10s %10s %8s  %s </span>", fn.Info.Lineno, valueOrDot(fn.Flat, rpt), valueOrDot(fn.Cum, rpt), "", template.HTMLEscapeString(fn.Info.Name));
            var srcIndent = indentation(fn.Info.Name);
            fmt.Fprint(w, "<span class=asm>");
            slice<callID> curCalls = default;
            foreach (var (i, an) in assembly)
            {
                if (an.startsBlock && i != 0L)
                { 
                    // Insert a separator between discontiguous blocks.
                    fmt.Fprintf(w, " %8s %28s\n", "", "⋮");
                }
                @string fileline = default;
                if (an.file != "")
                {
                    fileline = fmt.Sprintf("%s:%d", template.HTMLEscapeString(an.file), an.line);
                }
                var flat = an.flat;
                var cum = an.cum;
                if (an.flatDiv != 0L)
                {
                    flat = flat / an.flatDiv;
                }
                if (an.cumDiv != 0L)
                {
                    cum = cum / an.cumDiv;
                } 

                // Print inlined call context.
                foreach (var (j, c) in an.inlineCalls)
                {
                    if (j < len(curCalls) && curCalls[j] == c)
                    { 
                        // Skip if same as previous instruction.
                        continue;
                    }
                    curCalls = null;
                    var fname = trimPath(c.file);
                    var (fline, ok) = reader.line(fname, c.line);
                    if (!ok)
                    {
                        fline = "";
                    }
                    var text = strings.Repeat(" ", srcIndent + 4L + 4L * j) + strings.TrimSpace(fline);
                    fmt.Fprintf(w, " %8s %10s %10s %8s  <span class=inlinesrc>%s</span> <span class=unimportant>%s:%d</span>\n", "", "", "", "", template.HTMLEscapeString(fmt.Sprintf("%-80s", text)), template.HTMLEscapeString(filepath.Base(fname)), c.line);
                }
                curCalls = an.inlineCalls;
                text = strings.Repeat(" ", srcIndent + 4L + 4L * len(curCalls)) + an.instruction;
                fmt.Fprintf(w, " %8s %10s %10s %8x: %s <span class=unimportant>%s</span>\n", "", valueOrDot(flat, rpt), valueOrDot(cum, rpt), an.address, template.HTMLEscapeString(fmt.Sprintf("%-80s", text)), template.HTMLEscapeString(fileline));
            }
            fmt.Fprintln(w, "</span>");
        }

        // printFunctionClosing prints the end of a function in a weblist report.
        private static void printFunctionClosing(io.Writer w)
        {
            fmt.Fprintln(w, "</pre>");
        }

        // printPageClosing prints the end of the page in a weblist report.
        private static void printPageClosing(io.Writer w)
        {
            fmt.Fprintln(w, weblistPageClosing);
        }

        // getSourceFromFile collects the sources of a function from a source
        // file and annotates it with the samples in fns. Returns the sources
        // as nodes, using the info.name field to hold the source code.
        private static (graph.Nodes, @string, error) getSourceFromFile(@string file, ref sourceReader reader, graph.Nodes fns, long start, long end)
        {
            file = trimPath(file);
            var lineNodes = make_map<long, graph.Nodes>(); 

            // Collect source coordinates from profile.
            const long margin = 5L; // Lines before first/after last sample.
 // Lines before first/after last sample.
            if (start == 0L)
            {
                if (fns[0L].Info.StartLine != 0L)
                {
                    start = fns[0L].Info.StartLine;
                }
                else
                {
                    start = fns[0L].Info.Lineno - margin;
                }
            }
            else
            {
                start -= margin;
            }
            if (end == 0L)
            {
                end = fns[0L].Info.Lineno;
            }
            end += margin;
            foreach (var (_, n) in fns)
            {
                var lineno = n.Info.Lineno;
                var nodeStart = n.Info.StartLine;
                if (nodeStart == 0L)
                {
                    nodeStart = lineno - margin;
                }
                var nodeEnd = lineno + margin;
                if (nodeStart < start)
                {
                    start = nodeStart;
                }
                else if (nodeEnd > end)
                {
                    end = nodeEnd;
                }
                lineNodes[lineno] = append(lineNodes[lineno], n);
            }
            if (start < 1L)
            {
                start = 1L;
            }
            graph.Nodes src = default;
            {
                var lineno__prev1 = lineno;

                for (lineno = start; lineno <= end; lineno++)
                {
                    var (line, ok) = reader.line(file, lineno);
                    if (!ok)
                    {
                        break;
                    }
                    var (flat, cum) = lineNodes[lineno].Sum();
                    src = append(src, ref new graph.Node(Info:graph.NodeInfo{Name:strings.TrimRight(line,"\n"),Lineno:lineno,},Flat:flat,Cum:cum,));
                }


                lineno = lineno__prev1;
            }
            {
                var err = reader.fileError(file);

                if (err != null)
                {
                    return (null, file, err);
                }

            }
            return (src, file, null);
        }

        // getMissingFunctionSource creates a dummy function body to point to
        // the source file and annotates it with the samples in asm.
        private static (graph.Nodes, @string) getMissingFunctionSource(@string filename, map<long, slice<assemblyInstruction>> asm, long start, long end)
        {
            graph.Nodes fnodes = default;
            for (var i = start; i <= end; i++)
            {
                var insts = asm[i];
                if (len(insts) == 0L)
                {
                    continue;
                }
                assemblyInstruction group = default;
                foreach (var (_, insn) in insts)
                {
                    group.flat += insn.flat;
                    group.cum += insn.cum;
                    group.flatDiv += insn.flatDiv;
                    group.cumDiv += insn.cumDiv;
                }
                var flat = group.flatValue();
                var cum = group.cumValue();
                fnodes = append(fnodes, ref new graph.Node(Info:graph.NodeInfo{Name:"???",Lineno:i,},Flat:flat,Cum:cum,));
            }

            return (fnodes, filename);
        }

        // sourceReader provides access to source code with caching of file contents.
        private partial struct sourceReader
        {
            public @string searchPath; // files maps from path name to a list of lines.
// files[*][0] is unused since line numbering starts at 1.
            public map<@string, slice<@string>> files; // errors collects errors encountered per file.  These errors are
// consulted before returning out of these module.
            public map<@string, error> errors;
        }

        private static ref sourceReader newSourceReader(@string searchPath)
        {
            return ref new sourceReader(searchPath,make(map[string][]string),make(map[string]error),);
        }

        private static error fileError(this ref sourceReader reader, @string path)
        {
            return error.As(reader.errors[path]);
        }

        private static (@string, bool) line(this ref sourceReader reader, @string path, long lineno)
        {
            var (lines, ok) = reader.files[path];
            if (!ok)
            { 
                // Read and cache file contents.
                lines = new slice<@string>(new @string[] { "" }); // Skip 0th line
                var (f, err) = openSourceFile(path, reader.searchPath);
                if (err != null)
                {
                    reader.errors[path] = err;
                }
                else
                {
                    var s = bufio.NewScanner(f);
                    while (s.Scan())
                    {
                        lines = append(lines, s.Text());
                    }

                    f.Close();
                    if (s.Err() != null)
                    {
                        reader.errors[path] = err;
                    }
                }
                reader.files[path] = lines;
            }
            if (lineno <= 0L || lineno >= len(lines))
            {
                return ("", false);
            }
            return (lines[lineno], true);
        }

        // openSourceFile opens a source file from a name encoded in a
        // profile. File names in a profile after often relative paths, so
        // search them in each of the paths in searchPath (or CWD by default),
        // and their parents.
        private static (ref os.File, error) openSourceFile(@string path, @string searchPath)
        {
            if (filepath.IsAbs(path))
            {
                var (f, err) = os.Open(path);
                return (f, err);
            } 

            // Scan each component of the path
            foreach (var (_, dir) in strings.Split(searchPath, ":"))
            { 
                // Search up for every parent of each possible path.
                while (true)
                {
                    var filename = filepath.Join(dir, path);
                    {
                        var f__prev1 = f;

                        (f, err) = os.Open(filename);

                        if (err == null)
                        {
                            return (f, null);
                        }

                        f = f__prev1;

                    }
                    var parent = filepath.Dir(dir);
                    if (parent == dir)
                    {
                        break;
                    }
                    dir = parent;
                }

            }
            return (null, fmt.Errorf("Could not find file %s on path %s", path, searchPath));
        }

        // trimPath cleans up a path by removing prefixes that are commonly
        // found on profiles.
        private static @string trimPath(@string path)
        {
            @string basePaths = new slice<@string>(new @string[] { "/proc/self/cwd/./", "/proc/self/cwd/" });

            var sPath = filepath.ToSlash(path);

            foreach (var (_, base) in basePaths)
            {
                if (strings.HasPrefix(sPath, base))
                {
                    return filepath.FromSlash(sPath[len(base)..]);
                }
            }
            return path;
        }

        private static long indentation(@string line)
        {
            long column = 0L;
            foreach (var (_, c) in line)
            {
                if (c == ' ')
                {
                    column++;
                }
                else if (c == '\t')
                {
                    column++;
                    while (column % 8L != 0L)
                    {
                        column++;
                    }
                else

                }                {
                    break;
                }
            }
            return column;
        }
    }
}}}}}}}
