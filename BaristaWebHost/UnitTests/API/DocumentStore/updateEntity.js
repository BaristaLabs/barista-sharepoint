require("Unit Testing");
var ds = require('Document Store');

var containerTitle = chance.sentence({ words: 2 });
var containerDescription = chance.sentence({ words: 7 });

var container = ds.createContainer(containerTitle, containerDescription);
ds.containerTitle = containerTitle;

var entity = ds.createEntity("This is my entity", "http://someurl/", "asdfasdfasdf");
entity.title = "This is my new title";

var updatedEntity = ds.updateEntity(entity);

assert.areNotEqual(updatedEntity.title, "This is my entity", "The updated entity and the original entity should not have the same titles.");
assert.areNotEqual(entity.modified, updatedEntity.modified, "The updated entity and the original entity should not have the same modified date.");
assert.areEqual(entity.data, updatedEntity.data, "The updated entity and the original entity should have the same content.");

true;