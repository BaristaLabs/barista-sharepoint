namespace Barista.ConsoleTests
{
    using System;
    using Barista.Properties;
    using Barista.Extensions;
    using System.IO;
    using System.Linq;
    using Barista.Newtonsoft.Json.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            
            var jo = new JObject();
            jo["name"] = "foooo";
            jo["version"] = "5.6.43.1.6";
            Package.IsValidPackage(jo);

            //Console.WriteLine(results);
            //Console.WriteLine(engine.GlobalObject.GetProperty("result").ToString());

            Console.ReadLine();
        }
    }
}
