// package main -- go2cs converted at 2018 June 07 01:54:02 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\SortArrayType.go

using fmt = go.fmt_package;
using sort = go.sort_package;

using static go.BuiltInFunctions;

namespace go
{
    public static unsafe partial class main_package
    {
        public partial struct Person
        {
            public string Name;
            public long Age;
            public single ShoeSize;
        }

        public partial struct PeopleByShoeSize // Slice<Person>
        {
        }
        returnlen(p)
        p[i],p[j]=p[j],p[i]
        return(p[i].ShoeSize<p[j].ShoeSize)

        private static bool Main() => func((defer, panic, recover) =>
        {
            people:=[]Person{{Name:"Person1",Age:25,ShoeSize:8,},{Name:"Person2",Age:21,ShoeSize:4,},{Name:"Person3",Age:15,ShoeSize:9,},{Name:"Person4",Age:45,ShoeSize:15,},{Name:"Person5",Age:25,ShoeSize:8.5,}}fmt.Println(people)sort.Sort(PeopleByShoeSize(people))fmt.Println(people)
        });
    }
}
