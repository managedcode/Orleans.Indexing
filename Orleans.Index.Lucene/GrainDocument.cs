using Lucene.Net.Documents;

namespace Orleans.Index.Lucene;

public class GrainDocument
{
    public static string GrainIdFieldName = "___grainId";

    public GrainDocument(string grainId)
    {
        LuceneDocument = new Document {new StringField(GrainIdFieldName, grainId, Field.Store.YES)};
    }

    public Document LuceneDocument { get; }
}