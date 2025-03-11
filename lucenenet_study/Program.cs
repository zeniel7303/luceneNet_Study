namespace LucenenetStudy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            /*var search = new MySearch(Path.Combine(Environment.CurrentDirectory, "index"));

            search.Index();

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

            // 2번째 옵션이 "물리 공격력 증가" 이고 수치가 5이상 10이하인 아이템 검색
            search.SearchByOptionAndRange(2, "물리 공격력 증가", 5, 10);

            search.SearchByOptions(new List<(string optionName, int minValue, int maxValue)>
            {
                ("힘 증가", 10, 20),        // 첫 번째 옵션: 힘 증가, 범위 10~20
                ("물리 공격력 증가", 5, 15)  // 두 번째 옵션: 물리 공격력 증가, 범위 5~15
            });*/

            TempFunc1();
            
            Console.WriteLine(" ********************************************************************************** ");

            TempFunc2();
            
            Console.WriteLine(" ********************************************************************************** ");

            TempFunc3();
        }
        
        private static void TempFunc1()
        {
            for (var i = 0; i < 5; i++)
            {
                var search = new MySearch(Path.Combine(Environment.CurrentDirectory, $"index0{i}"));
                search.Index(1_000_000);
            }
            
#if FALSE
            var search = new MySearch(Path.Combine(Environment.CurrentDirectory, "index1"));
            search.Index(1_000_000);

            search.Search("롱 소");
            search.Search("물리 공격");
            search.Search("힘 증가", "물리 공격력 증가");
            
            // 2번째 옵션이 "물리 공격력 증가" 이고 수치가 5이상 10이하인 아이템 검색
            search.SearchByOptionAndRange(2, "물리 공격력 증가", 5, 10);
            
            search.SearchByOptions(new List<(string optionName, int minValue, int maxValue)>
            {
                ("힘 증가", 10, 20),        // 첫 번째 옵션: 힘 증가, 범위 10~20
                ("물리 공격력 증가", 5, 15)  // 두 번째 옵션: 물리 공격력 증가, 범위 5~15
            });

#endif
        }

        private static void TempFunc2()
        {
            for (var i = 0; i < 5; i++)
            {
                var searchV2 = new MySearchV2(Path.Combine(Environment.CurrentDirectory, $"index1{i}"));
                searchV2.Index(1_000_000);
            }
            
#if false
            searchV2.SearchWithName("롱 소");
            searchV2.SearchWithOption("물리 공격");
            searchV2.SearchWithOptions("힘 증가", "물리 공격력 증가");
            
            // 2번째 옵션이 "물리 공격력 증가" 이고 수치가 5이상 10이하인 아이템 검색
            searchV2.SearchByOptionAndRange(2, "물리 공격력 증가", 5, 10);
            
            searchV2.SearchByOptions(new List<(string optionName, int minValue, int maxValue)>
            {
                ("힘 증가", 10, 20),        // 첫 번째 옵션: 힘 증가, 범위 10~20
                ("물리 공격력 증가", 5, 15)  // 두 번째 옵션: 물리 공격력 증가, 범위 5~15
            });
#endif
        }

        private static void TempFunc3()
        {
            for (var i = 0; i < 5; i++)
            {
                var search = new MySearchV3(Path.Combine(Environment.CurrentDirectory, $"index2{i}"));
                search.Index(1_000_000);
            }
        }
    }
}