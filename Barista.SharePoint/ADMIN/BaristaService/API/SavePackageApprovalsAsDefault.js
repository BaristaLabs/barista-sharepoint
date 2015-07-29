var web = require("Web");
var sp = require("SharePoint");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var serviceApplication = sp.farm.getServiceApplicationById(serviceApplicationId);
if (!serviceApplication)
    throw "A service application with the specified id could not be located. " + serviceApplicationId;

var packageApprovals;
if (serviceApplication.propertyBag.containsKey("BaristaPackageApprovals")) {
    packageApprovals = serviceApplication.propertyBag.getValueByKey("BaristaPackageApprovals");
    packageApprovals = JSON.parse(packageApprovals);
}
else {
    packageApprovals = [];
}

sp.farm.setFarmKeyValue("BaristaPackageApprovals", packageApprovals);