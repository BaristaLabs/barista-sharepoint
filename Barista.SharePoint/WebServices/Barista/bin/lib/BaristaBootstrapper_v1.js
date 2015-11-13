var vm = require('vm');
var _ = require('lodash');
var util = require('util');
var edge = require('edge');
var Handlebars = require('handlebars');

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
    'handlebars',
    'linq',
    'lodash',
    'moment',
    'q',
    'uglify-js'
];

var templates = {};

templates.javascriptExceptionWithStackTrace = Handlebars.compile("<?xml version=\"1.0\" encoding=\"utf-8\"?>\
<HTML><HEAD>\
<STYLE type=\"text/css\">\
#content{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}\
BODY{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}\
P{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}\
PRE{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}\
.heading1{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; PADDING-BOTTOM: 3px; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #492B29}\
.intro{MARGIN-LEFT: -15px}</STYLE>\
<TITLE>JavaScript Error</TITLE></HEAD><BODY>\
<P class=\"heading1\">JavaScript Error</P>\
<DIV id=\"content\">\
<BR/>\
<P class=\"intro\">The JavaScript being executed on the server threw an exception ({{name}}). The exception message is '{{message}}'.</P>\
<P class=\"intro\"/>\
<P class=\"intro\">The exception stack trace is:</P>\
<P class=\"intro stackTrace\">{{{stack}}}</P>\
</DIV>\
</BODY></HTML>\
");

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
        isUndefined: util.isUndefined,
        str2buf: function (str) {
            var buf = new Buffer(str.length);
            for (var i = 0; i <= str.length; i++) {
                buf[i] = str.charCodeAt(i);
            }
            return buf;
        }
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

        var executionResult = vm.runInNewContext(data.code, vm.createContext(sandbox), data.path);

        if (_.isNull(baristaContext.response.content) || _.isUndefined(baristaContext.response.content))
            baristaContext.response.content = executionResult;

        if (Buffer.isBuffer(baristaContext.response.content)) {
            if (!baristaContext.response.contentType)
                baristaContext.response.contentType = "application/octet-stream";
            baristaContext.response.content = baristaContext.response.content.toString('base64');
        }
        else {
            if (!baristaContext.response.contentType)
                baristaContext.response.contentType = "application/json";

            var resultString = JSON.stringify(baristaContext.response.content);
            if (_.isUndefined(resultString))
                resultString = "\"undefined\"";

            baristaContext.response.content = sandbox.barista.str2buf(resultString).toString('base64');
        }

        var resultData = {
            response: JSON.stringify(baristaContext.response),
        };

        callback(null, resultData);
    } catch (ex) {
        //When a javascript error occurs, output a purty message.
        baristaContext.response.statusCode = 400;
        baristaContext.response.contentType = "text/html";

        if (!_.isError(ex)) {
            ex = new Error(ex);
        } else {
            ex.stack = ex.stack.replace(/(?:\r\n|\r|\n)/g, '<br />');
        }

        baristaContext.response.content = sandbox.barista.str2buf(templates.javascriptExceptionWithStackTrace(ex)).toString('base64');

        var resultData = {
            response: JSON.stringify(baristaContext.response),
        };

        callback(null, resultData);
    }
};