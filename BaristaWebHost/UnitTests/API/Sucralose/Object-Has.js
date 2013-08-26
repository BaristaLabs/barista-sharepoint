var r = require("Sucralose");
require("Unit Testing");

// Object.has

assert.areDeepEqual(Object.has({ foo: 'bar' }, 'foo'), true, 'Object.has | finds a property');
assert.areDeepEqual(Object.has({ foo: 'bar' }, 'baz'), false, 'Object.has | does not find a nonexistant property');
assert.areDeepEqual(Object.has({ hasOwnProperty: true, foo: 'bar' }, 'foo'), true, 'Object.has | local hasOwnProperty is ignored');
"";