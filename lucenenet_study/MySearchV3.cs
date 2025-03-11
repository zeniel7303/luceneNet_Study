using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lucene.Net.Documents;

namespace LucenenetStudy;

public class MySearchV3
{
    private enum EName
    {
        LongSword,
        LongBow,
        Staff,
        Scepter
    }
    
    private enum EOption
    {
        IncStr,
        IncDex,
        IncInt,
        IncMen,
        IncPhyDmg,
        IncMagDmg
    }
    
    private const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;

    private readonly IndexWriter _writer;
    private readonly SearcherManager _searchManager;
    private readonly QueryParser _queryParser;
    
    private static readonly string[] fields = new[] { "name", "option1", "value1", "option2", "value2"};
    
    public MySearchV3(string indexPath)
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
            new[] { 1, 1, 10, 5, 12 },
            new[] { 2, 2, 8, 5, 7 }, 
            new[] { 3, 3, 12, 6, 6 },
            new[] { 4, 4, 9, 6, 11 }
        };

        var counts = new int[items.Length];
        var random = new Random();
        for (var i = 0; i < num; i++)
        {
            var index = random.Next(items.Length);
            var item = items[index];
            counts[index]++;
            AddDocument(item[0], item[1], item[2], item[3], item[4]);
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
    
    private void AddDocument(int name, int option1, int value1, int option2, int value2)
    { 
        var document = new Document
        {
            new Int64Field("name", name, Field.Store.YES), 
            new StringField("option1_value1", $"{option1}:{value1}", Field.Store.YES),
            new StringField("option2_value2", $"{option2}:{value2}", Field.Store.YES)
        };
        
        /*var document = new Document
        {
            new Int64Field("id", id, Field.Store.YES),
            new Int64Field("name", name, Field.Store.YES), 
            new Int64Field("option1", option1, Field.Store.YES), 
            new Int64Field("value1", value1, Field.Store.YES), 
            new Int64Field("option2", option2, Field.Store.YES), 
            new Int64Field("value2", value2, Field.Store.YES)
        };*/

        _writer.AddDocument(document);
    }
}