module("Barista Active Directory");

asyncTest("Get Current ADUser", function () {

    var code = "var ad = require(\"Active Directory\");\
    ad.getADUser();";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: Barista.getBaristaServiceUrl(),
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data != null, "Current AD User was returned..");
        start();
    });

    request.fail(function (jqXHR, textStatus) {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});