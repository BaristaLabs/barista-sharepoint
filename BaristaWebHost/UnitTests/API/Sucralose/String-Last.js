var r = require("Sucralose");
require("Unit Testing");

assert.areDeepEqual('quack'.last(), 'k', 'String#last | last character');
assert.areDeepEqual('quack'.last(2), 'ck', 'String#last | last 2 characters');
assert.areDeepEqual('quack'.last(3), 'ack', 'String#last | last 3 characters');
assert.areDeepEqual('quack'.last(4), 'uack', 'String#last | last 4 characters');
assert.areDeepEqual('quack'.last(10), 'quack', 'String#last | last 10 characters');
assert.areDeepEqual('quack'.last(-1), '', 'String#last | last -1 characters');
assert.areDeepEqual('quack'.last(-5), '', 'String#last | last -5 characters');
assert.areDeepEqual('quack'.last(-10), '', 'String#last | last -10 characters');
assert.areDeepEqual('fa'.last(3), 'fa', 'String#last | last 3 characters');
"";