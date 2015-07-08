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
    throw "The specified new search index did not define a 'name' property";

if (typeof (newSearchIndex.description) === "undefined")
    newSearchIndex.description = "";

if (typeof (newSearchIndex.indexType) === "undefined")
    newSearchIndex.indexType = "RAM Directory";

var knownDirectoryTypes = [{
    Name: "RAM Directory",
    Value: "Lucene.Net.Store.RAMDirectory, Lucene.Net, Version=3.0.3.0, Culture=neutral, PublicKeyToken=85089178b9ac3181",
}, {
    Name: "File Directory",
    Value: "Lucene.Net.Store.SimpleFSDirectory, Lucene.Net, Version=3.0.3.0, Culture=neutral, PublicKeyToken=85089178b9ac3181",
}, {
    Name: "SharePoint Directory",
    Value: "Barista.SharePoint.Search.SPDirectory, Barista.SharePoint.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52",
}];

var searchIndexType = Enumerable.From(knownDirectoryTypes)
    .Where(function(f) { return f.Name == newSearchIndex.indexType; })
    .FirstOrDefault();

if (searchIndexType == null)
    throw "The index type of the new search index could not be determined.";

if (typeof (newSearchIndex.indexStoragePath) === "undefined")
    newSearchIndex.indexStoragePath = "";

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
if (currentSearchIndex == null) {
    var newValue = {
        "name": newSearchIndex.name,
        "description": newSearchIndex.description,
        "typeName": searchIndexType.Value,
        "indexStoragePath": newSearchIndex.indexStoragePath
    };
    searchIndexes.splice(0, 0, newValue);
    result = true;
}
else {
    currentSearchIndex.description = newSearchIndex.description;
    currentSearchIndex.typeName = searchIndexType.Value;
    currentSearchIndex.indexStoragePath = newSearchIndex.indexStoragePath;
    result = true;
}

serviceApplication.propertyBag.setValueByKey("BaristaSearchIndexDefinitions", JSON.stringify(searchIndexes));
serviceApplication.update();
result;