var r = require("Sucralose");
require("Unit Testing");

assert.areDeepEqual((5).ceil(), 5, 'Number#ceil | 5');
assert.areDeepEqual((-5.5).ceil(), -5, 'Number#ceil | -5.5');
assert.areDeepEqual((-5.14).ceil(), -5, 'Number#ceil | -5.14');
assert.areDeepEqual((-5).ceil(), -5, 'Number#ceil | -5');
assert.areDeepEqual((4417.1318).ceil(0), 4418, 'Number#ceil | 0');
assert.areDeepEqual((4417.1318).ceil(1), 4417.2, 'Number#ceil | 1', { prototype: 4418, mootools: 4418 });
assert.areDeepEqual((4417.1318).ceil(2), 4417.14, 'Number#ceil | 2', { prototype: 4418, mootools: 4418 });
assert.areDeepEqual((4417.1318).ceil(3), 4417.132, 'Number#ceil | 3', { prototype: 4418, mootools: 4418 });
assert.areDeepEqual((4417.1318).ceil(-1), 4420, 'Number#ceil | -1', { prototype: 4418, mootools: 4418 });
assert.areDeepEqual((4417.1318).ceil(-2), 4500, 'Number#ceil | -2', { prototype: 4418, mootools: 4418 });
assert.areDeepEqual((4417.1318).ceil(-3), 5000, 'Number#ceil | -3', { prototype: 4418, mootools: 4418 });

assert.areDeepEqual((5.5).floor(), 5, 'Number#floor | 5.5');
assert.areDeepEqual((5.14).floor(), 5, 'Number#floor | 5.14');
assert.areDeepEqual((5.9).floor(), 5, 'Number#floor | 5.9');
assert.areDeepEqual((5).floor(), 5, 'Number#floor | 5');
assert.areDeepEqual((-5.5).floor(), -6, 'Number#floor | -5.5');
assert.areDeepEqual((-5.14).floor(), -6, 'Number#floor | -5.14');
assert.areDeepEqual((-5).floor(), -5, 'Number#floor | -5');
assert.areDeepEqual((4417.1318).floor(0), 4417, 'Number#floor | 0');
assert.areDeepEqual((4417.1318).floor(1), 4417.1, 'Number#floor | 1', { prototype: 4417, mootools: 4417 });
assert.areDeepEqual((4417.1318).floor(2), 4417.13, 'Number#floor | 2', { prototype: 4417, mootools: 4417 });
assert.areDeepEqual((4417.1318).floor(3), 4417.131, 'Number#floor | 3', { prototype: 4417, mootools: 4417 });
assert.areDeepEqual((4417.1318).floor(-1), 4410, 'Number#floor | -1', { prototype: 4417, mootools: 4417 });
assert.areDeepEqual((4417.1318).floor(-2), 4400, 'Number#floor | -2', { prototype: 4417, mootools: 4417 });
assert.areDeepEqual((4417.1318).floor(-3), 4000, 'Number#floor | -3', { prototype: 4417, mootools: 4417 });

assert.areDeepEqual((3).round(), 3, 'Number#round | 3');
assert.areDeepEqual((3.241).round(), 3, 'Number#round | 3.241');
assert.areDeepEqual((3.752).round(), 4, 'Number#round | 3.752');
assert.areDeepEqual((-3.241).round(), -3, 'Number#round | -3.241');
assert.areDeepEqual((-3.752).round(), -4, 'Number#round | -3.752');
assert.areDeepEqual((3.241).round(1), 3.2, 'Number#round | 3.241 to 1 place', { prototype: 3 });

assert.areDeepEqual((3.752).round(1), 3.8, 'Number#round | 3.752 to 1 place', { prototype: 4 });
assert.areDeepEqual((3.241).round(2), 3.24, 'Number#round | 3.241 to 2 places', { prototype: 3 });
assert.areDeepEqual((3.752).round(2), 3.75, 'Number#round | 3.752 to 2 places', { prototype: 4 });

assert.areDeepEqual((322855.241).round(-2), 322900, 'Number#round | 322855.241 to -2 places', { prototype: 322855 });
assert.areDeepEqual((322855.241).round(-3), 323000, 'Number#round | 322855.241 to -3 places', { prototype: 322855 });
assert.areDeepEqual((322855.241).round(-4), 320000, 'Number#round | 322855.241 to -4 places', { prototype: 322855 });
assert.areDeepEqual((322855.241).round(-6), 0, 'Number#round | 322855.241 to -6 places', { prototype: 322855 });
assert.areDeepEqual((722855.241).round(-6), 1000000, 'Number#round | 722855.241 to -6 places', { prototype: 722855 });
assert.areDeepEqual((722855.241).round(-8), 0, 'Number#round | 722855.241 to -8 places', { prototype: 722855 });

"";