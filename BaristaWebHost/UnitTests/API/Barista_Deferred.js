require("Deferred");
var combined = new Array();
var calls = new Array();

for (var i = 0; i < 10; i++) {
    var deferred = new Deferred(function () {
        var value = Math.floor((Math.random() * 1000) + 1);
        delay(value);
        return value;
    });

    deferred.done(function (result) { combined.push(result); });
    calls.push(deferred);
}

waitAll(calls);

combined;