module("Barista Core");

asyncTest("Test Creating an Uri", function () {
    expect(2);
    var code = "var uri = new Uri('http://www.google.com');\
    uri;\
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data != null, "A result was returned from the service.");
        equal(data.authority, "www.google.com", "The authority of the Uri matches.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Test Including a file", function () {
    expect(2);
    var code = "include('{WebUrl}/Lists/BaristaUnitTests/Content/Barista_Include.js');\
    auBonPainCoffee;\
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data != null, "A result was returned from the service.");
        equal(data, "Crappy", "The result included data that was defined in the include file.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Test Writing to Console", function () {
    expect(2);
    var code = "\
    require(\"Deferred\");\
    var log = require(\"Unified Logging Service\");\
    console.error('A catastrophic error has occurred.');\
    delay(2000);\
    var util = require('Utility');\
    var correlationId = util.getCurrentCorrelationId();\
    var logEntries = log.getLocalLogEntries(correlationId);\
    logEntries;\
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data != null, "A result was returned from the service.");
        var result = Enumerable.From(data).Any(function (entry) { return entry.Message == "A catastrophic error has occurred."; });
        ok(result == true, "The console message was written to the Uls Log.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Test retriving query string parameters", function () {
    expect(4);
    var code = "var web = require('Web');\
    var queryString = web.request.queryString;\
    var result = { foo: queryString['foo'], bar: queryString['bar'], baz: queryString.baz };\
    result;\
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?foo=hello&bar=world&baz=prizes",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data != null, "A result was returned from the service.");
        equal("hello", data.foo, "Foo was correct.");
        equal("world", data.bar, "Bar was correct.");
        equal("prizes", data.baz, "baz was correct.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Execute Simple Expression", function () {
    expect(1);
    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=42%2B1");

    request.done(function (data) {
        deepEqual(data, 43, "Expression was run and a correct result returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Execute Simple Expression via POST", function () {
    expect(1);

    var code = "42+1";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data) {
        deepEqual(data, 43, "Expression was run and a correct result returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Execute Simple Expression within script file", function () {
    expect(1);

    var request = jQuery.ajax({
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(getContentPath()) + "Barista_SimpleExpression.js"
    });

    request.done(function (data) {
        deepEqual(data, 43, "Expression was run and a correct result returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Execute Simple Expression via multi-part post.", function () {
    expect(1);

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "multipart/form-data",
        dataType: "json",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=42%2B1"
    });

    request.done(function (data) {
        deepEqual(data, 43, "Expression was run and a correct result returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Execute Expression with JS Objects", function () {
    expect(1);

    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=Math.random()%2B1");

    request.done(function (data) {
        ok(data > 0, "Expression was run and a correct result returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Make Ajax call using no settings", function () {
    expect(1);

    var code = "var ad = require(\"ad\"); var web = require(\"web\"); ad.getADUser();";

    var url = getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(code);
    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=var%20web%20%3D%20require(%22Web%22)%3B%20web.ajax(\"" + encodeURIComponent(url) + "\")");

    request.done(function (data) {
        ok(data != null, "The Ajax call was made and a result returned.");
        start();
    });

    request.fail(function (jqXHR, textStatus) {
        equal(jqXHR.statusText, "Bad Request", "Should require default credentials to execute the ajax request");
        start();
    });
});

asyncTest("Make Ajax call using proxy settings", function () {
    expect(3);

    var code = "var web = require(\"Web\"); web.ajax(\"http://search.twitter.com/search.atom?q=gaga&count=5\", " + JSON.stringify({ useDefaultCredentials: true, proxy: getProxy() }) + ")";
    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(code));

    request.done(function (data) {
        ok(data != null, "A response was returned from twitter.");
        if (typeof (data) !== 'undefined' && data != null) {
            ok(typeof (data.feed) !== 'undefined', "A feed object was returned from twitter.");
            ok(data.feed["entry"].length > 0, "The feed element contained multiple entries.");
        }
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Make Ajax call using default credentials", function () {
    expect(3);

    var url = getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=var%20ad%20%3D%20require(%22Active%20Directory%22)%3B%20ad.getADUser();";
    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=var%20web%20%3D%20require(%22Web%22)%3B%20web.ajax(\"" + encodeURIComponent(url) + "\", " + JSON.stringify({ useDefaultCredentials: true, proxy: getProxy() }) + ")");

    request.done(function (data) {
        ok(data != null, "A result was returned from the service.");
        if (typeof (data.name) !== 'undefined') {
            ok(data.name.length > 0, "Name property has a value.");
            ok(data.firstName.length > 0, "First Name has a value.");
        }
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Make async ajax call.", function () {
    expect(4);

    var code = "\
	require(\"Deferred\");\
    var web = require(\"Web\");\
    var calls = new Array();\
    var result = { users: [] };\
    for(var i = 0; i < 10; i++) {\
        var deferred = web.ajax(\"{WebUrl}/_vti_bin/Barista/v1/Barista.svc/eval?c=var%20ad%20%3D%20require(%22Active%20Directory%22)%3B%20ad.getADUser();\", { useDefaultCredentials: true, async: true, proxy: " + getProxy() + " }); \
        deferred.done(function(data) { result.users.push(data); });\
        deferred.fail(function() { result.completed = false; });\
        calls.push(deferred);\
    }\
    waitAll(calls);\
    result;\
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data != null, "A result was returned from the service.");
        if (data != null && typeof (data.users) !== "undefined") {
            ok(data.users.length == 10, "10 results were returned.");
            ok(data.users[0].name.length > 0, "Name property has a value.");
            ok(data.users[0].firstName.length > 0, "First Name has a value.");
        }
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Execute long-running tasks via deferreds.", function () {
    expect(1);

    var request = jQuery.ajax({
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(getContentPath()) + "Barista_Deferred.js"
    });

    request.done(function (data) {
        deepEqual(data.length, 10, "Script was executed -- 10 results were returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});