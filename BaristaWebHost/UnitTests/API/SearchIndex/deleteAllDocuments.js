require("Unit Testing");
var index = require("Barista Search Index");

index.indexName = "Test";

index.index({ "@id": chance.guid(), hello: "world" });
index.index({ "@id": chance.guid(), hello: "world" });

var allDocsQuery = index.createMatchAllDocsQuery();
var allDocs = index.search(allDocsQuery);

index.deleteAllDocuments();

var allDocsAfterDelete = index.search(allDocsQuery);

assert.isTrue(allDocs.length >= 2, "Items were not added to the index.");
assert.isTrue(allDocsAfterDelete.length == 0, "There should be no items in the index.");

allDocsAfterDelete;