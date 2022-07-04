// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//EditProfile.cshtml flexSwitchCheckDefault onclick
function SellerAccountChange() {
    if (document.getElementById("flexSwitchCheckDefault").checked) {
        document.getElementById("SellerLabel").innerHTML = "Seller"
    }
    else {
        document.getElementById("SellerLabel").innerHTML = "Not a Seller"
    }
}

function ShowDNSModal() {

    $('#exampleModal').modal('show');
}

//let listItem = document.getElementById("CategoryListItem");
//listItem = addEventListener('dblclick', AddSubCategory)

function httpGet(url) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", url, false);
    xmlHttp.send(null);
    return xmlHttp.responseText;
}