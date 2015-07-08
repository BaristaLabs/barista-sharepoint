var web = require("Web");
var sp = require("SharePoint");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var customBundles = barista.listCustomBundles();

var serviceApplication = sp.farm.getServiceApplicationById(serviceApplicationId);
if (!serviceApplication)
    throw "A service application with the specified id could not be located. " + serviceApplicationId;

var approvals;
if (serviceApplication.propertyBag.containsKey("BaristaCustomBundleApprovals")) {
    approvals = serviceApplication.propertyBag.getValueByKey("BaristaCustomBundleApprovals");
    approvals = JSON.parse(trustedLocations);
}
else {
    approvals = [];
}

var keys = Object.getOwnPropertyNames(customBundles);
keys.forEach(function(key) {
    if (approvals.hasOwnProperty(key))
        customBundles[key].approval = approvals[key];
    else
        customBundles[key].approval = {};
});

customBundles;