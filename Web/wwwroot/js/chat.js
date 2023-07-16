document.getElementById("sendButton").disabled = true;

$.ajax({
    url: getChatHubUrl,
    type: 'GET',
    success: function (response) {
        var chatHubUrl = response.chatHubUrl;
        var connection = new signalR.HubConnectionBuilder()
            .withUrl(chatHubUrl)
            .build();

        connection.on("ReceiveMessage", function (user, message) {
            var li = document.createElement("li");
            document.getElementById("messagesList").appendChild(li);
            li.textContent = `${user} says ${message}`;
        });

        connection.start().then(function () {
            document.getElementById("sendButton").disabled = false;
            var connectionId = connection.connectionId;
            var data = {
                connectionId: connectionId
            };

            $.ajax({
                type: "POST",
                url: setConnectionIdTouser,
                data: JSON.stringify(data),
                contentType: "application/json",
                success: function (response) {
                    console.log(response);
                    var connectionId = response.connectionId;
                },
                error: function (xhr, status, error) {
                    if (xhr.status === 401 || xhr.status === 403) {
                        window.location.href = HomeIndexUrl;
                    } else {
                        console.error(xhr);
                    }
                }
            });
        }).catch(function (err) {
            return console.error(err.toString());
        });

        document.getElementById("sendButton").addEventListener("click", function (event) {
            var sendToUserInput = document.getElementById("sendToUserInput").value;
            var message = document.getElementById("messageInput").value;

            var data = {
                sendToUserInput: sendToUserInput,
                message: message
            };

            $.ajax({
                type: "POST",
                url: sendMessage,
                data: JSON.stringify(data),
                contentType: "application/json",
                success: function (response) {
                    console.log(response);
                    var receivedMessage = response.message;
                    var connectionId = response.connectionId;
                },
                error: function (xhr, status, error) {
                    if (xhr.status === 401 || xhr.status === 403) {
                        window.location.href = homeIndexUrl;
                    } else {
                        console.error(xhr);
                    }
                }
            });

            event.preventDefault();
        });
    },
    error: function (xhr, status, error) {
        if (xhr.status === 401 || xhr.status === 403) {
            window.location.href = homeIndexUrl;
        } else {
            console.error(xhr);
        }
    }
});
