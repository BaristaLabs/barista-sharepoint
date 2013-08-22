var r = require("Sucralose");
require("Unit Testing");

var equiv = (function () {

    // Call the o related callback with the given arguments.
    function bindCallbacks(o, callbacks, args) {
        var prop = BaristaUnitTesting.objectType(o);
        if (prop) {
            if (BaristaUnitTesting.objectType(callbacks[prop]) === "function") {
                return callbacks[prop].apply(callbacks, args);
            } else {
                return callbacks[prop]; // or undefined
            }
        }
    }

    // the real equiv function
    var innerEquiv,
		// stack to decide between skip/abort functions
		callers = [],
		// stack to avoiding loops from circular referencing
		parents = [],
		parentsB = [],

		getProto = Object.getPrototypeOf || function (obj) {
		    /*jshint camelcase:false */
		    return obj.__proto__;
		},
		callbacks = (function () {

		    // for string, boolean, number and null
		    function useStrictEquality(b, a) {
		        /*jshint eqeqeq:false */
		        if (b instanceof a.constructor || a instanceof b.constructor) {
		            // to catch short annotation VS 'new' annotation of a
		            // declaration
		            // e.g. var i = 1;
		            // var j = new Number(1);
		            return a == b;
		        } else {
		            return a === b;
		        }
		    }

		    return {
		        "string": useStrictEquality,
		        "boolean": useStrictEquality,
		        "number": useStrictEquality,
		        "null": useStrictEquality,
		        "undefined": useStrictEquality,

		        "nan": function (b) {
		            return isNaN(b);
		        },

		        "date": function (b, a) {
		            return BaristaUnitTesting.objectType(b) === "date" && a.valueOf() === b.valueOf();
		        },

		        "regexp": function (b, a) {
		            return BaristaUnitTesting.objectType(b) === "regexp" &&
						// the regex itself
						a.source === b.source &&
						// and its modifiers
						a.global === b.global &&
						// (gmi) ...
						a.ignoreCase === b.ignoreCase &&
						a.multiline === b.multiline &&
						a.sticky === b.sticky;
		        },

		        // - skip when the property is a method of an instance (OOP)
		        // - abort otherwise,
		        // initial === would have catch identical references anyway
		        "function": function () {
		            var caller = callers[callers.length - 1];
		            return caller !== Object && typeof caller !== "undefined";
		        },

		        "array": function (b, a) {
		            var i, j, len, loop, aCircular, bCircular;

		            // b could be an object literal here
		            if (BaristaUnitTesting.objectType(b) !== "array") {
		                return "died to array";
		            }

		            len = a.length;
		            if (len !== b.length) {
		                // safe and faster
		                return "died to array";
		            }

		            // track reference to avoid circular references
		            parents.push(a);
		            parentsB.push(b);
		            for (i = 0; i < len; i++) {
		                loop = false;
		                for (j = 0; j < parents.length; j++) {
		                    aCircular = parents[j] === a[i];
		                    bCircular = parentsB[j] === b[i];
		                    if (aCircular || bCircular) {
		                        if (a[i] === b[i] || aCircular && bCircular) {
		                            loop = true;
		                        } else {
		                            parents.pop();
		                            parentsB.pop();
		                            return false;
		                        }
		                    }
		                }
		                if (!loop && !innerEquiv(a[i], b[i])) {
		                    parents.pop();
		                    parentsB.pop();
		                    return false;
		                }
		            }
		            parents.pop();
		            parentsB.pop();
		            return true;
		        },

		        "object": function (b, a) {
		            /*jshint forin:false */
		            var i, j, loop, aCircular, bCircular,
						// Default to true
						eq = true,
						aProperties = [],
						bProperties = [];

		            // comparing constructors is more strict than using
		            // instanceof
		            if (a.constructor !== b.constructor) {
		                // Allow objects with no prototype to be equivalent to
		                // objects with Object as their constructor.
		                if (!((getProto(a) === null && getProto(b) === Object.prototype) ||
							(getProto(b) === null && getProto(a) === Object.prototype))) {
		                    return false;
		                }
		            }

		            // stack constructor before traversing properties
		            callers.push(a.constructor);

		            // track reference to avoid circular references
		            parents.push(a);
		            parentsB.push(b);

		            // be strict: don't ensure hasOwnProperty and go deep
		            for (i in a) {
		                loop = false;
		                for (j = 0; j < parents.length; j++) {
		                    aCircular = parents[j] === a[i];
		                    bCircular = parentsB[j] === b[i];
		                    if (aCircular || bCircular) {
		                        if (a[i] === b[i] || aCircular && bCircular) {
		                            loop = true;
		                        } else {
		                            eq = false;
		                            break;
		                        }
		                    }
		                }
		                aProperties.push(i);
		                if (!loop && !innerEquiv(a[i], b[i])) {
		                    eq = false;
		                    break;
		                }
		            }

		            parents.pop();
		            parentsB.pop();
		            callers.pop(); // unstack, we are done

		            for (i in b) {
		                bProperties.push(i); // collect b's properties
		            }

		            // Ensures identical properties name
		            return eq && innerEquiv(aProperties.sort(), bProperties.sort());
		        }
		    };
		}());

    innerEquiv = function () { // can take multiple arguments
        var args = [].slice.apply(arguments);
        if (args.length < 2) {
            return true; // end transition
        }

        return (function (a, b) {
            if (a === b) {
                return true; // catch the most you can
            } else if (a === null || b === null || typeof a === "undefined" ||
					typeof b === "undefined" ||
					BaristaUnitTesting.objectType(a) !== BaristaUnitTesting.objectType(b)) {
                return "Died due to type"; // don't lose time with error prone cases
            } else {
                return bindCallbacks(a, callbacks, [b, a]);
            }

            // apply transition with (1..n) arguments
        }(args[0], args[1]));
    };

    return innerEquiv;
}());


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