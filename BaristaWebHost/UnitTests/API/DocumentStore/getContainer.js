var ds = require('Document Store');
require("Linq");
require("Unit Testing");

var name = chance.sentence({ words: 2 });
var description = chance.sentence({ words: 7 });

var c = ds.createContainer(name, description);

var container = ds.getContainer(name);

assert.isNotNull(container, "Newly created container should have been retrieved");
assert.areEqual(container.title, name, "Container and retrieved container should have identical titles");
assert.areEqual(container.description, description, "Container and retrieved container should have identical descriptions");
assert.isNotNull(container.modifiedBy, "Retrieved container should specify the user who last modified the container.");
"";