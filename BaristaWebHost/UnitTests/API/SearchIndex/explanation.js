require("Unit Testing");
var index = require("Barista Search Index");

index.indexName = "Test";

index.index({ "@id": chance.guid(), hello: "world" });
index.index({ "@id": chance.guid(), hello: "world" });

var allDocsQuery = index.createMatchAllDocsQuery();
var allDocs = index.search(allDocsQuery);

var explaination = index.explain(allDocsQuery, allDocs[0]);
assert.isNotNull(explaination);

assert.isTrue(explaination.isMatch == true);
assert.isTrue(explaination.value == 1);
assert.isNotNull(explaination.explanationHtml);

explaination;