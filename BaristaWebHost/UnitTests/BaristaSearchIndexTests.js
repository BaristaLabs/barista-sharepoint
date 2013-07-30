module("Barista Search Index");

asyncTest("Store and retrieve a Json Doc.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/storeAndRetrieve.js";

    Barista.runTestScript(scriptPath);
});