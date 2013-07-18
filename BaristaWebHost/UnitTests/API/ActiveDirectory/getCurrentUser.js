var ad = require('Active Directory');
require("Unit Testing");

var user = ad.getADUser();

assert.isNotNull(user.displayName, "Retrieved user should have a display name.");
"";