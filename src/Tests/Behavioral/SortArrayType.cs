// package main -- go2cs converted at 2018 August 08 21:28:02 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\SortArrayType.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct Person
        {
            public @string Name;
            public @int Age;
            public float32 ShoeSize;
        }

        public partial struct PeopleByShoeSize // : slice<Person>
        {
        }

        public static @int Len(this PeopleByShoeSize p)
        {
            return len(p);
        }

        public static void Swap(this PeopleByShoeSize p, @int i, @int j)
        {
            p[i] = p[j];
            p[j] = p[i];
        }

        public static @bool Less(this PeopleByShoeSize p, @int i, @int j)
        {
            return (p[i].ShoeSize < p[j].ShoeSize);
        }

        private static void Main()
        {
            var people = []Person{{Name:"Person1",Age:25,ShoeSize:8,},{Name:"Person2",Age:21,ShoeSize:4,},{Name:"Person3",Age:15,ShoeSize:9,},{Name:"Person4",Age:45,ShoeSize:15,},{Name:"Person5",Age:25,ShoeSize:8.5,}};

            fmt.Println(people);
            sort.Sort(PeopleByShoeSize(people));
            fmt.Println(people);
        }
    }
}
