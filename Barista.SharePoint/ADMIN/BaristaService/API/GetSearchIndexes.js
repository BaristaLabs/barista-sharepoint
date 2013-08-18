var web = require("Web");
var sp = require("SharePoint");
require("Linq");

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

var searchIndexes = sp.farm.getFarmKeyValue("BaristaSearchIndexDefinitions");

if (typeof (searchIndexes) === "undefined" || searchIndexes == null)
    searchIndexes = [];

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