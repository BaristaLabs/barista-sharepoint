module("Barista Document Store");

asyncTest("Create Container", function () {

    var code = "var ds = require(\"Document Store\");\
    doc.html2Pdf(\"<html><body>Hello, world</body></html>\");";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: Barista.getBaristaServiceUrl(),
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(1 == 2);
        start();
    });

    request.fail(function (jqXHR, textStatus) {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});