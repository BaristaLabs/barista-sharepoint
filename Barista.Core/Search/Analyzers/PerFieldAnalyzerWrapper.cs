namespace Barista.Search.Analyzers
{
  using System.Collections.Generic;
  using Lucene.Net.Analysis;
  using Lucene.Net.Documents;
  using System.Linq;

	public sealed class BaristaPerFieldAnalyzerWrapper : Analyzer
	{
		private readonly Analyzer m_defaultAnalyzer;
		private readonly IDictionary<string, Analyzer> m_analyzerMap = new Dictionary<string, Analyzer>();

		public BaristaPerFieldAnalyzerWrapper(Analyzer defaultAnalyzer)
		{
			this.m_defaultAnalyzer = defaultAnalyzer;
		}

		public void AddAnalyzer(System.String fieldName, Analyzer analyzer)
		{
			m_analyzerMap[fieldName] = analyzer;
		}

		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			return GetAnalyzer(fieldName).TokenStream(fieldName, reader);
		}

		private Analyzer GetAnalyzer(string fieldName)
		{
			if (fieldName.StartsWith("@"))
			{
				var indexOfFieldStart = fieldName.IndexOf('<');
				var indexOfFieldEnd = fieldName.LastIndexOf('>');
				if (indexOfFieldStart != -1 && indexOfFieldEnd != -1)
				{
					fieldName = fieldName.Substring(indexOfFieldStart + 1, indexOfFieldEnd - indexOfFieldStart - 1);
				}
			}
			Analyzer value;
			m_analyzerMap.TryGetValue(fieldName, out value);
			return value ?? m_defaultAnalyzer;
		}

		public override TokenStream ReusableTokenStream(string fieldName, System.IO.TextReader reader)
		{
			return GetAnalyzer(fieldName).ReusableTokenStream(fieldName, reader);
		}

		public override int GetPositionIncrementGap(string fieldName)
		{
			return GetAnalyzer(fieldName).GetPositionIncrementGap(fieldName);
		}

		public override int GetOffsetGap(IFieldable field)
		{
			return GetAnalyzer(field.Name).GetOffsetGap(field);
		}

		public override System.String ToString()
		{
			return "PerFieldAnalyzerWrapper(" + string.Join(",", m_analyzerMap.Select(x => x.Key + " -> " + x.Value).ToArray()) + ", default=" + m_defaultAnalyzer + ")";
		}
	}
}
