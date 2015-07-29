var web = require("Web");
var sp = require("SharePoint");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var packages = barista.listPackages();

var serviceApplication = sp.farm.getServiceApplicationById(serviceApplicationId);
if (!serviceApplication)
    throw "A service application with the specified id could not be located. " + serviceApplicationId;

var approvals;
if (serviceApplication.propertyBag.containsKey("BaristaPackageApprovals")) {
    approvals = serviceApplication.propertyBag.getValueByKey("BaristaPackageApprovals");
    approvals = JSON.parse(approvals);
}
else {
    approvals = [];
}

var result = [];
var keys = Object.getOwnPropertyNames(packages);
keys.forEach(function (key) {
    if (approvals.hasOwnProperty(key))
        packages[key].approval = approvals[key];
    else
        packages[key].approval = { approvalLevel: "notApproved" };

    packages[key].packageInfo = barista.getPackageInfo(key);

    if (!key.startsWith("!")) {
        var def = packages[key];
        def.id = key;
        result.push(def);
    }
});

result;