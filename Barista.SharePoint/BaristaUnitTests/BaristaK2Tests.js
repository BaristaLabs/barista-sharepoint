module("Barista K2");

asyncTest("Get K2 Worklist", function () {

    var code = "var k2 = require(\"K2\");\
        k2.servicesBaseUrl = '" + getK2ServiceUrl() + "'; \
        k2.openWorklist(true);";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(1 == 1, "Call to service succeeded.");
        start();
    });

    request.fail(function (jqXHR, textStatus) {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});