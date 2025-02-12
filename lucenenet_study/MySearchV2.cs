using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using LuceneDirectory = Lucene.Net.Store.Directory;

namespace LucenenetStudy;

public class MySearchV2
{
    private const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;

    private readonly IndexWriter _writer;
    private readonly SearcherManager _searchManager;
    private readonly QueryParser _queryParser;
    
    private static readonly string[] fields = new[] { "name", "option1", "value1", "option2", "value2"};

    public MySearchV2(string indexPath)
    {
        _writer = new IndexWriter(FSDirectory.Open(indexPath),
            new IndexWriterConfig(luceneVersion, new StandardAnalyzer(luceneVersion))
            {
                OpenMode = OpenMode.CREATE
            });
        _searchManager = new SearcherManager(_writer, true, null);
        _queryParser = new MultiFieldQueryParser(luceneVersion, fields, new StandardAnalyzer(luceneVersion));
    }
    
    public void Index()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var items = new[]
        {
            new[] { "롱 소드", "힘 증가", "10", "물리 공격력 증가", "12" },
            new[] { "롱 보우", "민첩 증가", "8", "물리 공격력 증가", "7" }, 
            new[] { "스태프", "지능 증가", "12", "마법 공격력 증가", "6" },
            new[] { "샙터", "정신력 증가", "9", "마법 공격력 증가", "11" }
        };

        var counts = new int[items.Length];
        var random = new Random();
        for (var i = 0; i < 100; i++)
        {
            var index = random.Next(items.Length);
            var item = items[index];
            counts[index]++;
            AddDocument(item[0], item[1], int.Parse(item[2]), item[3], int.Parse(item[4]));
        }

        for (var i = 0; i < items.Length; i++)
        {
            Console.WriteLine($"{items[i][0]}: {counts[i]}개");
        }

        _writer.Commit();
        
        Console.WriteLine($"세팅 종료 \n");
        
