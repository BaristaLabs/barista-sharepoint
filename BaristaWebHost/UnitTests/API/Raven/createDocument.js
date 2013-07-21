require("Raven Client");
require("Unit Testing");

var ds = new DocumentStore();
ds.url = "http://sixconcepts-ravendb-01.cloudapp.net/";
ds.defaultDatabase = "testdb";
ds.credentials = new NetworkCredential()
ds.credentials.userName = "RavenDBUser";
ds.credentials.password = "R00tB33r!";
ds.initialize();


var key = chance.name();
var text = chance.word();

ds.databaseCommands.put(key, null, { hello: text }, null);
var result = ds.databaseCommands.get(key)

assert.areEqual(text, result.data.hello);

result;