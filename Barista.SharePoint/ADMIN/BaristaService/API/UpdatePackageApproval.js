var web = require("Web");
var sp = require("SharePoint");
require("Linq");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var packageApproval = web.request.getBodyObject();

//Validation
if (!packageApproval)
    throw "A packageApproval was not contained in the body of the POST."

if (!packageApproval.id)
    throw "The specified package approval did not define a 'id' property";

var serviceApplication = sp.farm.getServiceApplicationById(serviceApplicationId);
if (!serviceApplication)
    throw "A service application with the specified id could not be located. " + serviceApplicationId;

var approvals;
if (serviceApplication.propertyBag.containsKey("BaristaPackageApprovals")) {
    approvals = serviceApplication.propertyBag.getValueByKey("BaristaPackageApprovals");
    approvals = JSON.parse(approvals);
}
else {
    approvals = {};
}

var currentPackageApproval = approvals[packageApproval.id];

var result = false;
if (!currentPackageApproval) {
    var newApproval = {
        "approvalLevel": packageApproval.approval.approvalLevel,
        "lastDateModified": new Date()
    };
    approvals[packageApproval.id] = newApproval;
    result = true;
}
else {
    currentPackageApproval.approvalLevel = packageApproval.approval.approvalLevel;
    currentPackageApproval.lastDateModified = new Date();
    result = true;
}

serviceApplication.propertyBag.setValueByKey("BaristaPackageApprovals", JSON.stringify(approvals));
serviceApplication.update();
result;