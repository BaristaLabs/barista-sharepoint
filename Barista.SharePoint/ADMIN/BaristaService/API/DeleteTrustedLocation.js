var web = require("Web");
var sp = require("SharePoint");
require("Linq");

var serviceApplicationId = web.request.queryString.serviceApplicationId;
if (!serviceApplicationId)
    throw "A Service Application Id must be specified in the query string.";

var newTrustedLocation = web.request.getBodyObject();

//Validation
if (typeof(newTrustedLocation) === "undefined")
    throw "A trusted location was not contained in the body of the POST."

if (typeof(newTrustedLocation.Url) === "undefined")
    throw "The specified trusted location did not define a 'Url' property";

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

var currentUrl = Enumerable.From(trustedLocations)
                           .Where(function (trustedLocation) {
                                if (typeof (trustedLocation.Url) !== "undefined" && newTrustedLocation.Url == trustedLocation.Url)
                                    return true;
                                else
                                    return false;
                           })
                           .FirstOrDefault();

var result = false;
if (currentUrl != null) {
    var index = trustedLocations.indexOf(currentUrl);
    if (index != -1) {
        trustedLocations.splice(index, 1);
        serviceApplication.propertyBag.setValueByKey("BaristaTrustedLocations", JSON.stringify(trustedLocations));
        //serviceApplication.update();
        result = true;
    }
}

result;