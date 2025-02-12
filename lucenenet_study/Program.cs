namespace LucenenetStudy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var search = new MySearch(Path.Combine(Environment.CurrentDirectory, "index"));
            
            search.Index();
            Console.WriteLine($"세팅 종료 \n");

            /*search.Search("롱 소");
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
            search.Search("정신력 증가", "마법 공격력 증가");*/

            // 옵션1의 수치가 5이상 10이하인 아이템 검색
            search.SearchByRange(1, 5, 10);

            // 2번째 옵션이 "물리 공격력 증가" 이고 수치가 5이상 10이하인 아이템 검색
            search.SearchByOptionAndRange(2, "물리 공격력 증가", 5, 10);

            search.SearchByOptions(new List<(string optionName, int minValue, int maxValue)>
            {
                ("힘 증가", 10, 20),        // 첫 번째 옵션: 힘 증가, 범위 10~20
                ("물리 공격력 증가", 5, 15)  // 두 번째 옵션: 물리 공격력 증가, 범위 5~15
            });
        }
    }
}