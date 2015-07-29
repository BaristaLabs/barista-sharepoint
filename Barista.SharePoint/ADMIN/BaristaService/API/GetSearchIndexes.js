var web = require("Web");
var sp = require("SharePoint");
require("Linq");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

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

var result = Enumerable.From(searchIndexes)
    .Select(function (si) {
        var directoryTypeName = Enumerable.From(knownDirectoryTypes)
            .Where(function (f) { return si.typeName == f.Value })
            .FirstOrDefault();

        return {
            name: si.name,
            description: si.description,
            indexType: directoryTypeName.Name,
            indexStoragePath: si.indexStoragePath
        }
    })
    .ToArray();

result;