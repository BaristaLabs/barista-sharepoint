namespace Barista.TuesPechkin
{
    using System;

    public class FinishEventArgs : EventArgs
    {
        public IDocument Document { get; set; }

        public bool Success { get; set; }
    }
}