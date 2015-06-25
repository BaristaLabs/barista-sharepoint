var web = require('Web');
require("Unit Testing");

var request = web.request;

assert.isNotNull(request, "web.request should not be null.");
assert.isTrue(request.clientInfo.userAgent.family.length > 0, "Request client info should specify a family.");
request;