require("Unit Testing");
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

//Index a bunch of items at a wack.
index.index(items);

//Search for a single term.
var toRetrieveIndex = chance.natural({ min: 0, max: numberOfItems - 1 });
var itemToRetrieve = items[toRetrieveIndex];

var termQuery = index.createTermQuery("hello", itemToRetrieve.hello);
var results = index.search(termQuery);

assert.isNotNull(results, "The specified item was not retrieved from the index.");
assert.isTrue(results.length > 0, "The specified item was not retrieved as part of the search results.");

results;