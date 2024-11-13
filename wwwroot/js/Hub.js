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
    var li = document.createElement("li");
    var MainUser = $('#Nameval').val();
    console.log(MainUser);
    console.log(Name);
    if (MainUser == Name) {
        $('#pops').append('<div class="direct-chat-msg end"><div class="direct-chat-infos clearfix"><span class="direct-chat-name float-start">'
            + Name + '</span><span class="direct-chat-timestamp float-end">'
            + Time + '</span></div><div class="direct-chat-text">'
            + Message + '</div></div>');
    }
    if (MainUser != Name) {
        $('#pops').append('<divclass="direct-chat-msg"><div class="direct-chat-infos clearfix"><span class="direct-chat-name float-end">'
            + Name + '</span><span class="direct-chat-timestamp float-start">'
            + Time + '</span></div><div class="direct-chat-text">'
            + Message + '</div></div>');
    }
});
