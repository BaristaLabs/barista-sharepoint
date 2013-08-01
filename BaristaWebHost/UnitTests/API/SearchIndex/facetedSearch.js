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

//Search for a single term.
var toRetrieveIndex = chance.natural({ min: 0, max: items.length - 1 });
var itemToRetrieve = items[toRetrieveIndex];

var results = index.facetedSearch("hello: h*", null, ["hello"]);

assert.isNotNull(results, "Faceted Search did not return any items.");
assert.isTrue(results.length > 0, "Faceted search should return a collection of all terms for the hello field.");
var hawaiiResult = Enumerable.From(results)
.Where(function (r) { return r.facetName == "hawaii"; })
.FirstOrDefault();

assert.isTrue(hawaiiResult != null, "Hawaii should be contained within the results.");
assert.isTrue(hawaiiResult.hitCount == 1, "The facet Hawaii should have exactly one hit.");
assert.isTrue(hawaiiResult.documents[0].document.metadata.tag == "This stuff is bananas", "The facet document should contain the inserted document with metadata.");


hawaiiResult;