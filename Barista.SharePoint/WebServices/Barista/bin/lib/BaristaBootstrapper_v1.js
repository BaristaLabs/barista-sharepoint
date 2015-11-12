var vm = require('vm');
var _ = require('lodash');
var util = require('util');

var expose = [
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

    //Define the require remap in our sandbox
    sandbox.require = function (id) {

        if (_.includes(whitelist, id)) {
            return require(id);
        }

        var baristaContext = {
            requestContext: JSON.parse(data.context.request),
            environment: JSON.parse(data.context.environment)
        };

        if (id === "barista-context") {
            return baristaContext;
        }

        //Define the packages that was there previously.
        if (id === "SharePoint") {
            return require("./../lib/SharePoint_v1.js")(baristaContext);
        }

        return require("./../lib/SharePoint_Require_v1.js")(baristaContext, id);
    };

    try {
        var result = vm.runInNewContext(data.code, vm.createContext(sandbox), data.path);
        callback(null, result);
    } catch (e) {
        callback(e.stack, result);
    }
};