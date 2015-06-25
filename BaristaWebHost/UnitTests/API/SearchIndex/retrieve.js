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

//Retrieve a random item, and ensure it has the values that we expect.
var toRetrieveIndex = chance.natural({ min: 0, max: numberOfItems - 1 });
var itemToRetrieve = items[toRetrieveIndex];

var item = index.retrieve(itemToRetrieve["@id"]);
assert.isNotNull(item, "The specified item was not retrieved from the index.");
assert.areEqual(itemToRetrieve["@id"], item.documentId, "Items should have identical ids.");
assert.areEqual(itemToRetrieve["@metadata"]["tag"], item.metadata.tag, "Items should have identical metadata.");
assert.areEqual(itemToRetrieve["hello"], item.data.hello, "Items should have identical data.");


item;