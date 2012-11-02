namespace Barista.Library
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;

  public class UtilInstance : ObjectInstance
  {
    private static Random s_random = new Random();
    private static string PASSWORD_CHARS_LCASE = "abcdefghijklmnopqrstuvwxyz";
    private static string PASSWORD_CHARS_NUMERIC = "0123456789";
    private static string PASSWORD_CHARS_SPECIAL = "*$-+?_&=!%{}/";
    private static string PASSWORD_CHARS_WHITESPACE = " \r\n\t\f\v";

    [System.Runtime.InteropServices.DllImport("advapi32.dll")]
    public static extern uint EventActivityIdControl(uint controlCode, ref Guid activityId);

    public UtilInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public static Random Random
    {
      get { return s_random; }
    }

    [JSFunction(Name = "randomString")]
    public static string RandomString(int size,
      [DefaultParameterValue(true)] bool allowNumbers = true,
      [DefaultParameterValue(true)] bool allowUpperCase = true,
      [DefaultParameterValue(true)] bool allowLowerCase = true,
      [DefaultParameterValue(true)] bool allowSpecialChars = true,
      [DefaultParameterValue(true)] bool allowWhitespace = true)
    {
      StringBuilder builder = new StringBuilder();

      List<char> validCharList = new List<char>();
      if (allowNumbers)
        validCharList.AddRange(PASSWORD_CHARS_NUMERIC.ToCharArray());
      if (allowLowerCase)
        validCharList.AddRange(PASSWORD_CHARS_LCASE.ToCharArray());
      if (allowUpperCase)
        validCharList.AddRange(PASSWORD_CHARS_LCASE.ToUpper().ToCharArray());
      if (allowSpecialChars)
        validCharList.AddRange(PASSWORD_CHARS_SPECIAL.ToCharArray());
      if (allowWhitespace)
        validCharList.AddRange(PASSWORD_CHARS_WHITESPACE.ToCharArray());

      while (builder.Length < size)
      {
        builder.Append(validCharList[s_random.Next(0, validCharList.Count)]);
      }

      return builder.ToString();
    }

    [JSFunction(Name = "getCurrentCorrelationId")]
    public static string GetCurrentCorrelationId()
    {
      //SharePoint makes it soooo easy.
      var g = new Guid();
      EventActivityIdControl(1, ref g);
      return g.ToString();
    }
  }
}
