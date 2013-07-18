require("Unit Testing");
require("Linq");
var ds = require('Document Store');

var containerTitle = chance.sentence({ words: 2 });
var containerDescription = chance.sentence({ words: 7 });

var container = ds.createContainer(containerTitle, containerDescription);
ds.containerTitle = containerTitle;

var entity = ds.createEntity("This is my entity", "http://someurl/", "asdfasdfasdf");

var entities = ds.listEntities();

assert.isTrue(entities.length > 0, "At least one entity should be listed.");
var result = Enumerable.From(entities).SingleOrDefault(function(e) {
    return e.id == entity.id;
});

assert.isNotNull(result, "The entity should be contained in the list results.");

true;