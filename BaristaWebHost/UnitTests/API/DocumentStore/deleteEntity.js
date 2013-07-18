require("Unit Testing");
var ds = require('Document Store');

var containerTitle = chance.sentence({ words: 2 });
var containerDescription = chance.sentence({ words: 7 });

var container = ds.createContainer(containerTitle, containerDescription);
ds.containerTitle = containerTitle;

var entity = ds.createEntity("This is my entity", "http://someurl/", "asdfasdfasdf");

var delResult = ds.deleteEntity(entity);

assert.isTrue(delResult, "Result should indicate that the entity was deleted.");
assert.isNull(ds.getEntity(entity.id), "Should not be able to retrieve the deleted entity.");
true;