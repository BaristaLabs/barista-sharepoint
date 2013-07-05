module("Barista Document");

asyncTest("Generate PDF", function () {

    var code = "var doc = require(\"Document\");\
    doc.html2Pdf(\"<html><body>Hello, world</body></html>\");";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: Barista.getBaristaServiceUrl(),
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.indexOf("%PDF-1.4") == 0, "Expression was run and a result that contained a PDF 1.4 header was returned.");
        deepEqual(jqXHR.getResponseHeader("Content-Type"), "application/pdf", "Response content type was application/pdf");
        start();
    });

    request.fail(function (jqXHR, textStatus) {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Generate ZIP file", function () {

    var code = "var doc = require(\"Document\");\
        var web = require(\"Web\");\
        var zip = new ZipFile(); \
        zip.addFile(\"Hello-1.pdf\", doc.html2Pdf(\"<html><body>Hello, world 1</body></html>\")); \
        zip.addFile(\"Hello-2.pdf\", doc.html2Pdf(\"<html><body>Hello, world 2</body></html>\")); \
        web.response.contentType=\"application/zip\"; \
        zip.finish();";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: Barista.getBaristaServiceUrl(),
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.indexOf("PK") == 0, "Expression was run and a result that contained a zip header was returned.");
        deepEqual(jqXHR.getResponseHeader("Content-Type"), "application/zip", "Response content type was application/zip");
        start();
    });

    request.fail(function (jqXHR, textStatus) {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});