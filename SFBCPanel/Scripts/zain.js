
function filteration() {
    if ($("#fromdate").val() == "" || $("#todate").val() == "") {
        alert("Please select date filter");
    } else {
        $('#overlay').show();
        $.ajax({
            type: 'GET',
            cache: false,
            url: '/CustomerReport/FilteredZainReport',
            dataType: 'json',
            data: {
                fromdate: $("#fromdate").val(),
                todate: $("#todate").val(),
                status: $("#statusfilter1").val(),

            },
            contentType: 'application/json',
            success: function (result) {
                $("#example").dataTable().fnClearTable();
                $.each(result, function (status, data) {
                    $.each(data, function (innerstatus, innerdata) {
                        var data = [];

                        data.push(innerdata.ID);
                        data.push(innerdata.TRAN_Data);
                       
                        data.push(innerdata.BILLER_VOUCHER);
                        data.push(innerdata.ft);
                        data.push(innerdata.BILL_AMOUNT);
                        data.push(innerdata.BBL_SYS_TRACENO);
                        data.push(innerdata.BBL_BNKREFRENCE);
                        data.push(innerdata.BBL_BILLERRESPONSE);
                        $("#example").dataTable().fnAddData(data);
                    });
                });
                $('#overlay').hide();
            }
        });
    }
}