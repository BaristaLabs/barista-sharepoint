require("Unit Testing");
var ds = require('Document Store');

var containerTitle = chance.sentence({ words: 2 });
var containerDescription = chance.sentence({ words: 7 });

var container = ds.createContainer(containerTitle, containerDescription);
ds.containerTitle = containerTitle;

var entity = ds.createEntity("This is my entity", "http://someurl/", "asdfasdfasdf");

//Test retrieving an entity by the object.
var entity2 = ds.getEntity(entity);

assert.areEqual(entity.id.ToString(), entity2.id.ToString(), "Created entity and entity retrieved via object must match.");

var entity3 = ds.getEntity(entity.id);

assert.areEqual(entity.id.ToString(), entity3.id.ToString(), "Created entity and entity retrieved via id must match.");

true;