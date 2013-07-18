module("Barista Active Directory");

asyncTest("Get Current User", function () {
    var scriptPath = "~/UnitTests/API/ActiveDirectory/getCurrentUser.js";

    Barista.runTestScript(scriptPath);
});