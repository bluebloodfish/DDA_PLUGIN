$(document).ready(function () {
    $.ajax({
        url: '/api_v1/console/GetVersion',
        method: 'get',
        dataType: 'json',
        success: function (result) {
            console.log(result.data.Version);
            $('#hVersion').text('Plugin Version: ' + result.data.Version);
        },
        error: function (err) {
            alert(err);
        }
    });

    $.ajax({
        url: '/api_v1/hospOrder/GetVersion',
        method: 'get',
        dataType: 'json',
        success: function (result) {
            console.log(result.data.Version);
            $('#orderVersion').text('OrderApi Version: ' + result.data.Version);
        },
        error: function (err) {
            alert(err);
        }
    });
});