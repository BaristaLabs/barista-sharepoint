require("Unit Testing");
var index = require("Barista Search Index");

index.indexName = "Test";

index.index({ "@id": chance.guid(), hello: "world" });
index.index({ "@id": chance.guid(), hello: "bang" });

var termQuery = index.createTermQuery("hello", "world");
var worlds = index.search(termQuery);

var highlight = index.highlight(termQuery, worlds[0], "hello");
assert.isNotNull(highlight);
assert.areEqual(highlight, "\"<b style=\\\"background:yellow\\\">world</b>\"", "Result should have a phunky style.");

highlight;