var log = require("Unified Logging Service");
var web = require("Web");

var correlationId = web.request.queryString['cid'];
var logEntries = log.getLocalLogEntries(correlationId);
logEntries;