
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


$("#deleteBtn").click(function () {
    $.ajax({
        url: '/Profile/DeleteAccount',
        type: 'POST',
        data: $("#deleteForm").serialize(),
        success: function (response) {
            if (response.success) {
                window.location.href = '/Account/Login';
            }
        },
        error: function () {
            alert("Error while deleted account!");
        }
    });
});