
function cpuUse() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/cpu",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d != []) {
                $('#dashboardResults').html(msg.d.substring(2, msg.d.length - 2) + " % CPU Usage");
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function trieStatus() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/trieStatus",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d != []) {
                $('#dashboardResults').html("The Trie is " + msg.d.substring(2, msg.d.length - 2));
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function mem() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/memUsage",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d != []) {
                $('#dashboardResults').html(msg.d.substring(2, msg.d.length - 2) + " MBs of Memory Available");
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function beginCrawler() {
    $('#dashboardResults').empty();
    $('#dashboardResults').html("Web Crawling Starting");
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/beginCrawler",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').html("Successfully Started Crawler");
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};
function clearCrawler() {
    $('#dashboardResults').empty();
    $('#dashboardResults').html("Deleting Web Crawler Results");
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/clearCrawler",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').html("Successfully Deleted Crawler");
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};
function endCrawler() {
    $('#dashboardResults').empty();
    $('#dashboardResults').html("Stopping Web Crawler");
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/endCrawler",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').html("Successfully Stopped Crawler");
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function startQuery() {
    findPlayers();
    findArticles();
}

function findArticles() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/findCNNArticles",
        data: JSON.stringify({
            userInput: document.getElementById("inputValue").value
        }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#cnnArticles').empty();
            if (msg.d == "[]") {
                $('#cnnArticles').html("<br>No Articles Were Found");
            }
            else {
                $('#cnnArticles').append("<br><b>Article Results</b>");
                var text = msg.d.substring(1, msg.d.length - 1);
                var res = text.split(",");
                for (var i = 0; i < res.length; i++) {
                    console.log(unescape(unescape(res[i])));
                    $('#cnnArticles').append("<br>" + unescape(unescape(res[i].substring(1, res[i].length - 1))));
                }
            }
        },
        error: function (msg) {
            $('#cnnArticles').html("<br>No Articles Were Found");
        }
    });
};

function findPlayers() {
    var player = $('#inputValue').val();
    $.ajax({
        crossDomain: true,
        contentType: "application/json; charset=utf-8",
        url: "http://54.149.230.201/index.php",
        data: { searchName: player },
        dataType: "jsonp",
        type: 'GET',
        success: function (msg) {
            $('#nbaResult').empty();
            console.log(msg);
            if (msg.length == 1) {
                $('#nbaResult').append("<br><b>" + JSON.stringify(msg[0][0]).substring(1, JSON.stringify(msg[0][0]).length - 1) + "</h3>");
                $('#nbaResult').append("<br> Games Played: " + JSON.stringify(msg[0][1]).substring(1, JSON.stringify(msg[0][1]).length - 1));
                $('#nbaResult').append("<br> Field Goal Percentage: " + JSON.stringify(msg[0][2]).substring(1, JSON.stringify(msg[0][2]).length - 1) + "%");
                $('#nbaResult').append("<br> Three Point Percentage: " + JSON.stringify(msg[0][3]).substring(1, JSON.stringify(msg[0][3]).length - 1) + "%");
                $('#nbaResult').append("<br> Free Throw Percentage: " + JSON.stringify(msg[0][4]).substring(1, JSON.stringify(msg[0][4]).length - 1) + "%");
                $('#nbaResult').append("<br> Points Per Game: " + JSON.stringify(msg[0][5]).substring(1, JSON.stringify(msg[0][5]).length - 1));                
            } else {
                $('#nbaResult').html("<br>No NBA Player Results Were Found");
            }
        },
        error: function (msg) {
            console.log(msg);
            $('#nbaResult').html("<br>No NBA Player Results Were Found");
        }
    });
};

