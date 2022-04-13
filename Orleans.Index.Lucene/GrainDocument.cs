using Lucene.Net.Documents;

namespace Orleans.Index.Lucene;

public class GrainDocument
{
    public GrainDocument(string grainId)
    {
        LuceneDocument = new Document {new StringField(Constants.GrainId, grainId, Field.Store.YES)};
    }

    public Document LuceneDocument { get; }
}