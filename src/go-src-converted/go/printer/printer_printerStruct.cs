//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:23:36 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using tabwriter = go.text.tabwriter_package;
using unicode = go.unicode_package;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class printer_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        [PromotedStruct(typeof(Config))]
        [PromotedStruct(typeof(commentInfo))]
        private partial struct printer
        {
            // Config structure promotion - sourced from value copy
            private readonly ptr<Config> m_ConfigRef;

            private ref Config Config_val => ref m_ConfigRef.Value;

            public ref Mode Mode => ref m_ConfigRef.Value.Mode;

            public ref long Tabwidth => ref m_ConfigRef.Value.Tabwidth;

            public ref long Indent => ref m_ConfigRef.Value.Indent;

            // commentInfo structure promotion - sourced from value copy
            private readonly ptr<commentInfo> m_commentInfoRef;

            private ref commentInfo commentInfo_val => ref m_commentInfoRef.Value;

            public ref long cindex => ref m_commentInfoRef.Value.cindex;

            public ref ptr<ast.CommentGroup> comment => ref m_commentInfoRef.Value.comment;

            public ref long commentOffset => ref m_commentInfoRef.Value.commentOffset;

            public ref bool commentNewline => ref m_commentInfoRef.Value.commentNewline;

            // Constructors
            public printer(NilType _)
            {
                this.m_ConfigRef = new ptr<Config>(new Config(nil));
                this.fset = default;
                this.output = default;
                this.indent = default;
                this.level = default;
                this.mode = default;
                this.endAlignment = default;
                this.impliedSemi = default;
                this.lastTok = default;
                this.prevOpen = default;
                this.wsbuf = default;
                this.pos = default;
                this.@out = default;
                this.last = default;
                this.linePtr = default;
                this.comments = default;
                this.useNodeComments = default;
                this.m_commentInfoRef = new ptr<commentInfo>(new commentInfo(nil));
                this.nodeSizes = default;
                this.cachedPos = default;
                this.cachedLine = default;
            }

            public printer(Config Config = default, ref ptr<token.FileSet> fset = default, slice<byte> output = default, long indent = default, long level = default, pmode mode = default, bool endAlignment = default, bool impliedSemi = default, token.Token lastTok = default, token.Token prevOpen = default, slice<whiteSpace> wsbuf = default, token.Position pos = default, token.Position @out = default, token.Position last = default, ref ptr<long> linePtr = default, slice<ptr<ast.CommentGroup>> comments = default, bool useNodeComments = default, commentInfo commentInfo = default, map<ast.Node, long> nodeSizes = default, token.Pos cachedPos = default, long cachedLine = default)
            {
                this.m_ConfigRef = new ptr<Config>(Config);
                this.fset = fset;
                this.output = output;
                this.indent = indent;
                this.level = level;
                this.mode = mode;
                this.endAlignment = endAlignment;
                this.impliedSemi = impliedSemi;
                this.lastTok = lastTok;
                this.prevOpen = prevOpen;
                this.wsbuf = wsbuf;
                this.pos = pos;
                this.@out = @out;
                this.last = last;
                this.linePtr = linePtr;
                this.comments = comments;
                this.useNodeComments = useNodeComments;
                this.m_commentInfoRef = new ptr<commentInfo>(commentInfo);
                this.nodeSizes = nodeSizes;
                this.cachedPos = cachedPos;
                this.cachedLine = cachedLine;
            }

            // Enable comparisons between nil and printer struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(printer value, NilType nil) => value.Equals(default(printer));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(printer value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, printer value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, printer value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator printer(NilType nil) => default(printer);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static printer printer_cast(dynamic value)
        {
            return new printer(value.Config, ref value.fset, value.output, value.indent, value.level, value.mode, value.endAlignment, value.impliedSemi, value.lastTok, value.prevOpen, value.wsbuf, value.pos, value.@out, value.last, ref value.linePtr, value.comments, value.useNodeComments, value.commentInfo, value.nodeSizes, value.cachedPos, value.cachedLine);
        }
    }
}}