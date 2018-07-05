// package main -- go2cs converted at 2018 July 05 21:01:34 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\SortArrayType.go

using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial struct Person
        {
            public GoString Name;
            public long Age;
            public single ShoeSize;
        }


        public partial struct PeopleByShoeSize // : Slice<Person>
        {
        }

        public static long Len(this PeopleByShoeSize p)
        {
            {
                return ;
            }        }

        public static void Swap(this PeopleByShoeSize p, long i, long j)
        {
            {
            }        }

        public static bool Less(this PeopleByShoeSize p, long i, long j)
        {
            {
                return ;
            }        }

        private static void Main()
        {
            fmt.Println(people);
            sort.Sort(PeopleByShoeSize(people));
            fmt.Println(people);
        }
    }
}
