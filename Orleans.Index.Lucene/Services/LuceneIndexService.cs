using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Orleans.Concurrency;
using Orleans.Indexing;

namespace Orleans.Index.Lucene.Services;

[Reentrant]
public class LuceneIndexService : IIndexService, IDisposable
{
    // Ensures index backward compatibility
    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
    private static BaseDirectory GetDirectory() => new RAMDirectory();


    private readonly BaseDirectory _indexDirectory;
    private readonly Analyzer _analyzer;
    private readonly IndexWriter _indexWriter;
    private DirectoryReader _directoryReader;
    private IndexSearcher _indexSearcher;

    public LuceneIndexService()
    {
        _indexDirectory = GetDirectory();
        _analyzer = new StandardAnalyzer(AppLuceneVersion);
        var config = new IndexWriterConfig(AppLuceneVersion, _analyzer);
        _indexWriter = new IndexWriter(_indexDirectory, config);
        _indexWriter.Commit();
        _directoryReader = DirectoryReader.Open(_indexDirectory);
        _indexSearcher = new IndexSearcher(_directoryReader);
    }

    public Task WriteIndex(GrainDocument document) => Task.Run(() =>
    {
        var parser = new QueryParser(AppLuceneVersion, GrainDocument.GrainIdFieldName, _analyzer);
        var query = parser.Parse(document.LuceneDocument.GetField(GrainDocument.GrainIdFieldName).GetStringValue());
        _indexWriter.DeleteDocuments(query);
        _indexWriter.AddDocument(document.LuceneDocument);
        _indexWriter.Commit();

        _directoryReader = DirectoryReader.OpenIfChanged(_directoryReader) ?? _directoryReader;
        _indexSearcher = new IndexSearcher(_directoryReader);

        return Task.CompletedTask;
    });

    // public Task<TopDocs> QueryByField(string field, string query, int take = 1000) => Task.Run(() =>
    // {
    //     var parser = new QueryParser(AppLuceneVersion, field ?? GrainDocument.GrainIdFieldName, this.analyzer);
    //     var result = this.indexSearcher.Search(parser.Parse(query), null, take);
    //
    //
    //     return result;
    // });

    public Task<IList<string>> GetGrainIdsByQuery(string field, string query, int take = 1000) => Task.Run(() =>
    {
        var parser = new QueryParser(AppLuceneVersion, field ?? GrainDocument.GrainIdFieldName, _analyzer);
        var result = _indexSearcher.Search(parser.Parse(query), null, take);

        IList<string> ids = new List<string>();
        foreach (var doc in result.ScoreDocs)
        {
            var document = _indexSearcher.Doc(doc.Doc);
            var indexableField = document.Fields.FirstOrDefault(f => f.Name == GrainDocument.GrainIdFieldName);
            ids.Add(indexableField.GetStringValue());
        }

        return ids;
    });

    public void Dispose()
    {
        _indexDirectory.Dispose();
        _directoryReader?.Dispose();
        _analyzer?.Dispose();
        _indexWriter?.Dispose();
    }
}