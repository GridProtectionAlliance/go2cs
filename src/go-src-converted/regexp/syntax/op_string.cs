// Code generated by "stringer -type Op -trimprefix Op"; DO NOT EDIT.

// package syntax -- go2cs converted at 2020 October 09 04:58:26 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Go\src\regexp\syntax\op_string.go
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace regexp
{
    public static partial class syntax_package
    {
        private static readonly @string _Op_name_0 = (@string)"NoMatchEmptyMatchLiteralCharClassAnyCharNotNLAnyCharBeginLineEndLineBeginTextEndTextWordBoundaryNoWordBoundaryCaptureStarPlusQuestRepeatConcatAlternate";
        private static readonly @string _Op_name_1 = (@string)"opPseudo";


        private static array<byte> _Op_index_0 = new array<byte>(new byte[] { 0, 7, 17, 24, 33, 45, 52, 61, 68, 77, 84, 96, 110, 117, 121, 125, 130, 136, 142, 151 });

        public static @string String(this Op i)
        {

            if (1L <= i && i <= 19L) 
                i -= 1L;
                return _Op_name_0[_Op_index_0[i].._Op_index_0[i + 1L]];
            else if (i == 128L) 
                return _Op_name_1;
            else 
                return "Op(" + strconv.FormatInt(int64(i), 10L) + ")";
            
        }
    }
}}
