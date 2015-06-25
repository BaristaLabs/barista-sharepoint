namespace Barista.Justache
{
  using System.IO;

  public class FileSystemTemplateLocator
  {
    private readonly string m_extension;
    private readonly string[] m_directories;

    public FileSystemTemplateLocator(string extension, params string[] directories)
    {
      m_extension = extension;
      m_directories = directories;
    }

    public Template GetTemplate(string name)
    {
      foreach (var directory in m_directories)
      {
        var path = Path.Combine(directory, name + m_extension);

        if (File.Exists(path))
        {
          var text = File.ReadAllText(path);
          var reader = new StringReader(text);
          var template = new Template();
          template.Load(reader);

          return template;
        }
      }

      return null;
    }
  }
}