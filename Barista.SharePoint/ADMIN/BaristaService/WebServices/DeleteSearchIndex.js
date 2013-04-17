var web = require("Web");
var sp = require("SharePoint");
require("Linq");

var newSearchIndex = web.request.getBodyObject();

//Validation
if (typeof (newSearchIndex) === "undefined")
    throw "A search index was not contained in the body of the POST."

if (typeof (newSearchIndex.name) === "undefined")
    throw "The specified search index did not define a 'name' property";

var searchIndexes = sp.farm.getFarmKeyValue("BaristaSearchIndexDefinitions");

if (typeof (searchIndexes) === "undefined")
    searchIndexes = [];

var currentSearchIndex = Enumerable.From(searchIndexes)
                           .Where(function (searchIndex) {
                                if (typeof (searchIndex.name) !== "undefined" && searchIndex.name == newSearchIndex.name)
                                    return true;
                                else
                                    return false;
                           })
                           .FirstOrDefault();

var result = false;
if (currentSearchIndex != null) {
    var index = searchIndexes.indexOf(currentSearchIndex);
    if (index != -1) {
        searchIndexes.splice(index, 1);
        sp.farm.setFarmKeyValue("BaristaSearchIndexDefinitions", searchIndexes);
        result = true;
    }
}

result;