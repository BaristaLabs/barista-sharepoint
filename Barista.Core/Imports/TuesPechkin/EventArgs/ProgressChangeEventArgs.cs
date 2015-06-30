namespace Barista.TuesPechkin
{
    using System;

    public class ProgressChangeEventArgs : EventArgs
    {
        public IDocument Document { get; set; }

        public int Progress { get; set; }

        public string ProgressDescription { get; set; }
    }
}