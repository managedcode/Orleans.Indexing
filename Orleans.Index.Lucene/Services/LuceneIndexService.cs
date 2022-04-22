using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using ManagedCode.Storage.Core;
using ManagedCode.Storage.Core.Models;
using Orleans.Concurrency;
using Orleans.Index.Annotations;
using Directory = System.IO.Directory;

namespace Orleans.Index.Lucene.Services;

[Reentrant]
public class LuceneIndexService : IIndexService, IDisposable
{
    // Ensures index backward compatibility
    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

    private Analyzer _analyzer;
    private IndexSearcher _indexSearcher;

    private readonly IStorage _storage;
    private readonly string _indexPath;
    private readonly Dictionary<string, BaseDirectory> _tempDirectories;
    private readonly Dictionary<string, IndexWriter> _writers;
    private MultiReader _reader;

    public LuceneIndexService(IStorage storage)
    {
        _storage = storage;
        _indexPath = Path.Combine(Path.GetTempPath(), "lucene");

        _writers = new Dictionary<string, IndexWriter>();
        _tempDirectories = new Dictionary<string, BaseDirectory>();

        InitDirectories();
        DownloadCache();
        InitSearcher();
    }

    public Task WriteIndex(Dictionary<string, object> properties)
    {
        var grainId = properties[Constants.GrainId] as string;

        var typeName = properties[Constants.TypeName] as string;

        var document = new GrainDocument(grainId);

        foreach (var property in properties)
        {
            document.LuceneDocument.Add(new StringField(property.Key, property.Value as string, Field.Store.YES));
        }

        var parser = new QueryParser(AppLuceneVersion, Constants.GrainId, _analyzer);
        var query = parser.Parse(document.LuceneDocument.GetField(Constants.GrainId).GetStringValue());

        _writers[typeName].DeleteDocuments(query);
        _writers[typeName].AddDocument(document.LuceneDocument);
        _writers[typeName].Commit();

        return Task.CompletedTask;
    }

    public Task<IList<string>> GetGrainIdsByQuery(string? field, string query, int take = 1000) => Task.Run(() =>
    {
        var parser = new QueryParser(AppLuceneVersion, field ?? Constants.GrainId, _analyzer);
        var result = _indexSearcher.Search(parser.Parse(query), null, take);

        IList<string> ids = new List<string>();

        foreach (var doc in result.ScoreDocs)
        {
            var document = _indexSearcher.Doc(doc.Doc);
            var indexableField = document.Fields.FirstOrDefault(f => f.Name == Constants.GrainId);
            ids.Add(indexableField.GetStringValue());
        }

        return ids;
    });

    public void DownloadCache()
    {
        var blobs = _storage.GetBlobList().ToList();

        foreach (var blob in blobs)
        {
            var splits = blob.Name.Split("__");

            if (splits.Length != 2)
            {
                continue;
            }

            var directoryName = splits[0];
            var fileName = splits[1];

            var directoryPath = Path.Combine(_indexPath, directoryName);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var path = Path.Combine(directoryPath, fileName);
            var file = _storage.Download(blob)!;

            using (var stream = File.Create(path))
            {
                file.FileStream.CopyToAsync(stream);
                file.Close();
                stream.Flush();
            }
        }
    }

    public void Dispose()
    {
        foreach (var writer in _writers)
        {
            writer.Value.Flush(false, false);
            writer.Value.Dispose();
        }

        foreach (var tempDirectory in _tempDirectories)
        {
            tempDirectory.Value.Dispose();
            UploadFiles(tempDirectory.Key);
        }

        _analyzer?.Dispose();
    }

    private void UploadFiles(string directoryName)
    {
        var path = Path.Combine(_indexPath, directoryName);
        var files = Directory.GetFiles(path);

        foreach (var filePath in files)
        {
            BlobMetadata blobMetadata = new()
            {
                Name = $"{directoryName}__{Path.GetFileName(filePath)}",
                Rewrite = true,
            };

            _storage.Upload(blobMetadata, filePath);
        }
    }

    public void InitDirectories()
    {
        _analyzer = new StandardAnalyzer(AppLuceneVersion);

        var grainTypes = GetEnumerableOfType<IndexGrain>();

        foreach (var grainType in grainTypes)
        {
            var directoryPath = Path.Combine(_indexPath, grainType.Name);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var directory = FSDirectory.Open(directoryPath);
            _tempDirectories.Add(grainType.Name, directory);

            var config = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            var indexWriter = new IndexWriter(directory, config);

            _writers.Add(grainType.Name, indexWriter);
        }
    }

    private void InitSearcher()
    {
        var readers = _writers.Select(r => r.Value.GetReader(false)).ToArray();
        _reader = new MultiReader(readers);
        _indexSearcher = new IndexSearcher(_reader);
    }

    public static IEnumerable<Type> GetEnumerableOfType<T>() where T : class
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        return assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)));
    }
}