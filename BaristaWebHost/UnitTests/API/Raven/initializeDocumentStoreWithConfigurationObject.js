require("Raven Client");
require("Unit Testing");

var ds = new DocumentStore({
    url: "http://sixconcepts-ravendb-01.cloudapp.net/",
    defaultDatabase: "testdb",
    credentials: { userName: "RavenDBUser", password: "R00tB33r!" }
}).initialize();


var key = chance.name();
var text = chance.word();

ds.databaseCommands.put(key, null, { hello: text }, null);
var result = ds.databaseCommands.get(key)

assert.areEqual(text, result.data.hello);

result;