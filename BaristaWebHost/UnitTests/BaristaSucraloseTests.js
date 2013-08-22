module("Barista Sucralose");

asyncTest("Object - Merge", function () {
    var scriptPath = "~/UnitTests/API/Sucralose/object-merge.js";

    Barista.runTestScript(scriptPath);
});