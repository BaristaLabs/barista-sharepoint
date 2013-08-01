require("Unit Testing");
var index = require("Barista Search Index");

index.indexName = "Test";

//Clear the index...
index.deleteAllDocuments();

//Plunk in a bunch of items.
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
    index.index(newItem);
    items.push(newItem);
}

var allDocsQuery = index.createMatchAllDocsQuery();
var results = index.search(allDocsQuery);

assert.isTrue(results.length == numberOfItems, "All the items should have been added to the index.");
assert.isTrue(results[0].luceneDocId == 0, "First item in the index should have an lucene id of 0.");
assert.areEqual(items[0]["@id"], results[0].document.documentId, "Items should have identical ids.");
assert.areEqual(items[0]["@metadata"]["tag"], results[0].document.metadata.tag, "Items should have identical metadata.");
assert.areEqual(items[0]["hello"], results[0].document.data.hello, "Items should have identical data.");

results;