require("Unit Testing");
var ds = require('Document Store');

var containerTitle = chance.sentence({ words: 2 });
var containerDescription = chance.sentence({ words: 7 });

var container = ds.createContainer(containerTitle, containerDescription);
ds.containerTitle = containerTitle;

var entity = ds.createEntity("This is my entity", "http://someurl/", "asdfasdfasdf");

assert.areEqual(entity.title, "This is my entity", "Entity title must match");
assert.areEqual(entity.namespace.ToString(), "http://someurl/", "Entity namespace must match");
assert.areEqual(entity.data, "asdfasdfasdf", "Entity data must match.");
true;