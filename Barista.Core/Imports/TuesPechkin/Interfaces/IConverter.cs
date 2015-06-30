namespace Barista.TuesPechkin
{
    using System;

    public interface IConverter
    {
        byte[] Convert(IDocument document);

        event EventHandler<BeginEventArgs> Begin;

        event EventHandler<WarningEventArgs> Warning;

        event EventHandler<ErrorEventArgs> Error;

        event EventHandler<PhaseChangeEventArgs> PhaseChange;

        event EventHandler<ProgressChangeEventArgs> ProgressChange;

        event EventHandler<FinishEventArgs> Finish;
    }
}
