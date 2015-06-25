module("Barista Web");

asyncTest("Get Web Request", function () {
    var scriptPath = "~/UnitTests/API/Web/getWebRequest.js";

    Barista.runTestScript(scriptPath);
});