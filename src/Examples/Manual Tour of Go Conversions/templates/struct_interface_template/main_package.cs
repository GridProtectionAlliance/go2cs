/*
Go source
*/

using go;
using static go.builtin;

static partial class main_package
{
    partial interface I
    {
        void M();
    }

    partial struct T
    {
        public @string S;
    }

    static void Main()
    {
    }
}