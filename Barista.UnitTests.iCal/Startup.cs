namespace Barista.DDay.iCal.Test
{
    using System;
    using System.Reflection;

    public class Startup
    {
        [STAThread]
        static public void Main(string[] args)
        {
            NUnit.Gui.AppEntry.Main(new string[]
            {
                Assembly.GetExecutingAssembly().Location
            });
        }
    }
}
