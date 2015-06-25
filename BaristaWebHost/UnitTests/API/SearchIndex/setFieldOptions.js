require("Unit Testing");
require("Linq");
var index = require("Barista Search Index");

index.indexName = "Test";

//Clear the index...
index.deleteAllDocuments();

//Set a field option.
index.setFieldOptions({
    fieldName: "stateDescription",
    index: "NotAnalyzed"
});

//Set a field option from an array.
index.setFieldOptions([{
    fieldName: "a",
    index: "NotAnalyzed"
},
{
    fieldName: "b",
    index: "NotAnalyzed"
}]);

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
    stateDescription: "Hawaii is a state",
    a: "Mister Bojangles",
    b: "Pine Cone",
    c: "Outer Banks",
});

//Index the items at a wack.
index.index(items);

//Perform a term query with our non-analyzed fields.
var query = index.createTermQuery("stateDescription", "Hawaii is a state");
var results = index.search(query);

//We should have one result.
assert.isTrue(results.length == 1, "One result should be returned from the term query on a non-analyzed field.");


//Perform a term query with the a field that we set a field option via an array.
query = index.createTermQuery("a", "Mister Bojangles");
results = index.search(query);

//We should have one result.
assert.isTrue(results.length == 1, "One result should be returned from the term query on a non-analyzed field.");

//Perform a term query on an analyzed field.
var query = index.createTermQuery("c", "Outer Banks");
var results = index.search(query);

//We should not have any results.
assert.isTrue(results.length == 0, "Fields should be analyzed by default.");


results;