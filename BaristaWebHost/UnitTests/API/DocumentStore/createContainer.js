var ds = require('Document Store');
require("Unit Testing");

var name = chance.sentence({ words: 2 });
var description = chance.sentence({ words: 7 });

var c = ds.createContainer(name, description);
assert.areEqual(c.title, name, "Container Name should match.");
assert.areEqual(c.description, description, "Container Description Should Match.");
"";