function findWorkerState() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/workerState",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d != []) {
                $('#dashboardResults').html("The Crawler is Currently " + msg.d.substring(2, msg.d.length - 2));
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function lastTen() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/lastTenUrl",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d == "[]") {
                $('#dashboardResults').html("No Results Were Found");
            }
            else {
                console.log(msg.d);
                $('#dashboardResults').html("The Last Ten URLs Crawled:");
                var text = msg.d.substring(1, msg.d.length - 1);
                var res = text.split(",");
                for (var i = 0; i < res.length; i++) {
                    $('#dashboardResults').append("<br>" + res[i].substring(1, res[i].length - 1));
                }
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function findQueueCount() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/queueCount",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            $('#dashboardResults').html(msg.d.substring(1, msg.d.length - 1) + " Messages Left in Queue");
        },
        error: function (msg) {
            console.log(msg);
        }
    });
};

function findMatches() {
    var query = $('#inputValue').val();
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/searchTrie",
        data: JSON.stringify({
            prefix: query
        }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log(msg.d);
            $('#json').empty();
            $('#json').append("Results for <b>" + document.getElementById("inputValue").value + "</b> <br>");
            if (msg.d == "[]") {
                $('#json').append("No Suggestions Were Found");
            } else {
                var validInput = /^[a-zA-Z ]+$/.test(document.getElementById("inputValue").value);;
                if (validInput) {
                    var text = msg.d.substring(1, msg.d.length - 1);
                    var res = text.split(",");
                    for (var i = 0; i < res.length; i++) {
                        $('#json').append(res[i].substring(1, res[i].length - 1) + "<br>");
                    }
                }
            }
        },
        error: function (msg) {
            console.log(msg);
        }
    });
};

function checkPerform() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/checksPerformed",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d != []) {
                $('#dashboardResults').html(msg.d.substring(2, msg.d.length - 2) + " URLs were Crawled");
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};
function checksPassed() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/checksPassed",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d != []) {
                $('#dashboardResults').html(msg.d.substring(2, msg.d.length - 2) + " Unique URLS were Successfully Entered into the Table");
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function lastTitle() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/lastTitle",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d != []) {
                $('#dashboardResults').html("The Last Title Added to Trie was " + msg.d.substring(2, msg.d.length - 2));
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function numberOfTitles() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/numberOfTitles",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d != []) {
                $('#dashboardResults').html(msg.d.substring(2, msg.d.length - 2) + " Titles included in the Trie");
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function lastTenErrors() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/lastTenErrors",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d == "[]") {
                $('#dashboardResults').html("No Errors Were Found");
            }
            else {
                console.log(msg.d);
                $('#dashboardResults').html("The Last Ten URLs Crawled with Errors:");
                var text = msg.d.substring(1, msg.d.length - 1);
                var res = text.split(",");
                for (var i = 0; i < res.length; i++) {
                    $('#dashboardResults').append("<br>" + res[i].substring(1, res[i].length - 1));
                }
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function readErrorTable() {
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/findErrorMessage",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').empty();
            if (msg.d == "[]") {
                $('#dashboardResults').html("No Errors Were Found");
            }
            else {
                console.log(msg.d);
                $('#dashboardResults').html("The Last 10 Error Messages");
                var text = msg.d.substring(1, msg.d.length - 1);
                var res = text.split(",");
                for (var i = 0; i < res.length; i++) {
                    $('#dashboardResults').append("<br>" + res[i].substring(1, res[i].length - 1));
                }
            }
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function showDashboard() {
    $("#dashboard").toggle();
};

function downloadWiki() {
    $('#dashboardResults').empty();
    $('#dashboardResults').html("Downloading Wiki");
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/downloadWiki",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').html("Successfully Download Wiki Titles");
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};

function buildTrie() {
    $('#dashboardResults').empty();
    $('#dashboardResults').html("Building Trie");
    $.ajax({
        type: "POST",
        url: "WebService1.asmx/buildTrie",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#dashboardResults').html("Successfully Built Trie");
        },
        error: function (msg) {
            $('#dashboardResults').html("The Required Table is not Available");
        }
    });
};