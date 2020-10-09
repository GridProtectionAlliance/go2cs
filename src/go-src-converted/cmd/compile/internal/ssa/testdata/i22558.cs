// package main -- go2cs converted at 2020 October 09 05:39:57 UTC
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

        private static void test(ptr<thing> _addr_t, ptr<thing> _addr_u)
        {
            ref thing t = ref _addr_t.val;
            ref thing u = ref _addr_u.val;

            if (t.next != null)
            {
                return ;
            }

            fmt.Fprintf(os.Stderr, "%s\n", t.name);
            u.self = u;
            t.self = t;
            t.next = u;
            foreach (var (_, p) in t.stuff)
            {
                if (isFoo(_addr_t, p))
                {
                    return ;
                }

            }

        }

        //go:noinline
        private static bool isFoo(ptr<thing> _addr_t, big b)
        {
            ref thing t = ref _addr_t.val;

            return true;
        }

        private static void Main()
        {
            growstack(); // Use stack early to prevent growth during test, which confuses gdb
            ptr<thing> t = addr(new thing(name:"t",self:nil,next:nil,stuff:make([]big,1)));
            ref thing u = ref heap(new thing(name:"u",self:t,next:t,stuff:make([]big,1)), out ptr<thing> _addr_u);
            test(t, _addr_u);

        }

        private static @string snk = default;

        //go:noinline
        private static void growstack()
        {
            snk = fmt.Sprintf("%#v,%#v,%#v", 1L, true, "cat");
        }
    }
}
