var web = require("Web");
var sp = require("SharePoint");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var serviceApplication = sp.farm.getServiceApplicationById(serviceApplicationId);
if (!serviceApplication)
    throw "A service application with the specified id could not be located. " + serviceApplicationId;

var indexDefinitions;
if (serviceApplication.propertyBag.containsKey("BaristaSearchIndexDefinitions")) {
    indexDefinitions = serviceApplication.propertyBag.getValueByKey("BaristaSearchIndexDefinitions");
    indexDefinitions = JSON.parse(indexDefinitions);
}
else {
    indexDefinitions = [];
}

sp.farm.setFarmKeyValue("BaristaSearchIndexDefinitions", indexDefinitions);