var r = require("Sucralose");
require("Unit Testing");

assert.areDeepEqual(Object.merge({ foo: 'bar' }, { broken: 'wear' }), { foo: 'bar', broken: 'wear' }, 'Object.merge | basic');
assert.areDeepEqual(Object.merge({ foo: 'bar' }, 'aha'), { foo: 'bar' }, 'Object.merge | will not merge a string');
assert.areDeepEqual(Object.merge({ foo: 'bar' }, null), { foo: 'bar' }, 'Object.merge | merge null');
assert.areDeepEqual(Object.merge({}, {}), {}, 'Object.merge | merge multi empty');


assert.areDeepEqual(Object.merge({ foo: 'bar' }, 8), { foo: 'bar' }, 'Object.merge | merge number');

assert.areDeepEqual(Object.merge({ foo: 'bar' }, 'wear', 8, null), { foo: 'bar' }, 'Object.merge | merge multi invalid', { mootools: { foo: 'bar', wear: 7 } });
assert.areDeepEqual(Object.merge([1, 2, 3, 4], [4, 5, 6]), [4, 5, 6, 4], 'Object.merge | arrays should also be mergeable');
assert.areDeepEqual(Object.merge({ foo: { one: 'two' } }, { foo: { two: 'three' } }, true, true), { foo: { one: 'two', two: 'three' } }, 'Object.merge | accepts deep merges');

assert.areDeepEqual(Object.merge('foo', 'bar'), 'foo', 'Object.merge | two strings');

assert.areDeepEqual(Object.merge({ a: 1 }, { a: 2 }), { a: 2 }, 'Object.merge | incoming wins');
assert.areDeepEqual(Object.merge({ a: 1 }, { a: 2 }), { a: 2 }, 'Object.merge | incoming wins | params true');
assert.areDeepEqual(Object.merge({ a: 1 }, { a: 2 }, false, false), { a: 1 }, 'Object.merge | target wins');
assert.areDeepEqual(Object.merge({ a: undefined }, { a: 2 }), { a: 2 }, 'Object.merge | existing but undefined properties are overwritten');
assert.areDeepEqual(Object.merge({ a: null }, { a: 2 }), { a: 2 }, 'Object.merge | null properties are not overwritten');
assert.areDeepEqual(Object.merge({ a: undefined }, { a: 2 }, false, false), { a: 2 }, 'Object.merge | false |existing but undefined properties are overwritten');
assert.areDeepEqual(Object.merge({ a: null }, { a: 2 }, false, false), { a: null }, 'Object.merge | false | null properties are not overwritten');
assert.areDeepEqual(Object.merge([{ foo: 'bar' }], [{ moo: 'car' }], true, true), [{ foo: 'bar', moo: 'car' }], 'Object.merge | can merge arrays as well');

var fn1 = function () { };
fn1.foo = 'bar';
assert.areDeepEqual(Object.merge(function () { }, fn1).foo, 'bar', 'Object.merge | retains properties');

var fn = function (key, a, b) {
    assert.areDeepEqual(key, 'a', 'Object.merge | resolve function | first argument is the key');
    assert.areDeepEqual(a, 1, 'Object.merge | resolve function | second argument is the target val');
    assert.areDeepEqual(b, 2, 'Object.merge | resolve function | third argument is the source val');
    assert.areDeepEqual(this, { a: 2 }, 'Object.merge | resolve function | context is the source object');
    return a + b;
};

assert.areDeepEqual(Object.merge({ a: 1 }, { a: 2 }, false, fn), { a: 3 }, 'Object.merge | function resolves');


// Issue #335

assert.areDeepEqual(Object.merge({ a: { b: 1 } }, { a: { b: 2, c: 3 } }, true, false), { a: { b: 1, c: 3 } }, 'Object.merge | two deep properties');


var fn1 = function () { return 'joe' };
var fn2 = function () { return 'moe' };
var date1 = new Date(2001, 1, 6);
var date2 = new Date(2005, 1, 6);
var inner1 = { foo: 'bar', hee: 'haw' }
var inner2 = { foo: 'car', mee: 'maw' }

var obj1 = {
    str: 'oolala',
    num: 18,
    fn: fn1,
    date: date1,
    prop1: 'next',
    inner: inner1,
    arr: [1, 2, 3, 4]
}

var obj2 = {
    str: 'foofy',
    num: 67,
    fn: fn2,
    date: date2,
    prop2: 'beebop',
    inner: inner2,
    arr: [4, 5, 6]
}

var fn = function (key, a, b) {
    if (key == 'str') {
        return 'conflict!';
    } else if (key == 'num') {
        return a + b;
    } else {
        return b;
    }
}

var expected = {
    str: 'conflict!',
    num: 85,
    fn: fn2,
    date: date2,
    prop1: 'next',

    inner: {
        foo: 'car',
        hee: 'haw',
        mee: 'maw'
    },
    arr: [4, 5, 6, 4],
    prop2: 'beebop'
}


assert.areDeepEqual(Object.merge(obj1, obj2, true, fn), expected, 'Object.merge | complex objects with resolve function');
"";