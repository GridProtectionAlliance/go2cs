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

// package report -- go2cs converted at 2022 March 06 23:23:48 UTC
// import "cmd/vendor/github.com/google/pprof/internal/report" ==> using report = go.cmd.vendor.github.com.google.pprof.@internal.report_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\report\source.go
// This file contains routines related to the generation of annotated
// source listings.

using bufio = go.bufio_package;
using fmt = go.fmt_package;
using template = go.html.template_package;
using io = go.io_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using graph = go.github.com.google.pprof.@internal.graph_package;
using measurement = go.github.com.google.pprof.@internal.measurement_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using profile = go.github.com.google.pprof.profile_package;
using System;


namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class report_package {

    // printSource prints an annotated source listing, include all
    // functions with samples that match the regexp rpt.options.symbol.
    // The sources are sorted by function name and then by filename to
    // eliminate potential nondeterminism.
private static error printSource(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    var o = rpt.options;
    var g = rpt.newGraph(null); 

    // Identify all the functions that match the regexp provided.
    // Group nodes for each matching function.
    graph.Nodes functions = default;
    var functionNodes = make_map<@string, graph.Nodes>();
    {
        var n__prev1 = n;

        foreach (var (_, __n) in g.Nodes) {
            n = __n;
            if (!o.Symbol.MatchString(n.Info.Name)) {
                continue;
            }
            if (functionNodes[n.Info.Name] == null) {
                functions = append(functions, n);
            }
            functionNodes[n.Info.Name] = append(functionNodes[n.Info.Name], n);

        }
        n = n__prev1;
    }

    functions.Sort(graph.NameOrder);

    var sourcePath = o.SourcePath;
    if (sourcePath == "") {
        var (wd, err) = os.Getwd();
        if (err != null) {
            return error.As(fmt.Errorf("could not stat current dir: %v", err))!;
        }
        sourcePath = wd;

    }
    var reader = newSourceReader(sourcePath, o.TrimPath);

    fmt.Fprintf(w, "Total: %s\n", rpt.formatValue(rpt.total));
    {
        var fn__prev1 = fn;

        foreach (var (_, __fn) in functions) {
            fn = __fn;
            var name = fn.Info.Name; 

            // Identify all the source files associated to this function.
            // Group nodes for each source file.
            graph.Nodes sourceFiles = default;
            var fileNodes = make_map<@string, graph.Nodes>();
            {
                var n__prev2 = n;

                foreach (var (_, __n) in functionNodes[name]) {
                    n = __n;
                    if (n.Info.File == "") {
                        continue;
                    }
                    if (fileNodes[n.Info.File] == null) {
                        sourceFiles = append(sourceFiles, n);
                    }
                    fileNodes[n.Info.File] = append(fileNodes[n.Info.File], n);

                }
                n = n__prev2;
            }

            if (len(sourceFiles) == 0) {
                fmt.Fprintf(w, "No source information for %s\n", name);
                continue;
            }
            sourceFiles.Sort(graph.FileOrder); 

            // Print each file associated with this function.
            foreach (var (_, fl) in sourceFiles) {
                var filename = fl.Info.File;
                var fns = fileNodes[filename];
                var (flatSum, cumSum) = fns.Sum();

                var (fnodes, _, err) = getSourceFromFile(filename, _addr_reader, fns, 0, 0);
                fmt.Fprintf(w, "ROUTINE ======================== %s in %s\n", name, filename);
                fmt.Fprintf(w, "%10s %10s (flat, cum) %s of Total\n", rpt.formatValue(flatSum), rpt.formatValue(cumSum), measurement.Percentage(cumSum, rpt.total));

                if (err != null) {
                    fmt.Fprintf(w, " Error: %v\n", err);
                    continue;
                }
                {
                    var fn__prev3 = fn;

                    foreach (var (_, __fn) in fnodes) {
                        fn = __fn;
                        fmt.Fprintf(w, "%10s %10s %6d:%s\n", valueOrDot(fn.Flat, rpt), valueOrDot(fn.Cum, rpt), fn.Info.Lineno, fn.Info.Name);
                    }
                    fn = fn__prev3;
                }
            }
        }
        fn = fn__prev1;
    }

    return error.As(null!)!;

}

// printWebSource prints an annotated source listing, include all
// functions with samples that match the regexp rpt.options.symbol.
private static error printWebSource(io.Writer w, ptr<Report> _addr_rpt, plugin.ObjTool obj) {
    ref Report rpt = ref _addr_rpt.val;

    printHeader(w, _addr_rpt);
    {
        var err = PrintWebList(w, _addr_rpt, obj, -1);

        if (err != null) {
            return error.As(err)!;
        }
    }

    printPageClosing(w);
    return error.As(null!)!;

}

// sourcePrinter holds state needed for generating source+asm HTML listing.
private partial struct sourcePrinter {
    public ptr<sourceReader> reader;
    public ptr<synthCode> synth;
    public plugin.ObjTool objectTool;
    public map<@string, plugin.ObjFile> objects; // Opened object files
    public ptr<regexp.Regexp> sym; // May be nil
    public map<@string, ptr<sourceFile>> files; // Set of files to print.
    public map<ulong, instructionInfo> insts; // Instructions of interest (keyed by address).

// Set of function names that we are interested in (because they had
// a sample and match sym).
    public map<@string, bool> interest; // Mapping from system function names to printable names.
    public map<@string, @string> prettyNames;
}

// addrInfo holds information for an address we are interested in.
private partial struct addrInfo {
    public ptr<profile.Location> loc; // Always non-nil
    public plugin.ObjFile obj; // May be nil
}

// instructionInfo holds collected information for an instruction.
private partial struct instructionInfo {
    public ulong objAddr; // Address in object file (with base subtracted out)
    public nint length; // Instruction length in bytes
    public @string disasm; // Disassembly of instruction
    public @string file; // For top-level function in which instruction occurs
    public nint line; // For top-level function in which instruction occurs
    public long flat; // Samples to report (divisor already applied)
    public long cum; // Samples to report (divisor already applied)
}

// sourceFile contains collected information for files we will print.
private partial struct sourceFile {
    public @string fname;
    public long cum;
    public long flat;
    public map<nint, slice<sourceInst>> lines; // Instructions to show per line
    public map<nint, @string> funcName; // Function name per line
}

// sourceInst holds information for an instruction to be displayed.
private partial struct sourceInst {
    public ulong addr;
    public slice<callID> stack; // Inlined call-stack
}

// sourceFunction contains information for a contiguous range of lines per function we
// will print.
private partial struct sourceFunction {
    public @string name;
    public nint begin; // Line numbers (end is not included in the range)
    public nint end; // Line numbers (end is not included in the range)
    public long flat;
    public long cum;
}

// addressRange is a range of addresses plus the object file that contains it.
private partial struct addressRange {
    public ulong begin;
    public ulong end;
    public plugin.ObjFile obj;
    public ptr<profile.Mapping> mapping;
    public long score; // Used to order ranges for processing
}

// PrintWebList prints annotated source listing of rpt to w.
// rpt.prof should contain inlined call info.
public static error PrintWebList(io.Writer w, ptr<Report> _addr_rpt, plugin.ObjTool obj, nint maxFiles) {
    ref Report rpt = ref _addr_rpt.val;

    var sourcePath = rpt.options.SourcePath;
    if (sourcePath == "") {
        var (wd, err) = os.Getwd();
        if (err != null) {
            return error.As(fmt.Errorf("could not stat current dir: %v", err))!;
        }
        sourcePath = wd;

    }
    var sp = newSourcePrinter(_addr_rpt, obj, sourcePath);
    sp.print(w, maxFiles, rpt);
    sp.close();
    return error.As(null!)!;

}

private static ptr<sourcePrinter> newSourcePrinter(ptr<Report> _addr_rpt, plugin.ObjTool obj, @string sourcePath) {
    ref Report rpt = ref _addr_rpt.val;

    ptr<sourcePrinter> sp = addr(new sourcePrinter(reader:newSourceReader(sourcePath,rpt.options.TrimPath),synth:newSynthCode(rpt.prof.Mapping),objectTool:obj,objects:map[string]plugin.ObjFile{},sym:rpt.options.Symbol,files:map[string]*sourceFile{},insts:map[uint64]instructionInfo{},prettyNames:map[string]string{},interest:map[string]bool{},)); 

    // If the regexp source can be parsed as an address, also match
    // functions that land on that address.
    ptr<ulong> address;
    if (sp.sym != null) {
        {
            var (hex, err) = strconv.ParseUint(sp.sym.String(), 0, 64);

            if (err == null) {
                address = _addr_hex;
            }

        }

    }
    map addrs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ulong, addrInfo>{};
    map flat = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ulong, long>{};
    map cum = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ulong, long>{}; 

    // Record an interest in the function corresponding to lines[index].
    Action<ulong, ptr<profile.Location>, nint> markInterest = (addr, loc, index) => {
        var fn = loc.Line[index];
        if (fn.Function == null) {
            return ;
        }
        sp.interest[fn.Function.Name] = true;
        sp.interest[fn.Function.SystemName] = true;
        {
            var (_, ok) = addrs[addr];

            if (!ok) {
                addrs[addr] = new addrInfo(loc,sp.objectFile(loc.Mapping));
            }

        }

    }; 

    // See if sp.sym matches line.
    Func<profile.Line, bool> matches = line => {
        if (line.Function == null) {
            return _addr_false!;
        }
        return _addr_sp.sym.MatchString(line.Function.Name) || sp.sym.MatchString(line.Function.SystemName) || sp.sym.MatchString(line.Function.Filename)!;

    }; 

    // Extract sample counts and compute set of interesting functions.
    foreach (var (_, sample) in rpt.prof.Sample) {
        var value = rpt.options.SampleValue(sample.Value);
        if (rpt.options.SampleMeanDivisor != null) {
            var div = rpt.options.SampleMeanDivisor(sample.Value);
            if (div != 0) {
                value /= div;
            }
        }
        for (var i = len(sample.Location) - 1; i >= 0; i--) {
            var loc = sample.Location[i];
            {
                var line__prev3 = line;

                foreach (var (_, __line) in loc.Line) {
                    line = __line;
                    if (line.Function == null) {
                        continue;
                    }
                    sp.prettyNames[line.Function.SystemName] = line.Function.Name;
                }

                line = line__prev3;
            }

            var addr = loc.Address;
            if (addr == 0) { 
                // Some profiles are missing valid addresses.
                addr = sp.synth.address(loc);

            }

            cum[addr] += value;
            if (i == 0) {
                flat[addr] += value;
            }

            if (sp.sym == null || (address != null && addr == address.val)) { 
                // Interested in top-level entry of stack.
                if (len(loc.Line) > 0) {
                    markInterest(addr, loc, len(loc.Line) - 1);
                }

                continue;

            } 

            // Seach in inlined stack for a match.
            var matchFile = (loc.Mapping != null && sp.sym.MatchString(loc.Mapping.File));
            {
                var line__prev3 = line;

                foreach (var (__j, __line) in loc.Line) {
                    j = __j;
                    line = __line;
                    if ((j == 0 && matchFile) || matches(line)) {
                        markInterest(addr, loc, j);
                    }
                }

                line = line__prev3;
            }
        }

    }    sp.expandAddresses(rpt, addrs, flat);
    sp.initSamples(flat, cum);
    return _addr_sp!;

}

private static void close(this ptr<sourcePrinter> _addr_sp) {
    ref sourcePrinter sp = ref _addr_sp.val;

    foreach (var (_, objFile) in sp.objects) {
        if (objFile != null) {
            objFile.Close();
        }
    }
}

private static void expandAddresses(this ptr<sourcePrinter> _addr_sp, ptr<Report> _addr_rpt, map<ulong, addrInfo> addrs, map<ulong, long> flat) {
    ref sourcePrinter sp = ref _addr_sp.val;
    ref Report rpt = ref _addr_rpt.val;
 
    // We found interesting addresses (ones with non-zero samples) above.
    // Get covering address ranges and disassemble the ranges.
    var (ranges, unprocessed) = sp.splitIntoRanges(rpt.prof, addrs, flat);
    sp.handleUnprocessed(addrs, unprocessed); 

    // Trim ranges if there are too many.
    const nint maxRanges = 25;

    sort.Slice(ranges, (i, j) => {
        return ranges[i].score > ranges[j].score;
    });
    if (len(ranges) > maxRanges) {
        ranges = ranges[..(int)maxRanges];
    }
    foreach (var (_, r) in ranges) {
        var (objBegin, err) = r.obj.ObjAddr(r.begin);
        if (err != null) {
            fmt.Fprintf(os.Stderr, "Failed to compute objdump address for range start %x: %v\n", r.begin, err);
            continue;
        }
        var (objEnd, err) = r.obj.ObjAddr(r.end);
        if (err != null) {
            fmt.Fprintf(os.Stderr, "Failed to compute objdump address for range end %x: %v\n", r.end, err);
            continue;
        }
        var @base = r.begin - objBegin;
        var (insts, err) = sp.objectTool.Disasm(r.mapping.File, objBegin, objEnd, rpt.options.IntelSyntax);
        if (err != null) { 
            // TODO(sanjay): Report that the covered addresses are missing.
            continue;

        }
        slice<plugin.Frame> lastFrames = default;
        ulong lastAddr = default;        ulong maxAddr = default;

        foreach (var (i, inst) in insts) {
            var addr = inst.Addr + base; 

            // Guard against duplicate output from Disasm.
            if (addr <= maxAddr) {
                continue;
            }

            maxAddr = addr;

            nint length = 1;
            if (i + 1 < len(insts) && insts[i + 1].Addr > inst.Addr) { 
                // Extend to next instruction.
                length = int(insts[i + 1].Addr - inst.Addr);

            } 

            // Get inlined-call-stack for address.
            var (frames, err) = r.obj.SourceLine(addr);
            if (err != null) { 
                // Construct a frame from disassembler output.
                frames = new slice<plugin.Frame>(new plugin.Frame[] { {Func:inst.Function,File:inst.File,Line:inst.Line} });

            }

            instructionInfo x = new instructionInfo(objAddr:inst.Addr,length:length,disasm:inst.Text);
            if (len(frames) > 0) { 
                // We could consider using the outer-most caller's source
                // location so we give the some hint as to where the
                // inlining happened that led to this instruction. So for
                // example, suppose we have the following (inlined) call
                // chains for this instruction:
                //   F1->G->H
                //   F2->G->H
                // We could tag the instructions from the first call with
                // F1 and instructions from the second call with F2. But
                // that leads to a somewhat confusing display. So for now,
                // we stick with just the inner-most location (i.e., H).
                // In the future we will consider changing the display to
                // make caller info more visible.
                nint index = 0; // Inner-most frame
                x.file = frames[index].File;
                x.line = frames[index].Line;

            }

            sp.insts[addr] = x; 

            // We sometimes get instructions with a zero reported line number.
            // Make such instructions have the same line info as the preceding
            // instruction, if an earlier instruction is found close enough.
            const nint neighborhood = 32;

            if (len(frames) > 0 && frames[0].Line != 0) {
                lastFrames = frames;
                lastAddr = addr;
            }
            else if ((addr - lastAddr <= neighborhood) && lastFrames != null) {
                frames = lastFrames;
            }

            sp.addStack(addr, frames);

        }
    }
}

private static void addStack(this ptr<sourcePrinter> _addr_sp, ulong addr, slice<plugin.Frame> frames) {
    ref sourcePrinter sp = ref _addr_sp.val;
 
    // See if the stack contains a function we are interested in.
    foreach (var (i, f) in frames) {
        if (!sp.interest[f.Func]) {
            continue;
        }
        var fname = canonicalizeFileName(f.File);
        var file = sp.files[fname];
        if (file == null) {
            file = addr(new sourceFile(fname:fname,lines:map[int][]sourceInst{},funcName:map[int]string{},));
            sp.files[fname] = file;
        }
        var callees = frames[..(int)i];
        var stack = make_slice<callID>(0, len(callees));
        for (var j = len(callees) - 1; j >= 0; j--) { // Reverse so caller is first
            stack = append(stack, new callID(file:callees[j].File,line:callees[j].Line,));

        }
        file.lines[f.Line] = append(file.lines[f.Line], new sourceInst(addr,stack)); 

        // Remember the first function name encountered per source line
        // and assume that that line belongs to that function.
        {
            var (_, ok) = file.funcName[f.Line];

            if (!ok) {
                file.funcName[f.Line] = f.Func;
            }

        }

    }
}

// synthAsm is the special disassembler value used for instructions without an object file.
private static readonly @string synthAsm = "";

// handleUnprocessed handles addresses that were skipped by splitIntoRanges because they
// did not belong to a known object file.


// handleUnprocessed handles addresses that were skipped by splitIntoRanges because they
// did not belong to a known object file.
private static void handleUnprocessed(this ptr<sourcePrinter> _addr_sp, map<ulong, addrInfo> addrs, slice<ulong> unprocessed) {
    ref sourcePrinter sp = ref _addr_sp.val;
 
    // makeFrames synthesizes a []plugin.Frame list for the specified address.
    // The result will typically have length 1, but may be longer if address corresponds
    // to inlined calls.
    Func<ulong, slice<plugin.Frame>> makeFrames = addr => {
        var loc = addrs[addr].loc;
        var stack = make_slice<plugin.Frame>(0, len(loc.Line));
        foreach (var (_, line) in loc.Line) {
            var fn = line.Function;
            if (fn == null) {
                continue;
            }
            stack = append(stack, new plugin.Frame(Func:fn.Name,File:fn.Filename,Line:int(line.Line),));
        }        return stack;
    };

    foreach (var (_, addr) in unprocessed) {
        var frames = makeFrames(addr);
        instructionInfo x = new instructionInfo(objAddr:addr,length:1,disasm:synthAsm,);
        if (len(frames) > 0) {
            x.file = frames[0].File;
            x.line = frames[0].Line;
        }
        sp.insts[addr] = x;

        sp.addStack(addr, frames);

    }
}

// splitIntoRanges converts the set of addresses we are interested in into a set of address
// ranges to disassemble. It also returns the set of addresses found that did not have an
// associated object file and were therefore not added to an address range.
private static (slice<addressRange>, slice<ulong>) splitIntoRanges(this ptr<sourcePrinter> _addr_sp, ptr<profile.Profile> _addr_prof, map<ulong, addrInfo> addrMap, map<ulong, long> flat) {
    slice<addressRange> _p0 = default;
    slice<ulong> _p0 = default;
    ref sourcePrinter sp = ref _addr_sp.val;
    ref profile.Profile prof = ref _addr_prof.val;
 
    // Partition addresses into two sets: ones with a known object file, and ones without.
    slice<ulong> addrs = default;    slice<ulong> unprocessed = default;

    {
        var info__prev1 = info;

        foreach (var (__addr, __info) in addrMap) {
            addr = __addr;
            info = __info;
            if (info.obj != null) {
                addrs = append(addrs, addr);
            }
            else
 {
                unprocessed = append(unprocessed, addr);
            }

        }
        info = info__prev1;
    }

    sort.Slice(addrs, (i, j) => addrs[i] < addrs[j]);

    const nint expand = 500; // How much to expand range to pick up nearby addresses.
 // How much to expand range to pick up nearby addresses.
    slice<addressRange> result = default;
    {
        nint i = 0;
        var n = len(addrs);

        while (i < n) {
            var begin = addrs[i];
            var end = addrs[i];
            var sum = flat[begin];
            i++;

            var info = addrMap[begin];
            var m = info.loc.Mapping;
            var obj = info.obj; // Non-nil because of the partitioning done above.

            // Find following addresses that are close enough to addrs[i].
            while (i < n && addrs[i] <= end + 2 * expand && addrs[i] < m.Limit) { 
                // When we expand ranges by "expand" on either side, the ranges
                // for addrs[i] and addrs[i-1] will merge.
                end = addrs[i];
                sum += flat[end];
                i++;

            }

            if (m.Start - begin >= expand) {
                begin -= expand;
            }
            else
 {
                begin = m.Start;
            }

            if (m.Limit - end >= expand) {
                end += expand;
            }
            else
 {
                end = m.Limit;
            }

            result = append(result, new addressRange(begin,end,obj,m,sum));

        }
    }
    return (result, unprocessed);

}

private static void initSamples(this ptr<sourcePrinter> _addr_sp, map<ulong, long> flat, map<ulong, long> cum) {
    ref sourcePrinter sp = ref _addr_sp.val;

    foreach (var (addr, inst) in sp.insts) { 
        // Move all samples that were assigned to the middle of an instruction to the
        // beginning of that instruction. This takes care of samples that were recorded
        // against pc+1.
        var instEnd = addr + uint64(inst.length);
        for (var p = addr; p < instEnd; p++) {
            inst.flat += flat[p];
            inst.cum += cum[p];
        }
        sp.insts[addr] = inst;

    }
}

private static void print(this ptr<sourcePrinter> _addr_sp, io.Writer w, nint maxFiles, ptr<Report> _addr_rpt) {
    ref sourcePrinter sp = ref _addr_sp.val;
    ref Report rpt = ref _addr_rpt.val;
 
    // Finalize per-file counts.
    foreach (var (_, file) in sp.files) {
        map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ulong, bool>{};
        foreach (var (_, line) in file.lines) {
            foreach (var (_, x) in line) {
                if (seen[x.addr]) { 
                    // Same address can be displayed multiple times in a file
                    // (e.g., if we show multiple inlined functions).
                    // Avoid double-counting samples in this case.
                    continue;

                }

                seen[x.addr] = true;
                var inst = sp.insts[x.addr];
                file.cum += inst.cum;
                file.flat += inst.flat;

            }

        }
    }    slice<ptr<sourceFile>> files = default;
    {
        var f__prev1 = f;

        foreach (var (_, __f) in sp.files) {
            f = __f;
            files = append(files, f);
        }
        f = f__prev1;
    }

    Func<nint, nint, bool> order = (i, j) => files[i].flat > files[j].flat;
    if (maxFiles < 0) { 
        // Order by name for compatibility with old code.
        order = (i, j) => files[i].fname < files[j].fname;
        maxFiles = len(files);

    }
    sort.Slice(files, order);
    {
        var f__prev1 = f;

        foreach (var (__i, __f) in files) {
            i = __i;
            f = __f;
            if (i < maxFiles) {
                sp.printFile(w, f, rpt);
            }
        }
        f = f__prev1;
    }
}

private static void printFile(this ptr<sourcePrinter> _addr_sp, io.Writer w, ptr<sourceFile> _addr_f, ptr<Report> _addr_rpt) {
    ref sourcePrinter sp = ref _addr_sp.val;
    ref sourceFile f = ref _addr_f.val;
    ref Report rpt = ref _addr_rpt.val;

    foreach (var (_, fn) in sp.functions(f)) {
        if (fn.cum == 0) {
            continue;
        }
        printFunctionHeader(w, fn.name, f.fname, fn.flat, fn.cum, _addr_rpt);
        slice<assemblyInstruction> asm = default;
        for (var l = fn.begin; l < fn.end; l++) {
            var (lineContents, ok) = sp.reader.line(f.fname, l);
            if (!ok) {
                if (len(f.lines[l]) == 0) { 
                    // Outside of range of valid lines and nothing to print.
                    continue;

                }

                if (l == 0) { 
                    // Line number 0 shows up if line number is not known.
                    lineContents = "<instructions with unknown line numbers>";

                }
                else
 { 
                    // Past end of file, but have data to print.
                    lineContents = "???";

                }

            } 

            // Make list of assembly instructions.
            asm = asm[..(int)0];
            long flatSum = default;            long cumSum = default;

            ulong lastAddr = default;
            foreach (var (_, inst) in f.lines[l]) {
                var addr = inst.addr;
                var x = sp.insts[addr];
                flatSum += x.flat;
                cumSum += x.cum;
                var startsBlock = (addr != lastAddr + uint64(sp.insts[lastAddr].length));
                lastAddr = addr; 

                // divisors already applied, so leave flatDiv,cumDiv as 0
                asm = append(asm, new assemblyInstruction(address:x.objAddr,instruction:x.disasm,function:fn.name,file:x.file,line:x.line,flat:x.flat,cum:x.cum,startsBlock:startsBlock,inlineCalls:inst.stack,));

            }
            printFunctionSourceLine(w, l, flatSum, cumSum, lineContents, asm, _addr_sp.reader, _addr_rpt);

        }
        printFunctionClosing(w);

    }
}

// functions splits apart the lines to show in a file into a list of per-function ranges.
private static slice<sourceFunction> functions(this ptr<sourcePrinter> _addr_sp, ptr<sourceFile> _addr_f) {
    ref sourcePrinter sp = ref _addr_sp.val;
    ref sourceFile f = ref _addr_f.val;

    slice<sourceFunction> funcs = default; 

    // Get interesting lines in sorted order.
    var lines = make_slice<nint>(0, len(f.lines));
    {
        var l__prev1 = l;

        foreach (var (__l) in f.lines) {
            l = __l;
            lines = append(lines, l);
        }
        l = l__prev1;
    }

    sort.Ints(lines); 

    // Merge adjacent lines that are in same function and not too far apart.
    const nint mergeLimit = 20;

    {
        var l__prev1 = l;

        foreach (var (_, __l) in lines) {
            l = __l;
            var name = f.funcName[l];
            {
                var (pretty, ok) = sp.prettyNames[name];

                if (ok) { 
                    // Use demangled name if available.
                    name = pretty;

                }

            }


            sourceFunction fn = new sourceFunction(name:name,begin:l,end:l+1);
            foreach (var (_, x) in f.lines[l]) {
                var inst = sp.insts[x.addr];
                fn.flat += inst.flat;
                fn.cum += inst.cum;
            } 

            // See if we should merge into preceding function.
            if (len(funcs) > 0) {
                var last = funcs[len(funcs) - 1];
                if (l - last.end < mergeLimit && last.name == name) {
                    last.end = l + 1;
                    last.flat += fn.flat;
                    last.cum += fn.cum;
                    funcs[len(funcs) - 1] = last;
                    continue;
                }
            } 

            // Add new function.
            funcs = append(funcs, fn);

        }
        l = l__prev1;
    }

    const nint expand = 5;

    foreach (var (i, f) in funcs) {
        if (i == 0) { 
            // Extend backwards, stopping at line number 1, but do not disturb 0
            // since that is a special line number that can show up when addr2line
            // cannot determine the real line number.
            if (f.begin > expand) {
                f.begin -= expand;
            }
            else if (f.begin > 1) {
                f.begin = 1;
            }

        }
        else
 { 
            // Find gap from predecessor and divide between predecessor and f.
            var halfGap = (f.begin - funcs[i - 1].end) / 2;
            if (halfGap > expand) {
                halfGap = expand;
            }

            funcs[i - 1].end += halfGap;
            f.begin -= halfGap;

        }
        funcs[i] = f;

    }    if (len(funcs) > 0) {
        funcs[len(funcs) - 1].end += expand;
    }
    return funcs;

}

// objectFile return the object for the specified mapping, opening it if necessary.
// It returns nil on error.
private static plugin.ObjFile objectFile(this ptr<sourcePrinter> _addr_sp, ptr<profile.Mapping> _addr_m) {
    ref sourcePrinter sp = ref _addr_sp.val;
    ref profile.Mapping m = ref _addr_m.val;

    if (m == null) {
        return null;
    }
    {
        var object__prev1 = object;

        var (object, ok) = sp.objects[m.File];

        if (ok) {
            return object; // May be nil if we detected an error earlier.
        }
        object = object__prev1;

    }

    var (object, err) = sp.objectTool.Open(m.File, m.Start, m.Limit, m.Offset);
    if (err != null) {
        object = null;
    }
    sp.objects[m.File] = object; // Cache even on error.
    return object;

}

// printHeader prints the page header for a weblist report.
private static void printHeader(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    fmt.Fprintln(w, "\n<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"UTF-8\">\n<title>Pprof listing</title" +
    ">");
    fmt.Fprintln(w, weblistPageCSS);
    fmt.Fprintln(w, weblistPageScript);
    fmt.Fprint(w, "</head>\n<body>\n\n");

    slice<@string> labels = default;
    foreach (var (_, l) in ProfileLabels(rpt)) {
        labels = append(labels, template.HTMLEscapeString(l));
    }    fmt.Fprintf(w, "<div class=\"legend\">%s<br>Total: %s</div>", strings.Join(labels, "<br>\n"), rpt.formatValue(rpt.total));

}

// printFunctionHeader prints a function header for a weblist report.
private static void printFunctionHeader(io.Writer w, @string name, @string path, long flatSum, long cumSum, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    fmt.Fprintf(w, "<h2>%s</h2><p class=\"filename\">%s</p>\n<pre onClick=\"pprof_toggle_asm(event)\">\n  T" +
    "otal:  %10s %10s (flat, cum) %s\n", template.HTMLEscapeString(name), template.HTMLEscapeString(path), rpt.formatValue(flatSum), rpt.formatValue(cumSum), measurement.Percentage(cumSum, rpt.total));

}

// printFunctionSourceLine prints a source line and the corresponding assembly.
private static void printFunctionSourceLine(io.Writer w, nint lineNo, long flat, long cum, @string lineContents, slice<assemblyInstruction> assembly, ptr<sourceReader> _addr_reader, ptr<Report> _addr_rpt) {
    ref sourceReader reader = ref _addr_reader.val;
    ref Report rpt = ref _addr_rpt.val;

    if (len(assembly) == 0) {
        fmt.Fprintf(w, "<span class=line> %6d</span> <span class=nop>  %10s %10s %8s  %s </span>\n", lineNo, valueOrDot(flat, rpt), valueOrDot(cum, rpt), "", template.HTMLEscapeString(lineContents));
        return ;
    }
    var nestedInfo = false;
    @string cl = "deadsrc";
    foreach (var (_, an) in assembly) {
        if (len(an.inlineCalls) > 0 || an.instruction != synthAsm) {
            nestedInfo = true;
            cl = "livesrc";
        }
    }    fmt.Fprintf(w, "<span class=line> %6d</span> <span class=%s>  %10s %10s %8s  %s </span>", lineNo, cl, valueOrDot(flat, rpt), valueOrDot(cum, rpt), "", template.HTMLEscapeString(lineContents));
    if (nestedInfo) {
        var srcIndent = indentation(lineContents);
        printNested(w, srcIndent, assembly, _addr_reader, _addr_rpt);
    }
    fmt.Fprintln(w);

}

private static void printNested(io.Writer w, nint srcIndent, slice<assemblyInstruction> assembly, ptr<sourceReader> _addr_reader, ptr<Report> _addr_rpt) {
    ref sourceReader reader = ref _addr_reader.val;
    ref Report rpt = ref _addr_rpt.val;

    fmt.Fprint(w, "<span class=asm>");
    slice<callID> curCalls = default;
    foreach (var (i, an) in assembly) {
        if (an.startsBlock && i != 0) { 
            // Insert a separator between discontiguous blocks.
            fmt.Fprintf(w, " %8s %28s\n", "", "⋮");

        }
        @string fileline = default;
        if (an.file != "") {
            fileline = fmt.Sprintf("%s:%d", template.HTMLEscapeString(filepath.Base(an.file)), an.line);
        }
        var flat = an.flat;
        var cum = an.cum; 

        // Print inlined call context.
        foreach (var (j, c) in an.inlineCalls) {
            if (j < len(curCalls) && curCalls[j] == c) { 
                // Skip if same as previous instruction.
                continue;

            }

            curCalls = null;
            var (fline, ok) = reader.line(c.file, c.line);
            if (!ok) {
                fline = "";
            }

            var text = strings.Repeat(" ", srcIndent + 4 + 4 * j) + strings.TrimSpace(fline);
            fmt.Fprintf(w, " %8s %10s %10s %8s  <span class=inlinesrc>%s</span> <span class=unimportant>%s:%d</span>\n", "", "", "", "", template.HTMLEscapeString(rightPad(text, 80)), template.HTMLEscapeString(filepath.Base(c.file)), c.line);

        }        curCalls = an.inlineCalls;
        if (an.instruction == synthAsm) {
            continue;
        }
        text = strings.Repeat(" ", srcIndent + 4 + 4 * len(curCalls)) + an.instruction;
        fmt.Fprintf(w, " %8s %10s %10s %8x: %s <span class=unimportant>%s</span>\n", "", valueOrDot(flat, rpt), valueOrDot(cum, rpt), an.address, template.HTMLEscapeString(rightPad(text, 80)), fileline);

    }    fmt.Fprint(w, "</span>");

}

// printFunctionClosing prints the end of a function in a weblist report.
private static void printFunctionClosing(io.Writer w) {
    fmt.Fprintln(w, "</pre>");
}

// printPageClosing prints the end of the page in a weblist report.
private static void printPageClosing(io.Writer w) {
    fmt.Fprintln(w, weblistPageClosing);
}

// getSourceFromFile collects the sources of a function from a source
// file and annotates it with the samples in fns. Returns the sources
// as nodes, using the info.name field to hold the source code.
private static (graph.Nodes, @string, error) getSourceFromFile(@string file, ptr<sourceReader> _addr_reader, graph.Nodes fns, nint start, nint end) {
    graph.Nodes _p0 = default;
    @string _p0 = default;
    error _p0 = default!;
    ref sourceReader reader = ref _addr_reader.val;

    var lineNodes = make_map<nint, graph.Nodes>(); 

    // Collect source coordinates from profile.
    const nint margin = 5; // Lines before first/after last sample.
 // Lines before first/after last sample.
    if (start == 0) {
        if (fns[0].Info.StartLine != 0) {
            start = fns[0].Info.StartLine;
        }
        else
 {
            start = fns[0].Info.Lineno - margin;
        }
    }
    else
 {
        start -= margin;
    }
    if (end == 0) {
        end = fns[0].Info.Lineno;
    }
    end += margin;
    foreach (var (_, n) in fns) {
        var lineno = n.Info.Lineno;
        var nodeStart = n.Info.StartLine;
        if (nodeStart == 0) {
            nodeStart = lineno - margin;
        }
        var nodeEnd = lineno + margin;
        if (nodeStart < start) {
            start = nodeStart;
        }
        else if (nodeEnd > end) {
            end = nodeEnd;
        }
        lineNodes[lineno] = append(lineNodes[lineno], n);

    }    if (start < 1) {
        start = 1;
    }
    graph.Nodes src = default;
    {
        var lineno__prev1 = lineno;

        for (lineno = start; lineno <= end; lineno++) {
            var (line, ok) = reader.line(file, lineno);
            if (!ok) {
                break;
            }
            var (flat, cum) = lineNodes[lineno].Sum();
            src = append(src, addr(new graph.Node(Info:graph.NodeInfo{Name:strings.TrimRight(line,"\n"),Lineno:lineno,},Flat:flat,Cum:cum,)));
        }

        lineno = lineno__prev1;
    }
    {
        var err = reader.fileError(file);

        if (err != null) {
            return (null, file, error.As(err)!);
        }
    }

    return (src, file, error.As(null!)!);

}

// sourceReader provides access to source code with caching of file contents.
private partial struct sourceReader {
    public @string searchPath; // trimPath is a filepath.ListSeparator-separated list of paths to trim.
    public @string trimPath; // files maps from path name to a list of lines.
// files[*][0] is unused since line numbering starts at 1.
    public map<@string, slice<@string>> files; // errors collects errors encountered per file. These errors are
// consulted before returning out of these module.
    public map<@string, error> errors;
}

private static ptr<sourceReader> newSourceReader(@string searchPath, @string trimPath) {
    return addr(new sourceReader(searchPath,trimPath,make(map[string][]string),make(map[string]error),));
}

private static error fileError(this ptr<sourceReader> _addr_reader, @string path) {
    ref sourceReader reader = ref _addr_reader.val;

    return error.As(reader.errors[path])!;
}

// line returns the line numbered "lineno" in path, or _,false if lineno is out of range.
private static (@string, bool) line(this ptr<sourceReader> _addr_reader, @string path, nint lineno) {
    @string _p0 = default;
    bool _p0 = default;
    ref sourceReader reader = ref _addr_reader.val;

    var (lines, ok) = reader.files[path];
    if (!ok) { 
        // Read and cache file contents.
        lines = new slice<@string>(new @string[] { "" }); // Skip 0th line
        var (f, err) = openSourceFile(path, reader.searchPath, reader.trimPath);
        if (err != null) {
            reader.errors[path] = err;
        }
        else
 {
            var s = bufio.NewScanner(f);
            while (s.Scan()) {
                lines = append(lines, s.Text());
            }

            f.Close();
            if (s.Err() != null) {
                reader.errors[path] = err;
            }
        }
        reader.files[path] = lines;

    }
    if (lineno <= 0 || lineno >= len(lines)) {
        return ("", false);
    }
    return (lines[lineno], true);

}

// openSourceFile opens a source file from a name encoded in a profile. File
// names in a profile after can be relative paths, so search them in each of
// the paths in searchPath and their parents. In case the profile contains
// absolute paths, additional paths may be configured to trim from the source
// paths in the profile. This effectively turns the path into a relative path
// searching it using searchPath as usual).
private static (ptr<os.File>, error) openSourceFile(@string path, @string searchPath, @string trim) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;

    path = trimPath(path, trim, searchPath); 
    // If file is still absolute, require file to exist.
    if (filepath.IsAbs(path)) {
        var (f, err) = os.Open(path);
        return (_addr_f!, error.As(err)!);
    }
    foreach (var (_, dir) in filepath.SplitList(searchPath)) { 
        // Search up for every parent of each possible path.
        while (true) {
            var filename = filepath.Join(dir, path);
            {
                var f__prev1 = f;

                (f, err) = os.Open(filename);

                if (err == null) {
                    return (_addr_f!, error.As(null!)!);
                }

                f = f__prev1;

            }

            var parent = filepath.Dir(dir);
            if (parent == dir) {
                break;
            }

            dir = parent;

        }

    }    return (_addr_null!, error.As(fmt.Errorf("could not find file %s on path %s", path, searchPath))!);

}

