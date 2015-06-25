require("Unit Testing");

console.error('A catastrophic error has occurred.');
Number.prototype.padLeft = function(n,str){
    return Array(n-String(this).length+1).join(str||'0')+this;
};

var today = new Date();
var logEntries = fs.load('~\\logs\\' + today.getFullYear() + '-' + (today.getMonth() + 1).padLeft(2) + '-' + (today.getDate()).padLeft(2) + '.log');

assert.isTrue(logEntries.indexOf("A catastrophic error has occurred.") !== -1);

true;