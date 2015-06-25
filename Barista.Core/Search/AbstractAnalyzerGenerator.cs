namespace Barista.Search
{
  using Lucene.Net.Analysis;
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.Util;

  //[InheritedExport]
  public abstract class AbstractAnalyzerGenerator
  {
    public abstract Analyzer GenerateAnalyzerForIndexing(string indexName, Lucene.Net.Documents.Document document, Analyzer previousAnalyzer);

    public abstract Analyzer GenerateAnalyzerForQuerying(string indexName, string query, Analyzer previousAnalyzer);
  }

  public class BaristaAnalyzerGenerator : AbstractAnalyzerGenerator
  {
    public override Analyzer GenerateAnalyzerForIndexing(string indexName, Lucene.Net.Documents.Document document, Analyzer previousAnalyzer)
    {
      return new StandardAnalyzer(Version.LUCENE_30);
    }

    public override Analyzer GenerateAnalyzerForQuerying(string indexName, string query, Analyzer previousAnalyzer)
    {
      return new StandardAnalyzer(Version.LUCENE_30);
    }
  }
}
