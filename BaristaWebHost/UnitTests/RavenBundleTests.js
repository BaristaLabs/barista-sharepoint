module("Barista Raven Bundle");

asyncTest("Create Document", function () {
    var scriptPath = "~/UnitTests/API/Raven/createDocument.js";

    Barista.runTestScript(scriptPath);
});