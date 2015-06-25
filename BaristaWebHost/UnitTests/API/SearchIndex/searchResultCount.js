require("Unit Testing");
require("Linq");
var index = require("Barista Search Index");

index.indexName = "Test";

//Clear the index...
index.deleteAllDocuments();

//Create a bunch of items.
var numberOfItems = chance.d20()
var items = [];
for (var i = 0; i < numberOfItems; i++) {
    var newItem = {
        "@id": chance.guid(),
        "@metadata": {
            "tag": chance.sentence()
        },
        hello: chance.word()
    };
    items.push(newItem);
}

//Add a new, known item.
items.push({
    "@id": chance.guid(),
    "@metadata": {
        "tag": "This stuff is bananas"
    },
    hello: "Hawaii"
});

//Index the items at a wack.
index.index(items);

//Get the total documents in the index.
var resultCount = index.searchResultCount(index.createMatchAllDocsQuery());

assert.isNotNull(resultCount, "Search Result Count did not return a value.");
assert.isTrue(resultCount == items.length, "Search result count should match the count of the items inserted.");

resultCount;