        stopwatch.Stop(); // 수행 시간 측정 종료
        Console.WriteLine($"Search execution time: {stopwatch.ElapsedMilliseconds} ms \n");
    }

    private void AddDocument(string name, string option1, int value1, string option2, int value2)
    {
        var document = new Document
        {
            new StringField("name", name.Replace(" ", ""), Field.Store.YES),
            new StringField("option1_value1", $"{option1.Replace(" ", "")}:{value1}", Field.Store.YES),
            new StringField("option2_value2", $"{option2.Replace(" ", "")}:{value2}", Field.Store.YES)
        };
        _writer.AddDocument(document);
    }
    
    public void SearchWithName(string query)
    {
        Console.WriteLine($"검색어: {query}");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
        _searchManager.MaybeRefreshBlocking();

        var searcher = _searchManager.Acquire();
        try
        {
            var booleanQuery = new BooleanQuery();
        
            // 입력된 쿼리에서 띄어쓰기 제거
            var queryWithoutWhiteSpace = query.Replace(" ", "");

            // name 필드에 대해 WildcardQuery 추가
            booleanQuery.Add(new WildcardQuery(new Term("name", $"*{queryWithoutWhiteSpace}*")), Occur.SHOULD);

            var topDocs = searcher.Search(booleanQuery, n: 10);
        
            Console.WriteLine($"검색 결과 수: {topDocs.TotalHits}");
            Console.WriteLine("===============================================================================");

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                PrintDocumentFields(searcher, scoreDoc);
            }
        }
        finally
        {
            _searchManager.Release(searcher);
            searcher = null;
        
            stopwatch.Stop(); // 수행 시간 측정 종료
            Console.WriteLine($"Search execution time: {stopwatch.ElapsedMilliseconds} ms \n");
        }
    }

    public void SearchWithOption(string query)
    {
        Console.WriteLine($"검색어: {query}");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
        _searchManager.MaybeRefreshBlocking();

        var searcher = _searchManager.Acquire();
        try
        {
            var booleanQuery = new BooleanQuery();
        
            // 입력된 쿼리에서 띄어쓰기 제거
            var queryWithoutWhiteSpace = query.Replace(" ", "");

            // options 필드에 대해 WildcardQuery 추가
            booleanQuery.Add(new WildcardQuery(new Term("option1_value1", $"*{queryWithoutWhiteSpace}*")), Occur.SHOULD);
            booleanQuery.Add(new WildcardQuery(new Term("option2_value2", $"*{queryWithoutWhiteSpace}*")), Occur.SHOULD); // 수정된 부분

            var topDocs = searcher.Search(booleanQuery, n: 10);
        
            Console.WriteLine($"검색 결과 수: {topDocs.TotalHits}");
            Console.WriteLine("===============================================================================");

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                PrintDocumentFields(searcher, scoreDoc);
            }
        }
        finally
        {
            _searchManager.Release(searcher);
            searcher = null;
        
            stopwatch.Stop(); // 수행 시간 측정 종료
            Console.WriteLine($"Search execution time: {stopwatch.ElapsedMilliseconds} ms \n");
        }
    }
    
    public void SearchWithOptions(string query1, string query2)
    {
        Console.WriteLine($"검색어: {query1}, {query2}");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
        _searchManager.MaybeRefreshBlocking();

        var searcher = _searchManager.Acquire();
        try
        {
            var booleanQuery = new BooleanQuery();
        
            // 입력된 쿼리에서 띄어쓰기 제거
            var query1WithoutWhiteSpace = query1.Replace(" ", "");
            var query2WithoutWhiteSpace = query2.Replace(" ", "");

            // options 필드에 대해 WildcardQuery 추가
            booleanQuery.Add(new WildcardQuery(new Term("option1_value1", $"*{query1WithoutWhiteSpace}*")), Occur.MUST);
            booleanQuery.Add(new WildcardQuery(new Term("option2_value2", $"*{query2WithoutWhiteSpace}*")), Occur.MUST);

            var topDocs = searcher.Search(booleanQuery, n: 10);
        
            Console.WriteLine($"Matching results: {topDocs.TotalHits}");
            Console.WriteLine("===============================================================================");

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                PrintDocumentFields(searcher, scoreDoc);
            }
        }
        finally
        {
            _searchManager.Release(searcher);
            searcher = null;
        
            stopwatch.Stop(); // 수행 시간 측정 종료
            Console.WriteLine($"Search execution time: {stopwatch.ElapsedMilliseconds} ms \n");
        }
    }

    public void SearchByOptionAndRange(int n, string optionName, int minValue, int maxValue) // n번째 옵션 추가
    {
        Console.WriteLine($"{n}번 옵션 범위 검색: {optionName} {minValue}~{maxValue}");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
        _searchManager.MaybeRefreshBlocking();

        var searcher = _searchManager.Acquire();
        try
        {
            var booleanQuery = new BooleanQuery();
            
            // 너무 무식한 방법이지만 일단 다르게 생각나는 방법이 없음
            // minValue와 maxValue 사이의 값을 찾기 위한 쿼리 추가
            for (var value = minValue; value <= maxValue; value++)
            {
                booleanQuery.Add(new WildcardQuery(new Term($"option{n}_value{n}", $"*{optionName.Replace(" ", "")}:{value}*")), Occur.SHOULD);
            }
            
            var topDocs = searcher.Search(booleanQuery, n: 10);
        
            Console.WriteLine($"검색 결과 수: {topDocs.TotalHits}");
            Console.WriteLine("===============================================================================");

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                PrintDocumentFields(searcher, scoreDoc);
            }
        }
        finally
        {
            _searchManager.Release(searcher);
            searcher = null;
        
            stopwatch.Stop();
            Console.WriteLine($"검색 소요 시간: {stopwatch.ElapsedMilliseconds} ms \n");
        }
    }

    public void SearchByOptions(List<(string optionName, int minValue, int maxValue)> options)
    {
        Console.WriteLine("옵션 범위 검색 시작...");
        Console.WriteLine($"검색할 옵션 개수: {options.Count}");
        foreach (var (optionName, minValue, maxValue) in options)
        {
            Console.WriteLine($"- 옵션: {optionName}, 범위: {minValue}~{maxValue}");
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _searchManager.MaybeRefreshBlocking();

        var searcher = _searchManager.Acquire();
        try
        {
            var booleanQuery = new BooleanQuery();

            var num = 1;
            foreach (var (optionName, minValue, maxValue) in options)
            {
                // 옵션 이름 쿼리
                var optionQuery = new WildcardQuery(
                    new Term($"option{num}_value{num}", $"*{optionName.Replace(" ", "")}*") // 첫 번째 옵션 필드 사용
                );

                // 범위 쿼리 생성
                var rangeQuery = new BooleanQuery();
                for (var value = minValue; value <= maxValue; value++)
                {
                    rangeQuery.Add(new WildcardQuery(new Term($"option{num}_value{num}", $"*{optionName.Replace(" ", "")}:{value}*")), Occur.SHOULD);
                }

                booleanQuery.Add(optionQuery, Occur.MUST);
                booleanQuery.Add(rangeQuery, Occur.MUST);

                num++;
            }

            var topDocs = searcher.Search(booleanQuery, n: 10);

            Console.WriteLine($"검색 결과 수: {topDocs.TotalHits}");
            Console.WriteLine("===============================================================================");

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                PrintDocumentFields(searcher, scoreDoc);
            }
        }
        finally
        {
            _searchManager.Release(searcher);
            searcher = null;

            stopwatch.Stop();
            Console.WriteLine($"검색 소요 시간: {stopwatch.ElapsedMilliseconds} ms \n");
        }
    }

    private static void PrintDocumentFields(IndexSearcher searcher, ScoreDoc scoreDoc)
    {
        /*var document = searcher.Doc(scoreDoc.Doc);
        Console.WriteLine(document.GetField("name")?.GetStringValue());
        Console.WriteLine($"{document.GetField("option1")?.GetStringValue()} : {document.GetField("value1")?.GetStringValue()}");
        Console.WriteLine($"{document.GetField("option2")?.GetStringValue()} : {document.GetField("value2")?.GetStringValue()}");
        Console.WriteLine("===============================================================================");*/
    }
}