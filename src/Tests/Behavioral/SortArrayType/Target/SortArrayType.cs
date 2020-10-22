using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.builtin;

using PeopleByShoeSize = go.slice<go.main_package.Person>;

namespace go
{
    public static partial class main_package
    {
        public partial struct Person
        {
            public @string Name;
            public nint Age;
            public float ShoeSize;
        }

        public static nint Len(this PeopleByShoeSize p)
        {
            return len(p);
        }

        public static void Swap(this PeopleByShoeSize p, nint i, nint j)
        {
            Person tmp = p[i];
            p[i] = p[j];
            p[j] = tmp;
        }

        public static bool Less(this PeopleByShoeSize p, nint i, nint j)
        {
            return (p[i].ShoeSize < p[j].ShoeSize);
        }

        private static void Main()
        {
            var people = new slice<Person>(new[] {
                new Person(
                    Name: "Person1",
                    Age: 25,
                    ShoeSize: 8F
                ),
                new Person(
                    Name: "Person2",
                    Age: 21,
                    ShoeSize: 4F
                ),
                new Person(
                    Name: "Person3",
                    Age: 15,
                    ShoeSize: 9F
                ),
                new Person(
                    Name: "Person4",
                    Age: 45,
                    ShoeSize: 15F
                ),
                new Person(
                    Name: "Person5",
                    Age: 25,
                    ShoeSize: 8.5F
                )
            });

            fmt.Println(people);
            sort.Sort(sort.Interface.As((PeopleByShoeSize)people));
            fmt.Println(people);
        }
    }
}
