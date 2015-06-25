require("Unit Testing");
var index = require("Barista Search Index");


index.indexName = "Test";

var result = index.doesIndexExist();

assert.isTrue(result, "Test index cannot be located.");
result;