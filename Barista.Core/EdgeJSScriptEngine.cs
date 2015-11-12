namespace Barista
{
    using Barista.Engine;
    using EdgeJs;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Threading.Tasks;

    public sealed class EdgeJSScriptEngine : IScriptEngine
    {
        public object Evaluate(IScriptSource script)
        {
            using (var x = script.GetReader())
            {
                var code = x.ReadToEnd();

                var bootstrapperPath = String.Empty;
                if (script.Flags.ContainsKey("bootstrapperPath"))
                    bootstrapperPath = script.Flags["bootstrapperPath"];

                var evaluateTask = Task.Run<object>(() => ExecuteAsync(bootstrapperPath, code, script.Path, script.Flags));
                evaluateTask.Wait();

                return evaluateTask.Result;
            }
        }

        public string Stringify(object value, object replacer, object spacer)
        {
            var stringifyTask = Task.Run<object>(() => StringifyAsync(value, replacer, spacer));
            stringifyTask.Wait();

            if (stringifyTask.Result == null)
                return string.Empty;

            return stringifyTask.Result.ToString();
        }

        private static async Task<object> StringifyAsync(object value, object replacer, object spacer)
        {
            var stringifyFunc = Edge.Func(@"
return function(data, callback) {
    var result = JSON.stringify(data.value, data.replacer, data.spacer);
    callback(null, result);
};");

            var input = new {
                value = value,
                replacer = replacer,
                spacer = spacer
            };

            var res = await stringifyFunc(input);
            return res;
        }

        private static async Task<object> ExecuteAsync(string bootstrapperPath, string code, string path, IDictionary<string, string> flags)
        {
            if (String.IsNullOrWhiteSpace(bootstrapperPath))
                throw new ArgumentNullException(bootstrapperPath);

            var executeFunc = Edge.Func(@"return require('" + bootstrapperPath + "')");

            dynamic baristaContext = new ExpandoObject();
            var dict = baristaContext as IDictionary<string, Object>;
            if (flags != null)
            {
                foreach (var key in flags.Keys)
                {
                    dict.Add(key, flags[key]);
                }
            }

            var data = new {
                code = code,
                path = path,
                context = baristaContext
            };

            var res = await executeFunc(data);
            return res;
        }
    }
}
