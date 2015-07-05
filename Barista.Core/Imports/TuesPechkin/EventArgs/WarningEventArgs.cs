namespace Barista.TuesPechkin
{
    using System;

    public class WarningEventArgs : EventArgs
    {
        public IDocument Document { get; set; }

        public string WarningMessage { get; set; }
    }
}