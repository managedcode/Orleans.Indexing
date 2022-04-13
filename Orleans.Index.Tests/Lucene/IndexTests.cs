using System;
using System.Threading.Tasks;
using FluentAssertions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Orleans.Index.Annotations;
using Orleans.Index.Lucene.Services;
using Orleans.Index.Tests.Grains;
using Xunit;

namespace Orleans.Index.Tests.Lucene;

public class IndexTests
{
    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

    // RAM
    private readonly string _indexPath = $"indexPath-{Guid.NewGuid():N}.luc";
    private const string Text = "This is the text to be indexed.";
    private const string FieldName = "fieldname";
    readonly Analyzer _analyzer = new StandardAnalyzer(AppLuceneVersion);

    [Fact]
    public void Index_Write_Query()
    {
        var ramDirectory = FSDirectory.Open(_indexPath);

        // Parse a simple query that searches for "text":
        var parser = new QueryParser(AppLuceneVersion, FieldName, _analyzer);
        var query = parser.Parse("text");

        var config = new IndexWriterConfig(AppLuceneVersion, _analyzer);
        var indexWriter = new IndexWriter(ramDirectory, config);

        var doc1 = new Document {new Field(FieldName, Text, TextField.TYPE_STORED)};

        indexWriter.AddDocument(doc1);
        indexWriter.Commit();
        // indexWriter.Flush(true,true);


        var indexReader = DirectoryReader.Open(ramDirectory);
        var indexSearcher = new IndexSearcher(indexReader);

        var hits = indexSearcher.Search(query, null, 1000).ScoreDocs;
        hits.Length.Should().Be(1);

        // Iterate through the results:
        foreach (var t in hits)
        {
            var hitDoc = indexSearcher.Doc(t.Doc);
            hitDoc.Get(FieldName).Should().Be(Text);
        }


        indexWriter.DeleteDocuments(query);
        indexWriter.Commit();
        // indexWriter.Flush(true,true);

        var indexReader2 = DirectoryReader.Open(ramDirectory);
        var indexSearcher2 = new IndexSearcher(indexReader2);
        hits = indexSearcher2.Search(query, null, 1000).ScoreDocs;
        hits.Length.Should().Be(0);

        hits = indexSearcher.Search(query, null, 1000).ScoreDocs;
        hits.Length.Should().Be(1);


        indexReader2.Dispose();
        indexReader.Dispose();
    }

    // [Fact]
    // public async Task GrainTest()
    // {
    //     var service = new LuceneIndexService();
    //     var grain = new TestGrain(service);
    //
    //     await grain.OnActivateAsync();
    //
    //     var docs = await service.QueryByField(nameof(TestGrain.Class.Test), "3");

    // await Task.WhenAll(Task.Run(async () =>
    // {
    //     for (int i = 0; i < 150; i++)
    //     {
    //         var doc = new GrainDocument(i.ToString());
    //         doc.LuceneDocument.Add(new StringField("property", $"i=0", Field.Store.YES));
    //         await grain.WriteIndex(doc);
    //         count++;
    //     }
    // }), Task.Run(async () =>
    // {
    // await Task.Delay(1000);
    // for (int i = 0; i < 300; i++)
    // {
    //     var doc = await grain.QueryByField(nameof(grain.Test), $"0");
    //     count++;
    //
    //     if (doc.TotalHits > 0)
    //     {
    //         foundCont += 1;
    //     }
    // }
    // }));

    //     await grain.OnDeactivateAsync();
    //
    //     docs.TotalHits.Should().Be(1);
    // }


    [Fact]
    public async Task GetGrainIds()
    {
        IIndexService service = new LuceneIndexService();

        const int count = 10;

        for (var i = 0; i < count; i++)
        {
            var grain = new TestGrain(service);
            await grain.OnActivateAsync();
        }

        var ids = await service.GetGrainIdsByQuery(nameof(TestGrain.Class.Test), "3");

        ids.Count.Should().Be(count);
    }
}