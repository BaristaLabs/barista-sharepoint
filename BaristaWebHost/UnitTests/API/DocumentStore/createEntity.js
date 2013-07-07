require("Unit Testing");
var ds = require('Document Store');

var containerTitle = chance.sentence({ words: 2 });
var containerDescription = chance.sentence({ words: 7 });

var container = ds.createContainer(containerTitle, containerDescription);
ds.containerTitle = containerTitle;

var entity = ds.createEntity("This is my entity", "http://someurl/", "asdfasdfasdf");

assert.is(entity.title, "This is my entity", "Entity title must match");
assert.is(entity.namespace.ToString(), "http://someurl/", "Entity namespace must match");
assert.is(entity.data, "asdfasdfasdf", "Entity data must match.");
true;