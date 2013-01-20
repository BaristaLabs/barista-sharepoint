namespace Barista.Linq2Rest.Parser
{
  using System;
  using System.Collections.Concurrent;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.Serialization;
  using System.Xml.Serialization;

  internal class MemberNameResolver : IMemberNameResolver
  {
    private static readonly ConcurrentDictionary<MemberInfo, string> KnownMemberNames =
      new ConcurrentDictionary<MemberInfo, string>();

    public string ResolveName(MemberInfo member)
    {
      var result = KnownMemberNames.GetOrAdd(member, ResolveNameInternal);

      return result;
    }

    private static string ResolveNameInternal(MemberInfo member)
    {
      if (member == null)
        throw new ArgumentNullException("member");

      var dataMember = member.GetCustomAttributes(typeof (DataMemberAttribute), true)
                             .OfType<DataMemberAttribute>()
                             .FirstOrDefault();

      if (dataMember != null && dataMember.Name != null)
      {
        return dataMember.Name;
      }

      var xmlElement = member.GetCustomAttributes(typeof (XmlElementAttribute), true)
                             .OfType<XmlElementAttribute>()
                             .FirstOrDefault();

      if (xmlElement != null && xmlElement.ElementName != null)
      {
        return xmlElement.ElementName;
      }

      var xmlAttribute = member.GetCustomAttributes(typeof (XmlAttributeAttribute), true)
                               .OfType<XmlAttributeAttribute>()
                               .FirstOrDefault();

      if (xmlAttribute != null && xmlAttribute.AttributeName != null)
      {
        return xmlAttribute.AttributeName;
      }

      if (member.Name == null)
        throw new InvalidOperationException("Member must have a name.");

      return member.Name;
    }
  }
}