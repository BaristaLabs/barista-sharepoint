var web = require("Web");
var sp = require("SharePoint");
require("Linq");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var bundleApproval = web.request.getBodyObject();

//Validation
if (!bundleApproval)
    throw "A bundleApproval was not contained in the body of the POST."

if (!bundleApproval.name)
    throw "The specified bundle approval did not define a 'name' property";

var serviceApplication = sp.farm.getServiceApplicationById(serviceApplicationId);
if (!serviceApplication)
    throw "A service application with the specified id could not be located. " + serviceApplicationId;

var approvals;
if (serviceApplication.propertyBag.containsKey("BaristaCustomBundleApprovals")) {
    approvals = serviceApplication.propertyBag.getValueByKey("BaristaCustomBundleApprovals");
    approvals = JSON.parse(approvals);
}
else {
    approvals = [];
}

var currentBundleApproval = Enumerable.From(approvals)
                           .Where(function (bundleApproval) {
                               if (bundleApproval.name && bundleApproval.name === bundleApproval.name)
                                   return true;
                               else
                                   return false;
                           })
                           .FirstOrDefault();

var result = false;
if (currentBundleApproval == null) {
    var newApproval = {
        "name": bundleApproval.name,
        "approvalLevel": bundleApproval.approvalLevel,
        "lastDateModified": new Date()
    };
    approvals.splice(0, 0, newApproval);
    result = true;
}
else {
    currentBundleApproval.approvalLevel = bundleApproval;
    currentBundleApproval.lastDateModified = new Date();
    result = true;
}

serviceApplication.propertyBag.setValueByKey("BaristaCustomBundleApprovals", JSON.stringify(approvals));
serviceApplication.update();
result;