var web = require("Web");
var sp = require("SharePoint");

var result = new Array();
var calls = new Array();

var webs = sp.currentContext.web.getWebs();
webs.forEach(function (w) {
    result.push(w.url);
    var deferred = web.ajax(w.url + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent("/Content/Barista_RecursiveWebsRetrieval.js"), { useDefaultCredentials: true, async: true });
    deferred.done(function (data) {
        if (data.length > 0) {
            data.forEach(function (url) {
                result.push(url);
            });
        }
    });
    calls.push(deferred);
});

waitAll(calls);

result;