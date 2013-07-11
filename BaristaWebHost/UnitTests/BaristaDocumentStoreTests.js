module("Barista Document Store");

asyncTest("Create Container", function () {
    var scriptPath = "~/UnitTests/API/DocumentStore/createContainer.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("List Containers", function () {
    var scriptPath = "~/UnitTests/API/DocumentStore/listContainers.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Delete Container", function () {
    var scriptPath = "~/UnitTests/API/DocumentStore/deleteContainer.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Get Container", function () {
    var scriptPath = "~/UnitTests/API/DocumentStore/getContainer.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Create Entity", function () {
    var scriptPath = "~/UnitTests/API/DocumentStore/createEntity.js";

    Barista.runTestScript(scriptPath);
});