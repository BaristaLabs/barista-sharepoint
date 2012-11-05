namespace OFS.OrcaDB.Core.Framework
{
  using System;
  using System.IO;
  using System.Text;

  public class StringStream : MemoryStream
  {
    public StringStream(string str) :
      base(Encoding.UTF8.GetBytes(str), false) { }
  }
}