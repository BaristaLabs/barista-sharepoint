// Author: Michael Schøler, 2008
// Dual licensed as MIT and LGPL, use as you like, don't hold me responsible for success or failure though
Array.prototype.compareTo = function(compareAry) {
  if (this.length === compareAry.length) {
    var i;
    for (i = 0; i < compareAry.length; i+=1) {
      if (Object.prototype.toString.call( this[i] ) === '[object Array]' ) {
        if (this[i].compareTo(compareAry[i]) === false) {
          return false;
        }
        continue;
      }
      else if (this[i] !== compareAry[i]) {
        return false;
      }
    }
    return true;
  }
  return false;
};
var jsonDataHandler = {
  merge: function(j1, j2) {
    if (typeof this.merging === "undefined" || this.merging === 0) {
      this.mergeCyclicCheck = [];
      this.merging = 0;
    }
    this.merging += 1;
    if (typeof j1 === "undefined") {
      j1 = {};
    }
    if (typeof j2 === "undefined") {
      j2 = {};
    }
    if (typeof this.mergeCyclicCheck === "undefined") {
      this.mergeCyclicCheck = [];
    }
    var key;
    for (key in j2) if (j2.hasOwnProperty(key)) {
      if (typeof j1[key] === "undefined") {
        j1[key] = j2[key];
      }
      else {
        if (typeof j2[key] === "object") {
          if (this.mergeCyclicCheck.indexOf(j1[key]) >= 0) {
            break;
          }
          this.merge(j1[key], j2[key]);
          this.mergeCyclicCheck.push(j1[key]);
        } 
        else {
          j1[key] = j2[key]; 
        }
      }
    }
    this.merging -= 1;
  },
  diff: function(j1, j2) {
    if (typeof this.diffing === "undefined" || this.diffing === 0) {
      this.diffCyclicCheck = [];
      this.diffing = 0;
    }
    var diffRes = {};
    this.diffing += 1;
    if (typeof j1 === "undefined") {
      j1 = {};
    }
    if (typeof j2 === "undefined") {
      j2 = {};
    }
    if (typeof this.diffCyclicCheck === "undefined") {
      this.diffCyclicCheck = [];
    }
    var key, bDiff;
    for (key in j2) if (j2.hasOwnProperty(key)) {  
      bDiff = false;
      if (typeof j1[key] === "undefined" || typeof j1[key] != typeof j2[key]) {
        bDiff = true;
      }
      else if (j1[key] !== j2[key]) {
        if (typeof j2[key] === "object") {
          if (this.diffCyclicCheck.indexOf(j2[key]) >= 0) {
            break;
          }
        else if (Object.prototype.toString.call(j2[key]) === '[object Array]') {
            if (j1[key].length !== j2[key].length || j1[key] !== j2[key]) {
              if (j2[key].compareTo(j1[key]) === false) {
                bDiff = true;
              }
            }
          }
          else if (typeof j1[key] === "object") {
            var dR = this.diff(j1[key], j2[key]);
            if (Object.keys(dR).length > 0) {
              diffRes[key] = dR;
            }
          }
          else {
            bDiff = true;
          }
          this.diffCyclicCheck.push(j2[key]);
        }
        else if (j1[key] !== j2[key]) {
          bDiff = true;
        }
      }
      if (bDiff) {
        diffRes[key] = j2[key];
      }
    }
    for (key in j1) if (j1.hasOwnProperty(key)) { 
      bDiff = false;
      if (typeof j2[key] === "undefined") {
        diffRes[key] = j1[key];
      }
    }
    this.diffing -= 1;
    return diffRes;
  }
};