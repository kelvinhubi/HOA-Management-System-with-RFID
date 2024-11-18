var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();
connection.start().catch(function (err) {
    return console.error(err.toString());
});
$("#sendmessage").on('click', function (event) {
    var message = document.getElementById("message").value;
    var name = document.getElementById('Nameval').value;
    if (message != "") {
        connection.invoke("JoinRF", name, message).catch(function (err) {
            return console.error(err.toString());
        });
        $('form').trigger('reset');
    }

});
$('#message').keypress(function (e) {
    if (e.which == 13) {
        e.preventDefault();
        //do something   
    }
});
$('form').keypress(function (event) {
    if (event.key === 'Enter') {
        var message = document.getElementById("message").value;
        var name = document.getElementById('Nameval').value;
        if (message != "") {
            connection.invoke("JoinRF", name, message).catch(function (err) {
                return console.error(err.toString());
            });
            $(this).trigger('reset');
        }
    }
});
connection.on("RecieveMessage", function (Name, Message, Time) {
    var MainUser = $('#Nameval').val();
    if (MainUser === Name) {
        $('#pops').append('<div class="msg-reverse"><p>'
            + Message + '</p><span>'
            + Name + ' ' + Time + '</span></div>');
    }
    if (MainUser != Name) {
        $('#pops').append('<div class="msg"><p>'
            + Message + '</p><span>'
            +Name + ' ' + Time +'</span></div>');
    }
});
connection.on("AdminError", function (Message) {
    const notification = document.getElementById('notifications');
    notification.classList.remove('notification');
    notification.textContent = "";
    notification.classList.add('notification');
    notification.textContent = Message;
    console.log(Message);
    setTimeout(() => {
        notification.classList.remove('notification');
        notification.textContent = "";
    }, 5000);
});
