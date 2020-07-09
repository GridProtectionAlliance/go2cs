public (string name, int age) GetStudentInfo(string id)
{
    // Search by ID and find the student.
    return (name: "Annie", age: 25);
}
 
public void Test()
{
    (string name, int age) info = GetStudentInfo("100-000-1000"); 
    Console.WriteLine($"Name: {info.name}, Age: {info.age}");
}