// trimPath cleans up a path by removing prefixes that are commonly
// found on profiles plus configured prefixes.
// TODO(aalexand): Consider optimizing out the redundant work done in this
// function if it proves to matter.
private static @string trimPath(@string path, @string trimPath, @string searchPath) { 
    // Keep path variable intact as it's used below to form the return value.
    var sPath = filepath.ToSlash(path);
    var searchPath = filepath.ToSlash(searchPath);
    if (trimPath == "") { 
        // If the trim path is not configured, try to guess it heuristically:
        // search for basename of each search path in the original path and, if
        // found, strip everything up to and including the basename. So, for
        // example, given original path "/some/remote/path/my-project/foo/bar.c"
        // and search path "/my/local/path/my-project" the heuristic will return
        // "/my/local/path/my-project/foo/bar.c".
        foreach (var (_, dir) in filepath.SplitList(searchPath)) {
            @string want = "/" + filepath.Base(dir) + "/";
            {
                var found = strings.Index(sPath, want);

                if (found != -1) {
                    return path[(int)found + len(want)..];
                }

            }

        }
    }
    var trimPaths = append(filepath.SplitList(filepath.ToSlash(trimPath)), "/proc/self/cwd/./", "/proc/self/cwd/");
    foreach (var (_, trimPath) in trimPaths) {
        if (!strings.HasSuffix(trimPath, "/")) {
            trimPath += "/";
        }
        if (strings.HasPrefix(sPath, trimPath)) {
            return path[(int)len(trimPath)..];
        }
    }    return path;

}

private static nint indentation(@string line) {
    nint column = 0;
    foreach (var (_, c) in line) {
        if (c == ' ') {
            column++;
        }
        else if (c == '\t') {
            column++;
            while (column % 8 != 0) {
                column++;
            }
        else


        } {
            break;
        }
    }    return column;

}

// rightPad pads the input with spaces on the right-hand-side to make it have
// at least width n. It treats tabs as enough spaces that lead to the next
// 8-aligned tab-stop.
private static @string rightPad(@string s, nint n) {
    strings.Builder str = default; 

    // Convert tabs to spaces as we go so padding works regardless of what prefix
    // is placed before the result.
    nint column = 0;
    foreach (var (_, c) in s) {
        column++;
        if (c == '\t') {
            str.WriteRune(' ');
            while (column % 8 != 0) {
                column++;
                str.WriteRune(' ');
            }
        else


        } {
            str.WriteRune(c);
        }
    }    while (column < n) {
        column++;
        str.WriteRune(' ');
    }
    return str.String();

}

private static @string canonicalizeFileName(@string fname) {
    fname = strings.TrimPrefix(fname, "/proc/self/cwd/");
    fname = strings.TrimPrefix(fname, "./");
    return filepath.Clean(fname);
}

} // end report_package
