var currentTip = null;
var currentTipElement = null;

function hideTip(evt, name, unique) {
    var el = document.getElementById(name);
    el.style.display = "none";
    currentTip = null;
}

function findOffsetParents(el) {
  var roots = [];
  var parent = el.offsetParent;
  while (parent) {
    roots.push(parent);
    parent = parent.offsetParent;
  }
  return roots;
}

function findCommonOffsetParent(a, b) {
  var aRoots = findOffsetParents(a);
  var bRoots = findOffsetParents(b);
  for (var aRoot of aRoots) {
    for (var bRoot of bRoots) {
      if (aRoot === bRoot) {
        return aRoot;
      }
    }
  }

  return document.body;
}

function findPos(obj, relativeTo) {
    // no idea why, but it behaves differently in webbrowser component
    if (window.location.search == "?inapp")
        return [obj.offsetLeft + 10, obj.offsetTop + 30];

    var root = findCommonOffsetParent(obj, relativeTo);

    var curleft = 0;
    var curtop = 0;
    while (obj && obj !== root) {
        curleft += obj.offsetLeft;
        curtop += obj.offsetTop;
        obj = obj.offsetParent;
    };
    return { left: curleft, top: curtop };
}

function hideUsingEsc(e) {
    if (!e) { e = event; }
    hideTip(e, currentTipElement, currentTip);
}

function showTip(evt, name, unique, owner) {
    document.onkeydown = hideUsingEsc;
    if (currentTip == unique) return;
    currentTip = unique;
    currentTipElement = name;

    var obj = owner ? owner : (evt.srcElement ? evt.srcElement : evt.target);
    var el = document.getElementById(name);
    el.style.opacity = 0;
    el.style.display = "block";

    var pos = findPos(obj, el);

    el.style.position = "absolute";
    el.style.left = pos.left + "px";
    el.style.top = pos.top + obj.offsetHeight + "px";
    el.style.opacity = 1;
}
