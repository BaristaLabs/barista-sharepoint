var web = require("Web");
var sp = require("SharePoint");
require("Linq");

var newTrustedLocation = web.request.getBodyObject();

//Validation
if (typeof(newTrustedLocation) === "undefined")
    throw "A trusted location was not contained in the body of the POST."

if (typeof(newTrustedLocation.Url) === "undefined")
    throw "The specified trusted location did not define a 'Url' property";

var trustedLocations = sp.farm.getFarmKeyValue("BaristaTrustedLocations");

if (typeof(trustedLocations) === "undefined")
    trustedLocations = [];

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
        sp.farm.setFarmKeyValue("BaristaTrustedLocations", trustedLocations);
        result = true;
    }
}

result;