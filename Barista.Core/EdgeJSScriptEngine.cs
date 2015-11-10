namespace Barista
{
    using Barista.Engine;
    using EdgeJs;
    using System.Threading.Tasks;

    public sealed class EdgeJSScriptEngine : IScriptEngine
    {
        public object Evaluate(IScriptSource script)
        {
            using (var x = script.GetReader())
            {
                var code = x.ReadToEnd();

                var evaluateTask = Task.Run<object>(() => ExecuteAsync(code, script.Path));
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

        private static async Task<object> ExecuteAsync(string code, string path)
        {
            var executeFunc = Edge.Func(@"
var vm = require('vm');
var _ = require('lodash');
var util = require('util');

var sandbox = {};
var expose = [
    'barista',
    'setTimeout',
    'setInterval',
    'clearTimeout',
    'clearInterval'
];

var defaultRequireWhitelist = [
    'buffer',
    'cluster',
    'crypto',
    'dns',
    'errors',
    'events',
    'os',
    'path',
    'process',
    'punycode',
    'querystring',
    'stream',
    'string_decoder',
    'url',
    'util',
    'v8',
    'vm',
    'zlib',
    
    'async',
    'csso',
    'edge',
    'edge-cs',
    'linq',
    'lodash',
    'moment',
    'q',
    'uglify-js'
];

for (var i = 0; i < expose.length; i++) {
    sandbox[expose[i]] = global[expose[i]];
}

return function(data, callback) {

    var whitelist = defaultRequireWhitelist;
    if (data.requireWhitelist && util.isArray(data.requireWhitelist)) {
        whitelist = data.requireWhitelist;
    }

    //Define the require remap.
    sandbox.require = function(id) {
        
        if (_.includes(whitelist, id)) {
            return require(id);
        }

        //TODO: load files from incoming barista context...
        return undefined;
    };

    try {
        var result = vm.runInNewContext(data.code, vm.createContext(sandbox), data.path);
        callback(null, result);
    } catch (e) {
        callback(e.stack, result);
    }
};");
            var data = new {
                code = code,
                path = path
            };

            var res = await executeFunc(data);
            return res;
        }
    }
}
