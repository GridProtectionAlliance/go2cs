// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using context = context_package;
using log = log_package;
using loginternal = log.internal_package;
using @internal = log.slog.internal_package;
using runtime = runtime_package;
using atomic = sync.atomic_package;
using time = time_package;
using log.slog;
using sync;
using ꓸꓸꓸAttr = Span<Attr>;
using ꓸꓸꓸany = Span<any>;

partial class slog_package {

internal static atomic.Pointer<Logger> defaultLogger;

internal static LevelVar logLoggerLevel;

// SetLogLoggerLevel controls the level for the bridge to the [log] package.
//
// Before [SetDefault] is called, slog top-level logging functions call the default [log.Logger].
// In that mode, SetLogLoggerLevel sets the minimum level for those calls.
// By default, the minimum level is Info, so calls to [Debug]
// (as well as top-level logging calls at lower levels)
// will not be passed to the log.Logger. After calling
//
//	slog.SetLogLoggerLevel(slog.LevelDebug)
//
// calls to [Debug] will be passed to the log.Logger.
//
// After [SetDefault] is called, calls to the default [log.Logger] are passed to the
// slog default handler. In that mode,
// SetLogLoggerLevel sets the level at which those calls are logged.
// That is, after calling
//
//	slog.SetLogLoggerLevel(slog.LevelDebug)
//
// A call to [log.Printf] will result in output at level [LevelDebug].
//
// SetLogLoggerLevel returns the previous value.
public static ΔLevel /*oldLevel*/ SetLogLoggerLevel(ΔLevel level) {
    ΔLevel oldLevel = default!;

    oldLevel = logLoggerLevel.Level();
    logLoggerLevel.Set(level);
    return oldLevel;
}

[GoInit] internal static void init() {
    defaultLogger.Store(New(~newDefaultHandler(loginternal.DefaultOutput)));
}

// Default returns the default [Logger].
public static ж<Logger> Default() {
    return defaultLogger.Load();
}

// SetDefault makes l the default [Logger], which is used by
// the top-level functions [Info], [Debug] and so on.
// After this call, output from the log package's default Logger
// (as with [log.Print], etc.) will be logged using l's Handler,
// at a level controlled by [SetLogLoggerLevel].
public static void SetDefault(ж<Logger> Ꮡl) {
    ref var l = ref Ꮡl.val;

    defaultLogger.Store(Ꮡl);
    // If the default's handler is a defaultHandler, then don't use a handleWriter,
    // or we'll deadlock as they both try to acquire the log default mutex.
    // The defaultHandler will use whatever the log default writer is currently
    // set to, which is correct.
    // This can occur with SetDefault(Default()).
    // See TestSetDefault.
    {
        var (_, ok) = l.Handler()._<defaultHandler.val>(ᐧ); if (!ok) {
            ref var capturePC = ref heap<bool>(out var ᏑcapturePC);
            capturePC = (nint)(log.Flags() & ((nint)(log.Lshortfile | log.Llongfile))) != 0;
            log.SetOutput(new handlerWriter(l.Handler(), Ꮡ(logLoggerLevel), capturePC));
            log.SetFlags(0);
        }
    }
}

// we want just the log message, no time or location

// handlerWriter is an io.Writer that calls a Handler.
// It is used to link the default log.Logger to the default slog.Logger.
[GoType] partial struct handlerWriter {
    internal ΔHandler h;
    internal Leveler level;
    internal bool capturePC;
}

[GoRecv] internal static (nint, error) Write(this ref handlerWriter w, slice<byte> buf) {
    ΔLevel level = w.level.Level();
    if (!w.h.Enabled(context.Background(), level)) {
        return (0, default!);
    }
    uintptr pc = default!;
    if (!@internal.IgnorePC && w.capturePC) {
        // skip [runtime.Callers, w.Write, Logger.Output, log.Print]
        array<uintptr> pcs = new(1);
        runtime.Callers(4, pcs[..]);
        pc = pcs[0];
    }
    // Remove final newline.
    nint origLen = len(buf);
    // Report that the entire buf was written.
    if (len(buf) > 0 && buf[len(buf) - 1] == (rune)'\n') {
        buf = buf[..(int)(len(buf) - 1)];
    }
    var r = NewRecord(time.Now(), level, ((@string)buf), pc);
    return (origLen, w.h.Handle(context.Background(), r));
}

// A Logger records structured information about each call to its
// Log, Debug, Info, Warn, and Error methods.
// For each call, it creates a [Record] and passes it to a [Handler].
//
// To create a new Logger, call [New] or a Logger method
// that begins "With".
[GoType] partial struct Logger {
    internal ΔHandler handler; // for structured logging
}

[GoRecv] internal static ж<Logger> clone(this ref Logger l) {
    ref var c = ref heap<Logger>(out var Ꮡc);
    c = l;
    return Ꮡc;
}

// Handler returns l's Handler.
[GoRecv] public static ΔHandler Handler(this ref Logger l) {
    return l.handler;
}

// With returns a Logger that includes the given attributes
// in each output operation. Arguments are converted to
// attributes as if by [Logger.Log].
[GoRecv("capture")] public static ж<Logger> With(this ref Logger l, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    if (len(args) == 0) {
        return WithꓸᏑl;
    }
    var c = l.clone();
    c.val.handler = l.handler.WithAttrs(argsToAttrSlice(args));
    return c;
}

// WithGroup returns a Logger that starts a group, if name is non-empty.
// The keys of all attributes added to the Logger will be qualified by the given
// name. (How that qualification happens depends on the [Handler.WithGroup]
// method of the Logger's Handler.)
//
// If name is empty, WithGroup returns the receiver.
[GoRecv("capture")] public static ж<Logger> WithGroup(this ref Logger l, @string name) {
    if (name == ""u8) {
        return WithGroupꓸᏑl;
    }
    var c = l.clone();
    c.val.handler = l.handler.WithGroup(name);
    return c;
}

// New creates a new Logger with the given non-nil Handler.
public static ж<Logger> New(ΔHandler h) {
    if (h == default!) {
        throw panic("nil Handler");
    }
    return Ꮡ(new Logger(handler: h));
}

// With calls [Logger.With] on the default logger.
public static ж<Logger> With(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return Default().With(args.ꓸꓸꓸ);
}

// Enabled reports whether l emits log records at the given context and level.
[GoRecv] public static bool Enabled(this ref Logger l, context.Context ctx, ΔLevel level) {
    if (ctx == default!) {
        ctx = context.Background();
    }
    return l.Handler().Enabled(ctx, level);
}

// NewLogLogger returns a new [log.Logger] such that each call to its Output method
// dispatches a Record to the specified handler. The logger acts as a bridge from
// the older log API to newer structured logging handlers.
public static ж<log.Logger> NewLogLogger(ΔHandler h, ΔLevel level) {
    return log.New(new handlerWriter(h, level, true), ""u8, 0);
}

// Log emits a log record with the current time and the given level and message.
// The Record's Attrs consist of the Logger's attributes followed by
// the Attrs specified by args.
//
// The attribute arguments are processed as follows:
//   - If an argument is an Attr, it is used as is.
//   - If an argument is a string and this is not the last argument,
//     the following argument is treated as the value and the two are combined
//     into an Attr.
//   - Otherwise, the argument is treated as a value with key "!BADKEY".
[GoRecv] public static void Log(this ref Logger l, context.Context ctx, ΔLevel level, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(ctx, level, msg, args.ꓸꓸꓸ);
}

// LogAttrs is a more efficient version of [Logger.Log] that accepts only Attrs.
[GoRecv] public static void LogAttrs(this ref Logger l, context.Context ctx, ΔLevel level, @string msg, params ꓸꓸꓸAttr attrsʗp) {
    var attrs = attrsʗp.slice();

    l.logAttrs(ctx, level, msg, attrs.ꓸꓸꓸ);
}

// Debug logs at [LevelDebug].
[GoRecv] public static void Debug(this ref Logger l, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(context.Background(), LevelDebug, msg, args.ꓸꓸꓸ);
}

// DebugContext logs at [LevelDebug] with the given context.
[GoRecv] public static void DebugContext(this ref Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(ctx, LevelDebug, msg, args.ꓸꓸꓸ);
}

// Info logs at [LevelInfo].
[GoRecv] public static void Info(this ref Logger l, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(context.Background(), LevelInfo, msg, args.ꓸꓸꓸ);
}

// InfoContext logs at [LevelInfo] with the given context.
[GoRecv] public static void InfoContext(this ref Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(ctx, LevelInfo, msg, args.ꓸꓸꓸ);
}

// Warn logs at [LevelWarn].
[GoRecv] public static void Warn(this ref Logger l, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(context.Background(), LevelWarn, msg, args.ꓸꓸꓸ);
}

// WarnContext logs at [LevelWarn] with the given context.
[GoRecv] public static void WarnContext(this ref Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(ctx, LevelWarn, msg, args.ꓸꓸꓸ);
}

// Error logs at [LevelError].
[GoRecv] public static void Error(this ref Logger l, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(context.Background(), LevelError, msg, args.ꓸꓸꓸ);
}

// ErrorContext logs at [LevelError] with the given context.
[GoRecv] public static void ErrorContext(this ref Logger l, context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    l.log(ctx, LevelError, msg, args.ꓸꓸꓸ);
}

// log is the low-level logging method for methods that take ...any.
// It must always be called directly by an exported logging method
// or function, because it uses a fixed call depth to obtain the pc.
[GoRecv] internal static void log(this ref Logger l, context.Context ctx, ΔLevel level, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    if (!l.Enabled(ctx, level)) {
        return;
    }
    uintptr pc = default!;
    if (!@internal.IgnorePC) {
        array<uintptr> pcs = new(1);
        // skip [runtime.Callers, this function, this function's caller]
        runtime.Callers(3, pcs[..]);
        pc = pcs[0];
    }
    var r = NewRecord(time.Now(), level, msg, pc);
    r.Add(args.ꓸꓸꓸ);
    if (ctx == default!) {
        ctx = context.Background();
    }
    _ = l.Handler().Handle(ctx, r);
}

// logAttrs is like [Logger.log], but for methods that take ...Attr.
[GoRecv] internal static void logAttrs(this ref Logger l, context.Context ctx, ΔLevel level, @string msg, params ꓸꓸꓸAttr attrsʗp) {
    var attrs = attrsʗp.slice();

    if (!l.Enabled(ctx, level)) {
        return;
    }
    uintptr pc = default!;
    if (!@internal.IgnorePC) {
        array<uintptr> pcs = new(1);
        // skip [runtime.Callers, this function, this function's caller]
        runtime.Callers(3, pcs[..]);
        pc = pcs[0];
    }
    var r = NewRecord(time.Now(), level, msg, pc);
    r.AddAttrs(attrs.ꓸꓸꓸ);
    if (ctx == default!) {
        ctx = context.Background();
    }
    _ = l.Handler().Handle(ctx, r);
}

// Debug calls [Logger.Debug] on the default logger.
public static void Debug(@string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(context.Background(), LevelDebug, msg, args.ꓸꓸꓸ);
}

// DebugContext calls [Logger.DebugContext] on the default logger.
public static void DebugContext(context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(ctx, LevelDebug, msg, args.ꓸꓸꓸ);
}

// Info calls [Logger.Info] on the default logger.
public static void Info(@string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(context.Background(), LevelInfo, msg, args.ꓸꓸꓸ);
}

// InfoContext calls [Logger.InfoContext] on the default logger.
public static void InfoContext(context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(ctx, LevelInfo, msg, args.ꓸꓸꓸ);
}

// Warn calls [Logger.Warn] on the default logger.
public static void Warn(@string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(context.Background(), LevelWarn, msg, args.ꓸꓸꓸ);
}

// WarnContext calls [Logger.WarnContext] on the default logger.
public static void WarnContext(context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(ctx, LevelWarn, msg, args.ꓸꓸꓸ);
}

// Error calls [Logger.Error] on the default logger.
public static void Error(@string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(context.Background(), LevelError, msg, args.ꓸꓸꓸ);
}

// ErrorContext calls [Logger.ErrorContext] on the default logger.
public static void ErrorContext(context.Context ctx, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(ctx, LevelError, msg, args.ꓸꓸꓸ);
}

// Log calls [Logger.Log] on the default logger.
public static void Log(context.Context ctx, ΔLevel level, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Default().log(ctx, level, msg, args.ꓸꓸꓸ);
}

// LogAttrs calls [Logger.LogAttrs] on the default logger.
public static void LogAttrs(context.Context ctx, ΔLevel level, @string msg, params ꓸꓸꓸAttr attrsʗp) {
    var attrs = attrsʗp.slice();

    Default().logAttrs(ctx, level, msg, attrs.ꓸꓸꓸ);
}

} // end slog_package
