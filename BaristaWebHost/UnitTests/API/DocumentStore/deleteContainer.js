var ds = require('Document Store');
require("Linq");
require("Unit Testing");

var name = chance.sentence({ words: 2 });
var description = chance.sentence({ words: 7 });

var c = ds.createContainer(name, description);

var containers = ds.listContainers();
var createdContainer = Enumerable.From(containers)
    .Where(function (c) { return c.title == name; })
    .FirstOrDefault();

assert.isNotNull(createdContainer, "Newly created container should be contained in the list of containers.");


ds.deleteContainer(name);

containers = ds.listContainers();
createdContainer = Enumerable.From(containers)
    .Where(function (c) { return c.title == name; })
    .FirstOrDefault();

assert.isNull(createdContainer, "Deleted container should NOT be contained in the list of containers.");

createdContainer === null;