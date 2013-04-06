namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Runtime.Serialization.Formatters.Binary;
  using System.Text;

  [Serializable]
  public class UtilInstance : ObjectInstance
  {
    private static readonly Random RandomInstance = new Random();
    private const string PasswordCharsLcase = "abcdefghijklmnopqrstuvwxyz";
    private const string PasswordCharsNumeric = "0123456789";
    private const string PasswordCharsSpecial = "*$-+?_&=!%{}/";
    private const string PasswordCharsWhitespace = " \r\n\t\f\v";

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
      get { return RandomInstance; }
    }

    [JSFunction(Name = "randomString")]
    public static string RandomString(int size,
      [DefaultParameterValue(true)] bool allowNumbers,
      [DefaultParameterValue(true)] bool allowUpperCase,
      [DefaultParameterValue(true)] bool allowLowerCase,
      [DefaultParameterValue(true)] bool allowSpecialChars,
      [DefaultParameterValue(true)] bool allowWhitespace)
    {
      StringBuilder builder = new StringBuilder();

      List<char> validCharList = new List<char>();
      if (allowNumbers)
        validCharList.AddRange(PasswordCharsNumeric.ToCharArray());
      if (allowLowerCase)
        validCharList.AddRange(PasswordCharsLcase.ToCharArray());
      if (allowUpperCase)
        validCharList.AddRange(PasswordCharsLcase.ToUpper().ToCharArray());
      if (allowSpecialChars)
        validCharList.AddRange(PasswordCharsSpecial.ToCharArray());
      if (allowWhitespace)
        validCharList.AddRange(PasswordCharsWhitespace.ToCharArray());

      while (builder.Length < size)
      {
        builder.Append(validCharList[RandomInstance.Next(0, validCharList.Count)]);
      }

      return builder.ToString();
    }

    [JSFunction(Name = "getExtensionFromFileName")]
    public static string GetExtensionFromFileName(string fileName)
    {
      return Path.GetExtension(fileName);
    }

    [JSFunction(Name = "getMimeTypeFromFileName")]
    public static string GetMimeTypeFromFileName(string fileName)
    {
      return StringHelper.GetMimeTypeFromFileName(fileName);
    }

    [JSFunction(Name = "getCurrentCorrelationId")]
    public static string GetCurrentCorrelationId()
    {
      //SharePoint makes it soooo easy.
      var g = new Guid();
      EventActivityIdControl(1, ref g);
      return g.ToString();
    }

    [JSFunction(Name = "serializeObjectToByteArray")]
    public object SerializeObject(object objectToSerialize)
    {
      if (objectToSerialize == Null.Value || objectToSerialize == Undefined.Value || objectToSerialize == null)
        return Null.Value;

      var serializer = new BinaryFormatter();

      Base64EncodedByteArrayInstance result;
      using (var stream = new MemoryStream())
      {
        serializer.Serialize(stream, objectToSerialize);
        result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, stream.ToArray());
      }

      return result;
    }

    [JSFunction(Name = "deserializeObjectFromByteArray")]
    public object DeserializeObject(object serializedObject)
    {
      if (serializedObject == Null.Value || serializedObject == Undefined.Value || serializedObject == null)
        return Null.Value;

      var dataArray = serializedObject as Base64EncodedByteArrayInstance;
      if (dataArray != null)
      {
        ScriptEngine.DeserializationEnvironment = this.Engine;

        var serializer = new BinaryFormatter();
        using (var stream = new MemoryStream(dataArray.Data))
        {
          var result = serializer.Deserialize(stream);
          return result;
        }
      }

      return Null.Value;
    }

    [JSFunction(Name = "replaceJsonReferences")]
    public static object ReplaceJsonReferences(object o)
    {
      var dictionary = new Dictionary<string, ObjectInstance>();
      return ReplaceJsonReferences(o, dictionary);
    }

    private static object ReplaceJsonReferences(object o, Dictionary<string, ObjectInstance> dictionary)
    {
      if (o is ArrayInstance)
      {
        var array = o as ArrayInstance;
        for (int i = 0; i < array.ElementValues.Count(); i++)
        {

          array[i] = ReplaceJsonReferences(array[i], dictionary);
        }
      }
      else if (o is ObjectInstance)
      {
        var obj = o as ObjectInstance;
        var properties = obj.Properties.ToList();

        //If there's only one property named "$ref" and it's value is a key that exists in the dictionary, return the value.
        if (properties.Count == 1 && properties[0].Name == "$ref" && dictionary.ContainsKey((string)properties[0].Value))
          return dictionary[(string)properties[0].Value];

        var idProperty = properties.FirstOrDefault(p => p.Name == "$id");
        if (idProperty != null && dictionary.ContainsKey((string)idProperty.Value) == false)
        {
          var str = JSONObject.Stringify(obj.Engine, obj, null, null);
          var clone = JSONObject.Parse(obj.Engine, str, null) as ObjectInstance;

          if (clone != null)
          {
            if (clone.HasProperty("$id"))
              clone.Delete("$id", false);

            dictionary.Add((string) idProperty.Value, clone);
          }
        }

        foreach (var property in properties)
        {
          obj.SetPropertyValue(property.Name, ReplaceJsonReferences(property.Value, dictionary), false);
        }
      }
      return o;
    }

  }
}
