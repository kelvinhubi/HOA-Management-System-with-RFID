var connection2 = new signalR.HubConnectionBuilder().withUrl("/chat").build();
connection2.on("RecieveNotification", function (Message) {
    const notification = document.getElementById('notifications');
    notification.classList.add('notification');
    notification.textContent = Message;
    console.log(Message);
    setTimeout(() => {
        notification.classList.remove('notification');
        notification.textContent = "";
    }, 5000);
});

connection2.start().catch(function (err) {
    return console.error(err.toString());
});
$("#notify").on('click', function () {
    console.log("Mustwork")
    var message = "Pls pay your remaining dues!";
    connection2.invoke("Notification", message).catch(function (err) {
            return console.error(err.toString());
        });
});