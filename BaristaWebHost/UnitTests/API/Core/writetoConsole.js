console.error('A catastrophic error has occurred.');
Number.prototype.padLeft = function(n,str){
    return Array(n-String(this).length+1).join(str||'0')+this;
};

var today = new Date();
var logEntries = fs.loadAsByteArray('~\\logs\\' + today.getFullYear() + '-' + (today.getMonth() + 1).padLeft(2) + '-' + (today.getDate()).padLeft(2) + '.log');

assert.isTrue(data.indexOf("A catastrophic error has occurred.") !== -1);

true;