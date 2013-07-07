module("Barista Document Store");

asyncTest("Create Container", function () {

    var name = chance.sentence({ words: 2 });
    var description = chance.sentence({ words: 7 });

    var code = "var ds = require('Document Store');\
    ds.createContainer('" + name +"', '" + description + "');";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: Barista.getBaristaServiceUrl(),
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.title === name, "Created Container Title Did Not Match.");
        ok(data.description === description, "Created Container Description Did Not Match.");
        ok(data.entityCount === 0, "Created container should not have any entities..");
        start();
    });

    request.fail(function (jqXHR, textStatus) {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Create Entity", function () {

    var scriptPath = "~/UnitTests/API/DocumentStore/createEntity.js";

    Barista.runTestScript(scriptPath);
});