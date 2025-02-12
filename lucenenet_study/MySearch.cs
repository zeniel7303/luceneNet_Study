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
    
    private static readonly string[] fields = new[] { "name", "option1", "option2"};

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
    
    public void Index()
    {
        var items = new[]
        {
            new[] { "롱 소드", "힘 증가", "물리 공격력 증가" },
            new[] { "롱 보우", "민첩 증가", "물리 공격력 증가" },
            new[] { "스태프", "지능 증가", "마법 공격력 증가" },
            new[] { "샙터", "정신력 증가", "마법 공격력 증가" }
        };

        var counts = new int[items.Length];
        var random = new Random();
        for (var i = 0; i < 100000; i++)
        {
            var index = random.Next(items.Length);
            var item = items[index];
            counts[index]++;
            AddDocument($"{i}", item[0], item[1], item[2]);
        }

        for (var i = 0; i < items.Length; i++)
        {
            Console.WriteLine($"{items[i][0]}: {counts[i]}개");
        }
        
        // AddDocument($"{0}", "롱 소드", "힘 증가", "물리 공격력 증가");
        // AddDocument($"{1}", "롱 보우", "민첩 증가", "물리 공격력 증가");
        // AddDocument($"{2}", "스태프", "지능 증가", "마법 공격력 증가");
        // AddDocument($"{3}", "샙터", "정신력 증가", "마법 공격력 증가");

        _writer.Commit();
    }

    private void AddDocument(string id, string name, string option1, string option2)
    {
        var document = new Document
        {
            new StringField("id", id, Field.Store.YES),
            new StringField("name", name.Replace(" ", ""), Field.Store.YES),
            new StringField("option1", option1.Replace(" ", ""), Field.Store.YES),
            new StringField("option2", option2.Replace(" ", ""), Field.Store.YES)
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
            
            Console.WriteLine($"Matching results: {topDocs.TotalHits}");
            
            /*foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var document = searcher.Doc(scoreDoc.Doc);
                Console.WriteLine(document.GetField("name")?.GetStringValue());
                Console.WriteLine(document.GetField("option1")?.GetStringValue());
                Console.WriteLine(document.GetField("option2")?.GetStringValue());
                Console.WriteLine("===============================================================================");
            }*/
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
            
            /*foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var document = searcher.Doc(scoreDoc.Doc);
                Console.WriteLine(document.GetField("name")?.GetStringValue());
                Console.WriteLine(document.GetField("option1")?.GetStringValue());
                Console.WriteLine(document.GetField("option2")?.GetStringValue());
                Console.WriteLine("===============================================================================");
            }*/
        }
        finally
        {
            _searchManager.Release(searcher);
            searcher = null;
            
            stopwatch.Stop(); // 수행 시간 측정 종료
            Console.WriteLine($"Search execution time: {stopwatch.ElapsedMilliseconds} ms \n");
        }
    }
}