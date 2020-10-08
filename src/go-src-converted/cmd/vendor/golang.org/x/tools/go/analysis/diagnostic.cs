// package analysis -- go2cs converted at 2020 October 08 04:54:17 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis" ==> using analysis = go.cmd.vendor.golang.org.x.tools.go.analysis_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\diagnostic.go
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class analysis_package
    {
        // A Diagnostic is a message associated with a source location or range.
        //
        // An Analyzer may return a variety of diagnostics; the optional Category,
        // which should be a constant, may be used to classify them.
        // It is primarily intended to make it easy to look up documentation.
        //
        // If End is provided, the diagnostic is specified to apply to the range between
        // Pos and End.
        public partial struct Diagnostic
        {
            public token.Pos Pos;
            public token.Pos End; // optional
            public @string Category; // optional
            public @string Message; // SuggestedFixes contains suggested fixes for a diagnostic which can be used to perform
// edits to a file that address the diagnostic.
// TODO(matloob): Should multiple SuggestedFixes be allowed for a diagnostic?
// Diagnostics should not contain SuggestedFixes that overlap.
// Experimental: This API is experimental and may change in the future.
            public slice<SuggestedFix> SuggestedFixes; // optional

// Experimental: This API is experimental and may change in the future.
            public slice<RelatedInformation> Related; // optional
        }

        // RelatedInformation contains information related to a diagnostic.
        // For example, a diagnostic that flags duplicated declarations of a
        // variable may include one RelatedInformation per existing
        // declaration.
        public partial struct RelatedInformation
        {
            public token.Pos Pos;
            public token.Pos End;
            public @string Message;
        }

        // A SuggestedFix is a code change associated with a Diagnostic that a user can choose
        // to apply to their code. Usually the SuggestedFix is meant to fix the issue flagged
        // by the diagnostic.
        // TextEdits for a SuggestedFix should not overlap. TextEdits for a SuggestedFix
        // should not contain edits for other packages.
        // Experimental: This API is experimental and may change in the future.
        public partial struct SuggestedFix
        {
            public @string Message;
            public slice<TextEdit> TextEdits;
        }

        // A TextEdit represents the replacement of the code between Pos and End with the new text.
        // Each TextEdit should apply to a single file. End should not be earlier in the file than Pos.
        // Experimental: This API is experimental and may change in the future.
        public partial struct TextEdit
        {
            public token.Pos Pos;
            public token.Pos End;
            public slice<byte> NewText;
        }
    }
}}}}}}}
