namespace Barista.Justache
{
  using System;
  using System.Collections.Generic;

  public class Parser
  {
    public void Parse(Section section, IEnumerable<Part> parts)
    {
      if (section == null)
        throw new ArgumentNullException("section");

      var sectionStack = new Stack<Section>();
      sectionStack.Push(section);

      foreach (var part in parts)
      {
        section.Add(part);

        var part1 = part as Section;
        if (part1 != null)
        {
          sectionStack.Push(section);
          section = part1;
        }
        else
        {
          var section1 = part as EndSection;
          if (section1 != null)
          {
            var endSection = section1;

            if (sectionStack.Count == 1)
            {
              throw new JustacheException(
                string.Format(
                  "End section {0} does not match any start section!",
                  endSection.Name));
            }

            if (endSection.Name != section.Name)
            {
              throw new JustacheException(
                string.Format(
                  "End section {0} does not match start section {1}!",
                  endSection.Name,
                  section.Name));
            }

            section = sectionStack.Pop();
          }
        }
      }
    }
  }
}