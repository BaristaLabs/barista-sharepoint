namespace Barista.TuesPechkin
{
    using System;

    public class PhaseChangeEventArgs : EventArgs
    {
        public IDocument Document { get; set; }

        public int PhaseNumber { get; set; }

        public string PhaseDescription { get; set; }
    }
}