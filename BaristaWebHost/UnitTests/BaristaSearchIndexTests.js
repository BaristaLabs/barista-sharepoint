module("Barista Search Index");

asyncTest("Assert that the test index exists.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/doesIndexExist.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Store and retrieve a Json Doc.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/storeAndRetrieve.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Store and delete a Json Doc.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/storeAndDelete.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert that deleting all documents clears the index.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/deleteAllDocuments.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert that an explaination for a search result is returned.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/explanation.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert that a highlight result is generated for a search result.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/highlight.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert Inserting items into the index has expected results.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/index.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert Inserting multiple items into the index has expected results.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/indexMultiple.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert that retrieving a single item from the index has the expected results.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/retrieve.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert that searching with various queries has the expected results.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/search.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert that faceted searches have the expected results.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/facetedSearch.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert that counts are returned via search result count.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/searchResultCount.js";

    Barista.runTestScript(scriptPath);
});

asyncTest("Assert that field options can be set and the corresponding behavior is correct.", function () {
    var scriptPath = "~/UnitTests/API/SearchIndex/setFieldOptions.js";

    Barista.runTestScript(scriptPath);
});