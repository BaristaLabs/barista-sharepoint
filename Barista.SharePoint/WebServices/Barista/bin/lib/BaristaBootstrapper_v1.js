var vm = require('vm');
var _ = require('lodash');
var util = require('util');
var edge = require('edge');

var expose = [
	'Buffer',
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


module.exports = function (data, callback) {

    //If the there is a require whitelist defined in the data, use that, otherwise use the default require whitelist
    var whitelist = defaultRequireWhitelist;
    if (data.requireWhitelist && util.isArray(data.requireWhitelist)) {
        whitelist = data.requireWhitelist;
    }

    //Initialize our sandbox
    var sandbox = {};
    for (var i = 0; i < expose.length; i++) {
        sandbox[expose[i]] = global[expose[i]];
    }

    //define the barista global
    sandbox.barista = {
        isArray: util.isArray,
        isDate: util.isDate,
        isDefined: function (obj) { return !util.isUndefined(obj); },
        isFunction: util.isFunction,
        isNumber: util.isNumber,
        isObject: util.isObject,
        isString: util.isString,
        isUndefined: util.isUndefined
    };

    var baristaContext = {
        request_xml: data.context.request_xml,
        response_xml: data.context.response_xml,
        request: JSON.parse(data.context.request),
        response: JSON.parse(data.context.response),
        environment: JSON.parse(data.context.environment)
    };

    //define the ensureSPBaristaContext function
    sandbox.barista.ensureSPBaristaContext = edge.func({
        source: function () {/*

		using Barista.SharePoint;
		using System.Threading.Tasks;

		public class Startup
		{
			public async Task<object> Invoke(dynamic context)
			{
				if (SPBaristaContext.HasCurrentContext == false) {
					var request = (string)context.request;
					var response = (string)context.response;
					SPBaristaContext.Current = SPBaristaContext.CreateContextFromXmlRequestResponse(request, response);
				}
				return SPBaristaContext.HasCurrentContext;
			}
		}
	*/},
        references: [
			baristaContext.environment.sharePointAssembly,
			baristaContext.environment.baristaAssembly,
			baristaContext.environment.baristaSharePointAssembly
        ]
    });

    //Define the require remap in our sandbox
    sandbox.require = function (id) {

        if (_.includes(whitelist, id)) {
            return require(id);
        }

        if (id === "barista-context") {
            return baristaContext;
        }

        //Define the packages that was there previously.
        if (id === "SharePoint") {
            return require("./../lib/SharePoint_v1.js")(baristaContext);
        }

        return require("./../lib/SharePoint_Require_v1.js")(_.cloneDeep(sandbox), baristaContext, id);
    };

    try {
        sandbox.barista.ensureSPBaristaContext({
            request: baristaContext.request_xml,
            response: baristaContext.response_xml
        });

        var result = vm.runInNewContext(data.code, vm.createContext(sandbox), data.path);
        callback(null, result);
    } catch (e) {
        callback(e.stack, result);
    }
};