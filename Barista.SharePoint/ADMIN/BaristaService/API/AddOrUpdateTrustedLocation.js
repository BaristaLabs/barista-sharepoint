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
    throw "The specified new trusted location did not define a 'Url' property";

if (typeof(newTrustedLocation.Description) === "undefined")
    newTrustedLocation.Description = "";

if (typeof(newTrustedLocation.LocationType) === "undefined")
    newTrustedLocation.LocationType = "Web";

if (typeof(newTrustedLocation.TrustChildren) === "undefined")
    newTrustedLocation.TrustChildren = false;

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
if (currentUrl == null) {
    var newValue = {
        "Url": newTrustedLocation.Url,
        "Description": newTrustedLocation.Description,
        "LocationType": newTrustedLocation.LocationType,
        "TrustChildren": newTrustedLocation.TrustChildren
    };
    trustedLocations.splice(0, 0, newValue);
    result = true;
}
else {
    currentUrl.Description = newTrustedLocation.Description;
    currentUrl.LocationType = newTrustedLocation.LocationType;
    currentUrl.TrustChildren = newTrustedLocation.TrustChildren;
    result = true;
}

serviceApplication.propertyBag.setValueByKey("BaristaTrustedLocations", JSON.stringify(trustedLocations));
//serviceApplication.update();
result;