require("Unit Testing");
var index = require("Barista Search Index");


index.indexName = "Test";

var docId = chance.guid();
index.index({ "@id": docId, hello: "world" });
var doc = index.retrieve(docId);

assert.areEqual(doc.documentId, docId, "Document Id and docId should be equal");
assert.areEqual(doc.data.hello, "world", "Document Data should be 'world'");
doc;