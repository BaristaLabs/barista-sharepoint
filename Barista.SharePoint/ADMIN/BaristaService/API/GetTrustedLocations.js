var web = require("Web");
var sp = require("SharePoint");
require("Linq");

var trustedLocations = sp.farm.getFarmKeyValue("BaristaTrustedLocations");
if (typeof (trustedLocations) === "undefined" || trustedLocations == null)
    trustedLocations = [];

trustedLocations;