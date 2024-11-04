// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function GetAllUsers() {
    $.ajax({
        url: "/Home/GetAllUsers",
        method: "GET",
        success: function (data) {
            console.log(data);
            let content = "";
            for (var i = 0; i < data.length; i++) {
                let style = '';
                let subContent = '';
                if (data[i].hasRequestPending) {
                    subContent = `<button class="btn btn-outline-secondary" onclick="TakeRequest('${data[i].id}')">Already Sent</button>`;
                }
                else {
                    if (data[i].isFriend) {
                        subContent = `<button class="btn btn-outline-secondary mr-2" onclick="UnFollowFriend('${data[i].id}')">UnFollow</button>
                                <a class="btn btn-outline-secondary" href="/Home/GoChat/${data[i].id}">Send Message</a>
                        `;

                    }
                    else {
                        subContent = `<button onclick="SendFollow('${data[i].id}')" class="btn btn-outline-primary">Follow</button>`;
                    }
                }
                if (data[i].isOnline) {
                    style = 'border:5px solid springgreen;';
                }
                else {
                    style = 'border:5px solid red;';
                }
                const item = `
                <div class="card" style='${style}width:220px;margin:5px;'>
                    <img style='width:100%;height:220px;' src="/images/${data[i].image}" />
                    <div class='card-body'>
                        <h5 class='card-title'>${data[i].userName}</h5>
                        <p class='card-text'>${data[i].email}</p>
                        ${subContent}
                    </div>
                </div>
                `;
                content += item;
            }
            $("#allUsers").html(content);
        }
    });

}


function SendMessage(receiverId, senderId) {
    const content = document.querySelector("#message-input");
    let obj = {
        receiverId: receiverId,
        senderId: senderId,
        content: content.value
    };

    $.ajax({
        url: "/Home/AddMessage",
        method: "POST",
        data: obj,
        success: function (data) {
            GetMessageCall(receiverId, senderId);
            content.value = "";
        }
    })
}

function GetMessages(receiverId, senderId) {
    //console.log("Entered GetMessages");
    $.ajax({
        url: `/Home/GetAllMessages?receiverId=${receiverId}&senderId=${senderId}`,
        method: "GET",
        success: function (data) {
            console.log(data.currentUserId);
            let content = "";
            //console.log("Entered GetMessages");
            //console.log(data);
            //alert("GetAllMessages");
            for (var i = 0; i < data.messages.length; i++) {
                let item = `<section style='display:flex;margin-top:25px;border:2px solid springgreen;
                                            border-radius:0 20px 20px 0;width:50%;padding:20px;padding-left:0px;'>
                                <h5>
                                ${data.messages[i].content}
                                </h5>
                                 <p>
                                     ${data.messages[i].dateTime}
                                 </p>
                            </section>`;
                content += item;
            }
            console.log(data);
            $("#currentMessages").html(content);
        }
    })
}

function UnFollowFriend(friendId) {
    const element = document.querySelector("#alert");
    element.style.display = "none";

    console.log("Entered UnFollowFriend function ! ");
    $.ajax({
        url: `Home/UnFollowFriend?friendId=${friendId}`,
        method: "DELETE",
        success: function (data) {
            element.style.display = "block";
            element.innerHTML = "You unfollow friend successfully !";

            GetAllUsers();
            SendFollowCall(friendId);
            //CallOnlyAllUsers(friendId);

            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000);
        }
    });
}

function TakeRequest(receiverId) {
    const element = document.querySelector("#alert");
    element.style.display = "none";
    $.ajax({
        url: `/Home/TakeRequest?receiverId=${receiverId}`,
        //url: `/Home/TakeRequest/${encodeURIComponent(receiverId)}`,
        method: "DELETE",
        success: function (data) {
            element.style.display = "block";
            element.innerHTML = "You had bring back friend request successfully !";
            //SendFollowCall(id);
            CallOnlyMyRequests(receiverId);
            GetAllUsers();
            //GetMyRequests();
            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000);
        }
    });
}

GetAllUsers();
GetMyRequests();
function SendFollow(id) {

    const element = document.querySelector("#alert");
    element.style.display = "none";
    $.ajax({
        url: `/Home/SendFollow/${id}`,
        method: "GET",
        success: function (data) {
            element.style.display = "block";
            element.innerHTML = "Your friend request sent successfully !";
            SendFollowCall(id);
            GetAllUsers();
            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000);
        }
    });
}

function GetMyRequests() {
    $.ajax({
        url: "/Home/GetAllRequests",
        method: "GET",
        success: function (response) {
            let content = '';
            let subContent = '';
            for (var i = 0; i < response.length; i++) {
                if (response[i].status == "Request") {
                    subContent = `
                    <div class="card-body">
                        <button class="btn btn-success" onclick="AcceptRequest('${response[i].senderId}','${response[i].receiverId}',${response[i].id})">Accept</button>
                        <button class="btn btn-secondary" onclick="DeclineRequest(${response[i].id},'${response[i].senderId}')">Decline</button>
                    </div>
                    `;
                }
                else {
                    subContent = `
                    <div class="card-body">
                        <button class="btn btn-warning" onclick="DeleteRequest(${response[i].id},'${response[i].senderId}','${response[i].receiverId}')">Delete</button>
                    </div>
                    `;
                }
                let item = `
                <div class="card" style="width:15rem">
                    <div class="card-body">
                        <h5>Request</h5>
                        <ul class="list-group list-group-flush">
                            <li>${response[i].content}</li>
                        </ul>
                        ${subContent}
                    </div>
                </div>
                `;
                content += item;
            }
            $("#requests").html(content);
        }
    })
}


function DeleteRequest(requestId, senderId, receiverId) {
    $.ajax({
        //url: `/Home/DeleteRequest/${requestId}`,//elvinin yazdigi route params ile
        url: `/Home/DeleteRequest?requestId=${requestId}&senderId=${senderId}&receiverId=${receiverId}`,
        method: "DELETE",
        success: function (data) {
            const element = document.querySelector("#alert");
            element.style.display = "block";
            element.innerHTML = "Request deleted successfully !";

            GetMyRequests();

            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000);
        }
    })
}

function AcceptRequest(senderId, receiverId, requestId) {
    $.ajax({
        url: `/Home/AcceptRequest?receiverId=${senderId}&senderId=${receiverId}&requestId=${requestId}`,
        method: "GET",
        success: function (data) {
            const element = document.querySelector("#alert");
            element.style.display = "block";
            element.innerHTML = "You accept request successfully !";

            GetAllUsers();
            SendFollowCall(senderId);
            SendFollowCall(receiverId);

            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000);
        }
    });
}


function DeclineRequest(currentId, senderId/*Jake_123*/) {
    $.ajax({
        url: `/Home/DeclineRequest?currentId=${currentId}&senderId=${senderId}`,
        method: "GET",
        success: function (data) {
            const element = document.querySelector("#alert");
            element.style.display = "block";
            element.innerHTML = "You declined request !";

            SendFollowCall(senderId);

            GetMyRequests();//bunu yoxla 
            GetAllUsers();  //bunu yoxla 

            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000);
        }
    })
}