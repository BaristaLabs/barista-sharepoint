var trustedLocations = sp.farm.getFarmKeyValue("BaristaTrustedLocations");
if (typeof (trustedLocations) === "undefined" || trustedLocations == null)
    trustedLocations = [];

trustedLocations;