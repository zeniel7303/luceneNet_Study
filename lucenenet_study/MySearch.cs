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

public class MySearch
{
    private const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;

    private readonly IndexWriter _writer;
    private readonly SearcherManager _searchManager;
    private readonly QueryParser _queryParser;
    
    private static readonly string[] fields = new[] { "name", "option1", "value1", "option2", "value2"};

    public MySearch(string indexPath)
    {
        _writer = new IndexWriter(FSDirectory.Open(indexPath),
            new IndexWriterConfig(luceneVersion, new StandardAnalyzer(luceneVersion))
            {
                OpenMode = OpenMode.CREATE
            });
        _searchManager = new SearcherManager(_writer, true, null);
        _queryParser = new MultiFieldQueryParser(luceneVersion, fields, new StandardAnalyzer(luceneVersion));
    }
    
    public void Index(int num)
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
        for (var i = 0; i < num; i++)
        {
            var index = random.Next(items.Length);
            var item = items[index];
            counts[index]++;
            AddDocument($"{i}", item[0], item[1], int.Parse(item[2]), item[3], int.Parse(item[4]));
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

    private void AddDocument(string id, string name, string option1, int value1, string option2, int value2)
    {
        var document = new Document
        {
            new StringField("id", id, Field.Store.YES),
            new StringField("name", name.Replace(" ", ""), Field.Store.YES),
            new StringField("option1", option1.Replace(" ", ""), Field.Store.YES),
            new Int64Field("value1", value1, Field.Store.YES),
            new StringField("option2", option2.Replace(" ", ""), Field.Store.YES),
            new Int64Field("value2", value2, Field.Store.YES)
        };
        _writer.AddDocument(document);
    }

    public void Search(string query)
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

            // fields 배열의 모든 필드에 대해 WildcardQuery 추가
            foreach (var field in fields)
            {
                booleanQuery.Add(new WildcardQuery(new Term(field, $"*{queryWithoutWhiteSpace}*")), Occur.SHOULD);
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
            
            stopwatch.Stop(); // 수행 시간 측정 종료
            Console.WriteLine($"Search execution time: {stopwatch.ElapsedMilliseconds} ms \n");
        }
    }
    
    public void Search(string query1, string query2)
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

            // fields 배열의 모든 필드에 대해 WildcardQuery 추가
            foreach (var field in fields)
            {
                booleanQuery.Add(new WildcardQuery(new Term("option1", $"*{query1WithoutWhiteSpace}*")), Occur.MUST);
                booleanQuery.Add(new WildcardQuery(new Term("option2", $"*{query2WithoutWhiteSpace}*")), Occur.MUST);
            }

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

    public void SearchByRange(int optionIndex, int minValue, int maxValue)
    {
        Console.WriteLine($"범위 검색: option{optionIndex} {minValue}~{maxValue}");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        _searchManager.MaybeRefreshBlocking();

        var searcher = _searchManager.Acquire();
        try
        {
            var booleanQuery = new BooleanQuery();
            
            // 옵션에 따라 검색할 필드 선택
            var valueField = $"value{optionIndex}";

            // 범위 쿼리 생성
            var rangeQuery = NumericRangeQuery.NewInt64Range(
                valueField,
                minValue,
                maxValue,
                true,  // minInclusive
                true   // maxInclusive
            );
            
            booleanQuery.Add(rangeQuery, Occur.MUST);

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

    public void SearchByOptionAndRange(int optionIndex, string optionName, int minValue, int maxValue)
    {
        Console.WriteLine($"옵션 범위 검색: {optionName} {minValue}~{maxValue}");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        _searchManager.MaybeRefreshBlocking();

        var searcher = _searchManager.Acquire();
        try
        {
            var booleanQuery = new BooleanQuery();
            
            // 옵션 필드와 값 필드 선택
            var optionField = $"option{optionIndex}";
            var valueField = $"value{optionIndex}";

            // 옵션 이름 쿼리
            var optionQuery = new WildcardQuery(
                new Term(optionField, $"*{optionName.Replace(" ", "")}*")
            );
            
            // 범위 쿼리
            var rangeQuery = NumericRangeQuery.NewInt64Range(
                valueField,
                minValue,
                maxValue,
                true,
                true
            );
            
            booleanQuery.Add(optionQuery, Occur.MUST);
            booleanQuery.Add(rangeQuery, Occur.MUST);

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

            foreach (var (optionName, minValue, maxValue) in options)
            {
                // 옵션 필드와 값 필드 선택
                var optionField = $"option{options.IndexOf((optionName, minValue, maxValue)) + 1}";
                var valueField = $"value{options.IndexOf((optionName, minValue, maxValue)) + 1}";

                // 옵션 이름 쿼리
                var optionQuery = new WildcardQuery(
                    new Term(optionField, $"*{optionName.Replace(" ", "")}*")
                );

                // 범위 쿼리
                var rangeQuery = NumericRangeQuery.NewInt64Range(
                    valueField,
                    minValue,
                    maxValue,
                    true,
                    true
                );

                booleanQuery.Add(optionQuery, Occur.MUST);
                booleanQuery.Add(rangeQuery, Occur.MUST);
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