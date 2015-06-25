﻿namespace Barista.Jurassic.Library
{
  using System;

  /// <summary>
  /// Represents an implementation of the Firebug API using the standard console.
  /// </summary>
  [Serializable]
  internal class StandardConsoleOutput : IFirebugConsoleOutput
  {
    private int m_currentIndentation;
    private int m_indentationDelta = 4;

    /// <summary>
    /// Gets or sets the number of spaces to output before writing any text to the console.
    /// </summary>
    public int CurrentIndentation
    {
      get { return this.m_currentIndentation; }
      set
      {
        if (value < 0 || value > 40)
          throw new ArgumentOutOfRangeException("value");
        this.m_currentIndentation = value;
      }
    }

    /// <summary>
    /// Gets or sets the number of spaces to add to the identation when group() is called.
    /// </summary>
    public int IndentationDelta
    {
      get { return this.m_indentationDelta; }
      set
      {
        if (value < 0 || value > 40)
          throw new ArgumentOutOfRangeException("value");
        this.m_indentationDelta = value;
      }
    }

    /// <summary>
    /// Logs a message to the console.
    /// </summary>
    /// <param name="style"> A style which influences the icon and text color. </param>
    /// <param name="objects"> The objects to output to the console. These can be strings or
    /// ObjectInstances. </param>
    public void Log(FirebugConsoleMessageStyle style, object[] objects)
    {
      var original = Console.ForegroundColor;
      switch (style)
      {
        case FirebugConsoleMessageStyle.Information:
          Console.ForegroundColor = ConsoleColor.White;
          break;
        case FirebugConsoleMessageStyle.Warning:
          Console.ForegroundColor = ConsoleColor.Yellow;
          break;
        case FirebugConsoleMessageStyle.Error:
          Console.ForegroundColor = ConsoleColor.Red;
          break;
      }
      // Convert the objects to a string.
      var message = new System.Text.StringBuilder();
      foreach (var t in objects)
      {
        message.Append(' ');
        message.Append(TypeConverter.ToString(t));
      }

      // Output the message to the console.
      Console.WriteLine(message.ToString());

      if (style != FirebugConsoleMessageStyle.Regular)
        Console.ForegroundColor = original;
    }

    /// <summary>
    /// Clears the console.
    /// </summary>
    public void Clear()
    {
      Console.Clear();
    }

    /// <summary>
    /// Starts grouping messages together.
    /// </summary>
    /// <param name="title"> The title for the group. </param>
    /// <param name="initiallyCollapsed"> <c>true</c> if subsequent messages should be hidden by default. </param>
    public void StartGroup(string title, bool initiallyCollapsed)
    {
      Log(FirebugConsoleMessageStyle.Regular, new object[] { title });
      this.CurrentIndentation = Math.Min(this.CurrentIndentation + this.IndentationDelta, 40);
    }

    /// <summary>
    /// Ends the most recently started group.
    /// </summary>
    public void EndGroup()
    {
      this.CurrentIndentation = Math.Max(this.CurrentIndentation - this.IndentationDelta, 0);
    }
  }
}
