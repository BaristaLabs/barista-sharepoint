var r = require("Sucralose");
require("Unit Testing");

// Object.has

assert.areDeepEqual((4).toNumber(), 4, 'Number#toNumber | 4 is 4');
assert.areDeepEqual((10000).toNumber(), 10000, 'Number#toNumber | 10000 is 10000');
assert.areDeepEqual((5.2345).toNumber(), 5.2345, 'Number#toNumber | 5.2345 is 5.2345');

"";