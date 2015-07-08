var web = require("Web");
var sp = require("SharePoint");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var serviceApplication = sp.farm.getServiceApplicationById(serviceApplicationId);
if (!serviceApplication)
    throw "A service application with the specified id could not be located. " + serviceApplicationId;

var trustedLocations;
if (serviceApplication.propertyBag.containsKey("BaristaTrustedLocations")) {
    trustedLocations = serviceApplication.propertyBag.getValueByKey("BaristaTrustedLocations");
    trustedLocations = JSON.parse(trustedLocations);
}
else {
    trustedLocations = [];
}

trustedLocations;