module("Barista Raven Bundle");

asyncTest("ConnectToDocumentStore", function () {
    var scriptPath = "~/UnitTests/API/Raven/createContainer.js";

    Barista.runTestScript(scriptPath);
});