
// Logout ajax code
$("#logoutBtn").click(function () {
    $.ajax({
        url: '/Account/Logout',
        type: 'POST',
        data: $("#logoutForm").serialize(),
        success: function (response) {
            if (response.success) {
                window.location.href = '/Account/Login';
            }
        },
        error: function () {
            alert("Error while logging out!");
        }
    });
});
