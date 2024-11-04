"use strict"

//const { signalR } = require("./signalr/dist/browser/signalr")

var connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();

connection.start().then(function () {
    GetAllUsers();
}).catch(function (err) {
    return console.error(err.toString());
});

const element = document.querySelector("#alert");
//element.style.display = "none";

connection.on("Connect", function (info) {
    console.log(info);
    GetAllUsers();
    element.style.display = "block";
    element.innerHTML = info;
    setTimeout(() => {
        element.innerHTML = "";
        element.style.display = "none";
    }, 5000);
});

connection.on("Disconnect", function (info) {
    GetAllUsers();
    element.style.display = "block";
    element.innerHTML = info;
    setTimeout(() => {
        element.innerHTML = "";
        element.style.display = "none";
    }, 5000);
});


async function SendFollowCall(id) {
    await connection.invoke("SendFollow", id);
}
connection.on("ReceiveNotification", function () {
    GetMyRequests();
    GetAllUsers();
});


async function CallOnlyMyRequests(id) {
    await connection.invoke("CallOnlyMyRequests", id);
}
connection.on("GetOnlyMyRequests", function () {
    GetMyRequests();
});


async function CallOnlyAllUsers(id) {
    await connection.invoke("CallOnlyAllUsers", id);
}
connection.on("GetOnlyAllUsers", function () {
    GetAllUsers();
});


async function GetMessageCall(receiverId, senderId) {
    //alert("GetMessageCall");
    await connection.invoke("GetMessages", receiverId, senderId);
}
connection.on("ReceiveMessages", function (receiverId, senderId) {
    //alert("ReceiveMessages");
    GetMessages(receiverId, senderId);
});

