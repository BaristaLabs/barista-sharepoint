var r = require("Sucralose");
require("Unit Testing");

assert.areDeepEqual(['a', 'b', 'c'].last(), 'c', 'Array#last | no argument');
assert.areDeepEqual(['a', 'b', 'c'].last(1), ['c'], 'Array#last | 1', { prototype: 'c' });
assert.areDeepEqual(['a', 'b', 'c'].last(2), ['b', 'c'], 'Array#last | 2', { prototype: 'c' });
assert.areDeepEqual(['a', 'b', 'c'].last(3), ['a', 'b', 'c'], 'Array#last | 3', { prototype: 'c' });
assert.areDeepEqual(['a', 'b', 'c'].last(4), ['a', 'b', 'c'], 'Array#last | 4', { prototype: 'c' });
assert.areDeepEqual(['a', 'b', 'c'].last(-1), [], 'Array#last | -1', { prototype: 'c' });
assert.areDeepEqual(['a', 'b', 'c'].last(-2), [], 'Array#last | -2', { prototype: 'c' });
assert.areDeepEqual(['a', 'b', 'c'].last(-3), [], 'Array#last | -3', { prototype: 'c' });
assert.areDeepEqual(['a', 'b', 'c'].last(-4), [], 'Array#last | -4', { prototype: 'c' });