namespace LucenenetStudy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var search = new MySearch(Path.Combine(Environment.CurrentDirectory, "index"));
            
            search.Index();
            Console.WriteLine($"세팅 종료 \n");

            search.Search("롱 소");
            search.Search("스퉤프");
            search.Search("리공");
            search.Search("리 공");
            search.Search("물리 공격"); 
            search.Search("법 공");
            
            search.Search("힘 증가", "물리 공격력 증가");
            search.Search("힘 증가", "마법 공격력 증가");
            
            search.Search("민첩 증가", "물리 공격력 증가");
            search.Search("민첩 증가", "마법 공격력 증가");
            
            search.Search("지능 증가", "물리 공격력 증가");
            search.Search("지능 증가", "마법 공격력 증가");
            
            search.Search("정신력 증가", "물리 공격력 증가");
            search.Search("정신력 증가", "마법 공격력 증가");
        }
    }
}