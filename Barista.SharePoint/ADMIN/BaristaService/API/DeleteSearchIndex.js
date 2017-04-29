var web = require("Web");
var sp = require("SharePoint");
require("Linq");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var newSearchIndex = web.request.getBodyObject();

//Validation
if (typeof (newSearchIndex) === "undefined")
    throw "A search index was not contained in the body of the POST."

if (typeof (newSearchIndex.name) === "undefined")
    throw "The specified search index did not define a 'name' property";

var serviceApplication = sp.farm.getServiceApplicationById(serviceApplicationId);
if (!serviceApplication)
    throw "A service application with the specified id could not be located. " + serviceApplicationId;

var searchIndexes;
if (serviceApplication.propertyBag.containsKey("BaristaSearchIndexDefinitions")) {
    searchIndexes = serviceApplication.propertyBag.getValueByKey("BaristaSearchIndexDefinitions");
    searchIndexes = JSON.parse(searchIndexes);
}
else {
    searchIndexes = [];
}

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
        serviceApplication.propertyBag.setValueByKey("BaristaSearchIndexDefinitions", JSON.stringify(searchIndexes));
        serviceApplication.update();
        result = true;
    }
}

result;