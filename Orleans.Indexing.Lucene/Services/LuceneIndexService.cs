using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Orleans.Indexing.Abstractions;

namespace Orleans.Indexing.Lucene.Services;

public abstract class LuceneIndexService : IIndexService, IDisposable
{
    // Ensures index backward compatibility
    protected const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
    protected MultiReader Reader;
    protected readonly Analyzer Analyzer;
    protected IndexSearcher IndexSearcher;

    protected readonly Dictionary<string, IndexWriter> Writers;
    protected readonly Dictionary<string, BaseDirectory> TempDirectories;

    protected LuceneIndexService()
    {
        Analyzer = new StandardAnalyzer(AppLuceneVersion);
        Writers = new Dictionary<string, IndexWriter>();
        TempDirectories = new Dictionary<string, BaseDirectory>();
    }

    public abstract Task InitializeAsync();

    public Task WriteIndex(Dictionary<string, object> properties)
    {
        var grainId = (properties[Constants.GrainId] as string)!;
        var typeName = (properties[Constants.TypeName] as string)!;
        var document = new GrainDocument(grainId);

        foreach (var property in properties)
        {
            document.LuceneDocument.Add(new StringField(property.Key, property.Value as string, Field.Store.YES));
        }

        var parser = new QueryParser(AppLuceneVersion, Constants.GrainId, Analyzer);
        var query = parser.Parse(document.LuceneDocument.GetField(Constants.GrainId).GetStringValue());

        Writers[typeName].DeleteDocuments(query);
        Writers[typeName].AddDocument(document.LuceneDocument);
        Writers[typeName].Commit();

        InitSearcher();

        return Task.CompletedTask;
    }

    public async Task<IList<string>> GetGrainIdsByQuery(string field, string query, int take = 1000)
    {
        return await GetGrainIdsByQueryInternal(IndexSearcher, field, query, take);
    }

    public async Task<IList<string>> GetGrainIdsByQuery<T>(string field, string query, int take = 1000) where T : IndexGrain
    {
        var grainName = typeof(T).Name;
        IndexSearcher searcher = new(Writers[grainName].GetReader(false));

        return await GetGrainIdsByQueryInternal(searcher, field, query, take);
    }

    private Task<IList<string>> GetGrainIdsByQueryInternal(IndexSearcher searcher, string? field, string query, int take = 1000) => Task.Run(() =>
    {
        var parser = new QueryParser(AppLuceneVersion, field ?? Constants.GrainId, Analyzer);
        var result = searcher.Search(parser.Parse(query), null, take);

        IList<string> ids = new List<string>();

        foreach (var doc in result.ScoreDocs)
        {
            var document = searcher.Doc(doc.Doc);
            var grainIdField = document.Fields.FirstOrDefault(f => f.Name == Constants.GrainId)!;
            ids.Add(grainIdField.GetStringValue());
        }

        return ids;
    });

    public virtual void Dispose()
    {
        Analyzer.Dispose();

        foreach (var writer in Writers)
        {
            writer.Value.Flush(true, true);
            writer.Value.Dispose();
        }

        foreach (var tempDirectory in TempDirectories)
        {
            tempDirectory.Value.Dispose();
        }
    }

    protected void InitWriters()
    {
        foreach (var tempDirectory in TempDirectories)
        {
            var config = new IndexWriterConfig(AppLuceneVersion, Analyzer);
            var indexWriter = new IndexWriter(tempDirectory.Value, config);

            Writers.Add(tempDirectory.Key, indexWriter);
        }
    }

    protected void InitSearcher()
    {
        var readers = Writers.Select(r => r.Value.GetReader(false)).ToArray();
        Reader = new MultiReader(readers);
        IndexSearcher = new IndexSearcher(Reader);
    }

    protected static IEnumerable<Type> GetEnumerableOfType<T>() where T : class
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        return assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)));
    }
}