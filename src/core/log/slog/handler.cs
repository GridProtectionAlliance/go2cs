// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using context = context_package;
using fmt = fmt_package;
using io = io_package;
using buffer = log.slog.@internal.buffer_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using sync = sync_package;
using time = time_package;
using log.slog.@internal;

partial class slog_package {

// A Handler handles log records produced by a Logger.
//
// A typical handler may print log records to standard error,
// or write them to a file or database, or perhaps augment them
// with additional attributes and pass them on to another handler.
//
// Any of the Handler's methods may be called concurrently with itself
// or with other methods. It is the responsibility of the Handler to
// manage this concurrency.
//
// Users of the slog package should not invoke Handler methods directly.
// They should use the methods of [Logger] instead.
[GoType] partial interface ΔHandler {
    // Enabled reports whether the handler handles records at the given level.
    // The handler ignores records whose level is lower.
    // It is called early, before any arguments are processed,
    // to save effort if the log event should be discarded.
    // If called from a Logger method, the first argument is the context
    // passed to that method, or context.Background() if nil was passed
    // or the method does not take a context.
    // The context is passed so Enabled can use its values
    // to make a decision.
    bool Enabled(context.Context _, ΔLevel _);
    // Handle handles the Record.
    // It will only be called when Enabled returns true.
    // The Context argument is as for Enabled.
    // It is present solely to provide Handlers access to the context's values.
    // Canceling the context should not affect record processing.
    // (Among other things, log messages may be necessary to debug a
    // cancellation-related problem.)
    //
    // Handle methods that produce output should observe the following rules:
    //   - If r.Time is the zero time, ignore the time.
    //   - If r.PC is zero, ignore it.
    //   - Attr's values should be resolved.
    //   - If an Attr's key and value are both the zero value, ignore the Attr.
    //     This can be tested with attr.Equal(Attr{}).
    //   - If a group's key is empty, inline the group's Attrs.
    //   - If a group has no Attrs (even if it has a non-empty key),
    //     ignore it.
    error Handle(context.Context _, Record _);
    // WithAttrs returns a new Handler whose attributes consist of
    // both the receiver's attributes and the arguments.
    // The Handler owns the slice: it may retain, modify or discard it.
    ΔHandler WithAttrs(slice<Attr> attrs);
    // WithGroup returns a new Handler with the given group appended to
    // the receiver's existing groups.
    // The keys of all subsequent attributes, whether added by With or in a
    // Record, should be qualified by the sequence of group names.
    //
    // How this qualification happens is up to the Handler, so long as
    // this Handler's attribute keys differ from those of another Handler
    // with a different sequence of group names.
    //
    // A Handler should treat WithGroup as starting a Group of Attrs that ends
    // at the end of the log event. That is,
    //
    //     logger.WithGroup("s").LogAttrs(ctx, level, msg, slog.Int("a", 1), slog.Int("b", 2))
    //
    // should behave like
    //
    //     logger.LogAttrs(ctx, level, msg, slog.Group("s", slog.Int("a", 1), slog.Int("b", 2)))
    //
    // If the name is empty, WithGroup returns the receiver.
    ΔHandler WithGroup(@string name);
}

[GoType] partial struct defaultHandler {
    internal ж<commonHandler> ch;
    // internal.DefaultOutput, except for testing
    internal Func<uintptr, slice<byte>, error> output;
}

internal static ж<defaultHandler> newDefaultHandler(Func<uintptr, slice<byte>, error> output) {
    return Ꮡ(new defaultHandler(
        ch: Ꮡ(new commonHandler(json: false)),
        output: output
    ));
}

[GoRecv] internal static bool Enabled(this ref defaultHandler _, context.Context _, ΔLevel l) {
    return l >= logLoggerLevel.Level();
}

// Collect the level, attributes and message in a string and
// write it with the default log.Logger.
// Let the log.Logger handle time and file/line.
[GoRecv] internal static error Handle(this ref defaultHandler h, context.Context ctx, Record r) => func((defer, _) => {
    var buf = buffer.New();
    buf.WriteString(r.Level.String());
    buf.WriteByte((rune)' ');
    buf.WriteString(r.Message);
    ref var state = ref heap<handleState>(out var Ꮡstate);
    state = h.ch.newHandleState(buf, true, " "u8);
    var stateʗ1 = state;
    defer(stateʗ1.free);
    state.appendNonBuiltIns(r);
    return h.output(r.PC, buf.val);
});

[GoRecv] internal static ΔHandler WithAttrs(this ref defaultHandler h, slice<Attr> @as) {
    return new defaultHandler(h.ch.withAttrs(@as), h.output);
}

[GoRecv] internal static ΔHandler WithGroup(this ref defaultHandler h, @string name) {
    return new defaultHandler(h.ch.withGroup(name), h.output);
}

// HandlerOptions are options for a [TextHandler] or [JSONHandler].
// A zero HandlerOptions consists entirely of default values.
[GoType] partial struct HandlerOptions {
    // AddSource causes the handler to compute the source code position
    // of the log statement and add a SourceKey attribute to the output.
    public bool AddSource;
    // Level reports the minimum record level that will be logged.
    // The handler discards records with lower levels.
    // If Level is nil, the handler assumes LevelInfo.
    // The handler calls Level.Level for each record processed;
    // to adjust the minimum level dynamically, use a LevelVar.
    public Leveler Level;
    // ReplaceAttr is called to rewrite each non-group attribute before it is logged.
    // The attribute's value has been resolved (see [Value.Resolve]).
    // If ReplaceAttr returns a zero Attr, the attribute is discarded.
    //
    // The built-in attributes with keys "time", "level", "source", and "msg"
    // are passed to this function, except that time is omitted
    // if zero, and source is omitted if AddSource is false.
    //
    // The first argument is a list of currently open groups that contain the
    // Attr. It must not be retained or modified. ReplaceAttr is never called
    // for Group attributes, only their contents. For example, the attribute
    // list
    //
    //     Int("a", 1), Group("g", Int("b", 2)), Int("c", 3)
    //
    // results in consecutive calls to ReplaceAttr with the following arguments:
    //
    //     nil, Int("a", 1)
    //     []string{"g"}, Int("b", 2)
    //     nil, Int("c", 3)
    //
    // ReplaceAttr can be used to change the default keys of the built-in
    // attributes, convert types (for example, to replace a `time.Time` with the
    // integer seconds since the Unix epoch), sanitize personal information, or
    // remove attributes from the output.
    public slog.Attr ReplaceAttr;
}

// Keys for "built-in" attributes.
public static readonly @string TimeKey = "time"u8;

public static readonly @string LevelKey = "level"u8;

public static readonly @string MessageKey = "msg"u8;

public static readonly @string SourceKey = "source"u8;

[GoType] partial struct commonHandler {
    internal bool json; // true => output JSON; false => output text
    internal HandlerOptions opts;
    internal slice<byte> preformattedAttrs;
    // groupPrefix is for the text handler only.
    // It holds the prefix for groups that were already pre-formatted.
    // A group will appear here when a call to WithGroup is followed by
    // a call to WithAttrs.
    internal @string groupPrefix;
    internal slice<@string> groups; // all groups started from WithGroup
    internal nint nOpenGroups;     // the number of groups opened in preformattedAttrs
    internal ж<sync_package.Mutex> mu;
    internal io_package.Writer w;
}

[GoRecv] internal static ж<commonHandler> clone(this ref commonHandler h) {
    // We can't use assignment because we can't copy the mutex.
    return Ꮡ(new commonHandler(
        json: h.json,
        opts: h.opts,
        preformattedAttrs: slices.Clip(h.preformattedAttrs),
        groupPrefix: h.groupPrefix,
        groups: slices.Clip(h.groups),
        nOpenGroups: h.nOpenGroups,
        w: h.w,
        mu: h.mu
    ));
}

// mutex shared among all clones of this handler

// enabled reports whether l is greater than or equal to the
// minimum level.
[GoRecv] internal static bool enabled(this ref commonHandler h, ΔLevel l) {
    ΔLevel minLevel = LevelInfo;
    if (h.opts.Level != default!) {
        minLevel = h.opts.Level.Level();
    }
    return l >= minLevel;
}

[GoRecv("capture")] internal static ж<commonHandler> withAttrs(this ref commonHandler h, slice<Attr> @as) => func((defer, _) => {
    // We are going to ignore empty groups, so if the entire slice consists of
    // them, there is nothing to do.
    if (countEmptyGroups(@as) == len(@as)) {
        return withAttrsꓸᏑh;
    }
    var h2 = h.clone();
    // Pre-format the attributes as an optimization.
    ref var state = ref heap<handleState>(out var Ꮡstate);
    state = h2.newHandleState((ж<buffer.Buffer>)(Ꮡ((~h2).preformattedAttrs)), false, ""u8);
    var stateʗ1 = state;
    defer(stateʗ1.free);
    state.prefix.WriteString(h.groupPrefix);
    {
        var pfa = h2.val.preformattedAttrs; if (len(pfa) > 0) {
            state.sep = h.attrSep();
            if ((~h2).json && pfa[len(pfa) - 1] == (rune)'{') {
                state.sep = ""u8;
            }
        }
    }
    // Remember the position in the buffer, in case all attrs are empty.
    nint pos = state.buf.Len();
    state.openGroups();
    if (!state.appendAttrs(@as)){
        state.buf.SetLen(pos);
    } else {
        // Remember the new prefix for later keys.
        h2.val.groupPrefix = state.prefix.String();
        // Remember how many opened groups are in preformattedAttrs,
        // so we don't open them again when we handle a Record.
        h2.val.nOpenGroups = len((~h2).groups);
    }
    return h2;
});

[GoRecv] internal static ж<commonHandler> withGroup(this ref commonHandler h, @string name) {
    var h2 = h.clone();
    h2.val.groups = append((~h2).groups, name);
    return h2;
}

// handle is the internal implementation of Handler.Handle
// used by TextHandler and JSONHandler.
[GoRecv] internal static error handle(this ref commonHandler h, Record r) => func((defer, _) => {
    ref var state = ref heap<handleState>(out var Ꮡstate);
    state = h.newHandleState(buffer.New(), true, ""u8);
    var stateʗ1 = state;
    defer(stateʗ1.free);
    if (h.json) {
        state.buf.WriteByte((rune)'{');
    }
    // Built-in attributes. They are not in a group.
    var stateGroups = state.groups;
    state.groups = default!;
    // So ReplaceAttrs sees no groups instead of the pre groups.
    var rep = h.opts.ReplaceAttr;
    // time
    if (!r.Time.IsZero()) {
        @string keyΔ1 = TimeKey;
        var valΔ1 = r.Time.Round(0);
        // strip monotonic to match Attr behavior
        if (rep == default!){
            state.appendKey(keyΔ1);
            state.appendTime(valΔ1);
        } else {
            state.appendAttr(Time(keyΔ1, valΔ1));
        }
    }
    // level
    @string key = LevelKey;
    ΔLevel val = r.Level;
    if (rep == default!){
        state.appendKey(key);
        state.appendString(val.String());
    } else {
        state.appendAttr(Any(key, val));
    }
    // source
    if (h.opts.AddSource) {
        state.appendAttr(Any(SourceKey, r.source()));
    }
    key = MessageKey;
    @string msg = r.Message;
    if (rep == default!){
        state.appendKey(key);
        state.appendString(msg);
    } else {
        state.appendAttr(String(key, msg));
    }
    state.groups = stateGroups;
    // Restore groups passed to ReplaceAttrs.
    state.appendNonBuiltIns(r);
    state.buf.WriteByte((rune)'\n');
    h.mu.Lock();
    defer(h.mu.Unlock);
    var (_, err) = h.w.Write(state.buf.val);
    return err;
});

[GoRecv] internal static void appendNonBuiltIns(this ref handleState s, Record r) {
    // preformatted Attrs
    {
        var pfa = s.h.preformattedAttrs; if (len(pfa) > 0) {
            s.buf.WriteString(s.sep);
            s.buf.Write(pfa);
            s.sep = s.h.attrSep();
            if (s.h.json && pfa[len(pfa) - 1] == (rune)'{') {
                s.sep = ""u8;
            }
        }
    }
    // Attrs in Record -- unlike the built-in ones, they are in groups started
    // from WithGroup.
    // If the record has no Attrs, don't output any groups.
    nint nOpenGroups = s.h.nOpenGroups;
    if (r.NumAttrs() > 0) {
        s.prefix.WriteString(s.h.groupPrefix);
        // The group may turn out to be empty even though it has attrs (for
        // example, ReplaceAttr may delete all the attrs).
        // So remember where we are in the buffer, to restore the position
        // later if necessary.
        nint pos = s.buf.Len();
        s.openGroups();
        nOpenGroups = len(s.h.groups);
        var empty = true;
        r.Attrs((Attr a) => {
            if (s.appendAttr(a)) {
                empty = false;
            }
            return true;
        });
        if (empty) {
            s.buf.SetLen(pos);
            nOpenGroups = s.h.nOpenGroups;
        }
    }
    if (s.h.json) {
        // Close all open groups.
        foreach ((_, _) in s.h.groups[..(int)(nOpenGroups)]) {
            s.buf.WriteByte((rune)'}');
        }
        // Close the top-level object.
        s.buf.WriteByte((rune)'}');
    }
}

// attrSep returns the separator between attributes.
[GoRecv] internal static @string attrSep(this ref commonHandler h) {
    if (h.json) {
        return ","u8;
    }
    return " "u8;
}

// handleState holds state for a single call to commonHandler.handle.
// The initial value of sep determines whether to emit a separator
// before the next key, after which it stays true.
[GoType] partial struct handleState {
    internal ж<commonHandler> h;
    internal ж<log.slog.@internal.buffer_package.Buffer> buf;
    internal bool freeBuf;           // should buf be freed?
    internal @string sep;        // separator to write before next key
    internal ж<log.slog.@internal.buffer_package.Buffer> prefix; // for text: key prefix
    internal ж<slice<@string>> groups; // pool-allocated slice of active groups, for ReplaceAttr
}

internal static sync.Pool groupPool = new sync.Pool(New: () => {
    var s = new slice<@string>(0, 10);
    return Ꮡ(s);
}
);

[GoRecv] internal static handleState newHandleState(this ref commonHandler h, ж<buffer.Buffer> Ꮡbuf, bool freeBuf, @string sep) {
    ref var buf = ref Ꮡbuf.val;

    var s = new handleState(
        h: h,
        buf: buf,
        freeBuf: freeBuf,
        sep: sep,
        prefix: buffer.New()
    );
    if (h.opts.ReplaceAttr != default!) {
        s.groups = groupPool.Get()._<slice<@string>.val>();
        s.groups.val = append(s.groups.val, h.groups[..(int)(h.nOpenGroups)].ꓸꓸꓸ);
    }
    return s;
}

[GoRecv] internal static unsafe void free(this ref handleState s) {
    if (s.freeBuf) {
        s.buf.Free();
    }
    {
        var gs = s.groups; if (gs != nil) {
            gs.val = new Span<ж<slice<@string>>>((slice<@string>**), 0);
            groupPool.Put(gs);
        }
    }
    s.prefix.Free();
}

[GoRecv] internal static void openGroups(this ref handleState s) {
    foreach (var (_, n) in s.h.groups[(int)(s.h.nOpenGroups)..]) {
        s.openGroup(n);
    }
}

// Separator for group names and keys.
internal static readonly UntypedInt keyComponentSep = /* '.' */ 46;

// openGroup starts a new group of attributes
// with the given name.
[GoRecv] internal static void openGroup(this ref handleState s, @string name) {
    if (s.h.json){
        s.appendKey(name);
        s.buf.WriteByte((rune)'{');
        s.sep = ""u8;
    } else {
        s.prefix.WriteString(name);
        s.prefix.WriteByte(keyComponentSep);
    }
    // Collect group names for ReplaceAttr.
    if (s.groups != nil) {
        s.groups.val = append(s.groups.val, name);
    }
}

// closeGroup ends the group with the given name.
[GoRecv] internal static void closeGroup(this ref handleState s, @string name) {
    if (s.h.json){
        s.buf.WriteByte((rune)'}');
    } else {
        (s.prefix.val) = (s.prefix.val)[..(int)(len(s.prefix.val) - len(name) - 1)];
    }
    /* for keyComponentSep */
    s.sep = s.h.attrSep();
    if (s.groups != nil) {
        s.groups.val = (s.groups.val)[..(int)(len(s.groups.val) - 1)];
    }
}

// appendAttrs appends the slice of Attrs.
// It reports whether something was appended.
[GoRecv] internal static bool appendAttrs(this ref handleState s, slice<Attr> @as) {
    var nonEmpty = false;
    foreach (var (_, a) in @as) {
        if (s.appendAttr(a)) {
            nonEmpty = true;
        }
    }
    return nonEmpty;
}

// appendAttr appends the Attr's key and value.
// It handles replacement and checking for an empty key.
// It reports whether something was appended.
[GoRecv] internal static bool appendAttr(this ref handleState s, Attr a) {
    a.Value = a.Value.Resolve();
    {
        var rep = s.h.opts.ReplaceAttr; if (rep != default! && a.Value.Kind() != KindGroup) {
            slice<@string> gs = default!;
            if (s.groups != nil) {
                gs = s.groups.val;
            }
            // a.Value is resolved before calling ReplaceAttr, so the user doesn't have to.
            a = rep(gs, a);
            // The ReplaceAttr function may return an unresolved Attr.
            a.Value = a.Value.Resolve();
        }
    }
    // Elide empty Attrs.
    if (a.isEmpty()) {
        return false;
    }
    // Special case: Source.
    {
        var v = a.Value; if (v.Kind() == KindAny) {
            {
                var (src, ok) = v.Any()._<Source.val>(ᐧ); if (ok) {
                    if (s.h.json){
                        a.Value = src.group();
                    } else {
                        a.Value = StringValue(fmt.Sprintf("%s:%d"u8, (~src).File, (~src).Line));
                    }
                }
            }
        }
    }
    if (a.Value.Kind() == KindGroup){
        var attrs = a.Value.Group();
        // Output only non-empty groups.
        if (len(attrs) > 0) {
            // The group may turn out to be empty even though it has attrs (for
            // example, ReplaceAttr may delete all the attrs).
            // So remember where we are in the buffer, to restore the position
            // later if necessary.
            nint pos = s.buf.Len();
            // Inline a group with an empty key.
            if (a.Key != ""u8) {
                s.openGroup(a.Key);
            }
            if (!s.appendAttrs(attrs)) {
                s.buf.SetLen(pos);
                return false;
            }
            if (a.Key != ""u8) {
                s.closeGroup(a.Key);
            }
        }
    } else {
        s.appendKey(a.Key);
        s.appendValue(a.Value);
    }
    return true;
}

[GoRecv] internal static void appendError(this ref handleState s, error err) {
    s.appendString(fmt.Sprintf("!ERROR:%v"u8, err));
}

[GoRecv] internal static void appendKey(this ref handleState s, @string key) {
    s.buf.WriteString(s.sep);
    if (s.prefix != nil && len(s.prefix.val) > 0){
        // TODO: optimize by avoiding allocation.
        s.appendString(((@string)(s.prefix.val)) + key);
    } else {
        s.appendString(key);
    }
    if (s.h.json){
        s.buf.WriteByte((rune)':');
    } else {
        s.buf.WriteByte((rune)'=');
    }
    s.sep = s.h.attrSep();
}

[GoRecv] internal static void appendString(this ref handleState s, @string str) {
    if (s.h.json){
        s.buf.WriteByte((rune)'"');
        s.buf.val = appendEscapedJSONString(s.buf.val, str);
        s.buf.WriteByte((rune)'"');
    } else {
        // text
        if (needsQuoting(str)){
            s.buf.val = strconv.AppendQuote(s.buf.val, str);
        } else {
            s.buf.WriteString(str);
        }
    }
}

[GoRecv] internal static void appendValue(this ref handleState s, Value v) => func((defer, recover) => {
    var vʗ1 = v;
    defer(() => {
        {
            var r = recover(); if (r != default!) {
                // If it panics with a nil pointer, the most likely cases are
                // an encoding.TextMarshaler or error fails to guard against nil,
                // in which case "<nil>" seems to be the feasible choice.
                //
                // Adapted from the code in fmt/print.go.
                {
                    ref var vʗ1 = ref heap<reflect_package.ΔValue>(out var Ꮡvʗ1);
                    vʗ1 = reflect.ValueOf(vʗ1.any); if (vʗ1.Kind() == reflect.ΔPointer && vʗ1.IsNil()) {
                        s.appendString("<nil>"u8);
                        return;
                    }
                }
                // Otherwise just print the original panic message.
                s.appendString(fmt.Sprintf("!PANIC: %v"u8, r));
            }
        }
    });
    error err = default!;
    if (s.h.json){
        err = appendJSONValue(s, v);
    } else {
        err = appendTextValue(s, v);
    }
    if (err != default!) {
        s.appendError(err);
    }
});

[GoRecv] internal static void appendTime(this ref handleState s, time.Time t) {
    if (s.h.json){
        appendJSONTime(s, t);
    } else {
        s.buf.val = appendRFC3339Millis(s.buf.val, t);
    }
}

internal static slice<byte> appendRFC3339Millis(slice<byte> b, time.Time t) {
    // Format according to time.RFC3339Nano since it is highly optimized,
    // but truncate it to use millisecond resolution.
    // Unfortunately, that format trims trailing 0s, so add 1/10 millisecond
    // to guarantee that there are exactly 4 digits after the period.
    const nint prefixLen = /* len("2006-01-02T15:04:05.000") */ 23;
    nint n = len(b);
    t = t.Truncate(time.Millisecond).Add(time.Millisecond / 10);
    b = t.AppendFormat(b, time.RFC3339Nano);
    b = append(b[..(int)(n + prefixLen)], b[(int)(n + prefixLen + 1)..].ꓸꓸꓸ);
    // drop the 4th digit
    return b;
}

} // end slog_package
