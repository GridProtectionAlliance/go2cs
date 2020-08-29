// package main -- go2cs converted at 2020 August 29 09:24:39 UTC
// Original source: C:\Go\src\cmd\compile\internal\ssa\testdata\i22558.go
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private partial struct big
        {
            public array<sbyte> pile;
        }

        private partial struct thing
        {
            public @string name;
            public ptr<thing> next;
            public ptr<thing> self;
            public slice<big> stuff;
        }

        private static void test(ref thing t, ref thing u)
        {
            if (t.next != null)
            {
                return;
            }
            fmt.Fprintf(os.Stderr, "%s\n", t.name);
            u.self = u;
            t.self = t;
            t.next = u;
            foreach (var (_, p) in t.stuff)
            {
                if (isFoo(t, p))
                {
                    return;
                }
            }
        }

        //go:noinline
        private static bool isFoo(ref thing t, big b)
        {
            return true;
        }

        private static void Main()
        {
            thing t = ref new thing(name:"t",self:nil,next:nil,stuff:make([]big,1));
            thing u = new thing(name:"u",self:t,next:t,stuff:make([]big,1));
            test(t, ref u);
        }
    }
}
