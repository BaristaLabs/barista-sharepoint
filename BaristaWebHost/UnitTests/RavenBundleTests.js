module("Barista Raven Bundle");

asyncTest("Create Document", function () {
    var scriptPath = "~/UnitTests/API/Raven/createDocument.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Create Document With Document Store Initialized with Configuration Object", function () {
    var scriptPath = "~/UnitTests/API/Raven/initializeDocumentStoreWithConfigurationObject.js";

    Barista.runTestScript(scriptPath);
});