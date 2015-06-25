namespace Barista.ConsoleTests
{
    using System;
    using Barista.Properties;
    using Barista.V8.Net;

    class Program
    {
        static void Main(string[] args)
        {
            var engine = new V8Engine();
            engine.Execute(Resources.uglifyjs);

            var res = engine.Execute("'hello' + ' ' + 'world!'");
            Console.WriteLine(res.AsString);
            engine.GlobalObject.SetProperty("code", @"(function (fallback) {
                fallback = fallback || function () { };
            })(null);");

            engine.Execute(@"var ast = UglifyJS.parse(code);
ast.figure_out_scope();
compressor = UglifyJS.Compressor();
ast = ast.transform(compressor);
var result = ast.print_to_string();");

            Console.WriteLine(engine.GlobalObject.GetProperty("result").ToString());

            Console.ReadLine();
        }
    }
}
