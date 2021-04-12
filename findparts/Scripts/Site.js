// TODO: use this file

function ResendEmail(userId) {
    $.ajax({
        type: "POST",
        url: "/Account/SendVerificationEmail",
        data: { userId },
        success: function (res) {
            if (res.success) {
                alert("Email has been sent");
            } else {
                alert(res.errorMessage);
            }
        },
        fail: function () {
            alert("Failed to send email")
        }
    });
}

function InitializeAdminScbscribersTable() {
    var $tbl = $('#subscribers-table');

    // new header and body elements to be inserted
    var tblHead = document.createElement('thead');
    var tblBody = document.createElement('tbody');
    // get the first row and the remainder
    var headerRow = $tbl.find('tr:first')
    var bodyRows = $tbl.find('tr:not(:first)');

    // remove the original rows
    headerRow.remove();
    bodyRows.remove();

    // SOLUTION HERE:
    // remove any existing thead/tbody elements
    $tbl.find('tbody').remove();
    $tbl.find('thead').remove();

    // add the rows to the header and body respectively
    $(tblHead).append(headerRow);
    $(tblBody).append(bodyRows);

    // add the head and body into the table
    $tbl.append(tblHead);
    $tbl.append(tblBody);

    var calcOffset = function () {
        var headerHeight = $('header').outerHeight();
        var bodyPadding = parseInt($('#body').css('padding-top'), 10);
        if (bodyPadding == 0) {
            return 0;
        } else {
            return headerHeight;
        }
    };

    var dtApi = $('#subscribers-table').DataTable({
        fixedHeader: {
            headerOffset: calcOffset()
        },
        autoWidth: false,
        lengthChange: false,
        pageLength: 100,
        processing: true,
        paging: true,
        serverSide: true,
        ajax: "/Admin/SubscriberList",
        columns: [
            {
                data: "SubscriberName",
                render: function (data, type, full, meta) {
                    if (type === "display" && data != null) {
                        return '<a href="/Admin/Subscribers/' + full["SubscriberID"] + '">' + data + '</a>'
                    }
                    return data;
                }
            },
            {
                data: "Email",
                render: function (data, type, full, meta) {
                    if (type === 'display' && data != null) {
                        return data.replace(/([@@.])/g, '<wbr>$1');
                    }
                    return data;
                }
            },
            {
                data: "Status",
                render: function (data, type, full, meta) {
                    if (type === 'display' && data == 'Brand New') {
                        return '<span class="label label-danger">' + data + '</span>';
                    }
                    return data;
                }
            },
            { data: "DateCreated" },
            {
                data: "DateActivated",
                render: function (data, type, full, meta) {
                    if (type === "display" && data == "") {
                        return '<a class="btn btn-primary btn-xs" href="javascript:void(0)" onclick="ResendEmail(' + full["UserID"] + ')">Resend Email</a>';
                    }
                    return data;
                }
            },
            { data: "ProfileFieldsCompleted" },
            { data: "MembershipLevel" },
            { data: "SearchCount" },
            { data: "InvoiceCount" },
            { data: "UserCount" },
            { data: "RecentInvoiceDate" },
            { data: "RFQSentCount" },
            { data: "QuotesReceivedCount" },
            { data: "PreferredCount" },
            { data: "BlockedCount" },
            { data: "BlockedByVendorCount" },
            { data: "MRO" },
            { data: "EmailDomains" }
        ],
        order: [0, 'asc']
    });

    var resizeTimer = null;
    $(window).resize(function () {
        if (resizeTimer) {
            clearTimeout(resizeTimer);
        }
        resizeTimer = setTimeout(function () {
            dtApi.fixedHeader.headerOffset(calcOffset());
        }, 500);
    });
}

function InitializeAdminVendorsTable() {
    var $tbl = $('#vendors-table');

    // new header and body elements to be inserted
    var tblHead = document.createElement('thead');
    var tblBody = document.createElement('tbody');
    // get the first row and the remainder
    var headerRow = $tbl.find('tr:first')
    var bodyRows = $tbl.find('tr:not(:first)');

    // remove the original rows
    headerRow.remove();
    bodyRows.remove();

    // SOLUTION HERE:
    // remove any existing thead/tbody elements
    $tbl.find('tbody').remove();
    $tbl.find('thead').remove();

    // add the rows to the header and body respectively
    $(tblHead).append(headerRow);
    $(tblBody).append(bodyRows);

    // add the head and body into the table
    $tbl.append(tblHead);
    $tbl.append(tblBody);

    var calcOffset = function () {
        var headerHeight = $('header').outerHeight();
        var bodyPadding = parseInt($('#body').css('padding-top'), 10);
        if (bodyPadding == 0) {
            return 0;
        } else {
            return headerHeight;
        }
    };

    var dtApi = $('#vendors-table').DataTable({
        fixedHeader: {
            headerOffset: calcOffset()
        },
        autoWidth: false,
        lengthChange: false,
        pageLength: 100,
        processing: true,
        paging: true,
        serverSide: true,
        ajax: "/Admin/VendorList",
        columns: [
            {
                data: "VendorName",
                render: function (data, type, full, meta) {
                    if (type === "display" && data != null) {
                        return '<a href="/Admin/Vendors/' + full["VendorID"] + '">' + data + '</a>'
                    }
                    return data;
                }
            },
            {
                data: "Email",
                render: function (data, type, full, meta) {
                    if (type === 'display' && data != null) {
                        return data.replace(/([@@.])/g, '<wbr>$1');
                    }
                    return data;
                }
            },
            {
                data: "Status",
                render: function (data, type, full, meta) {
                    if (type === 'display' && data == 'Brand New') {
                        return '<span class="label label-danger">' + data + '</span>';
                    }
                    return data;
                }
            },
            { data: "DateCreated" },
            {
                data: "DateActivated",
                render: function (data, type, full, meta) {
                    if (type === "display" && data == "") {
                        return '<a class="btn btn-primary btn-xs" href="javascript:void(0)" onclick="ResendEmail(' + full["UserID"] + ')">Resend Email</a>';
                    }
                    return data;
                }
            },
            { data: "ProfileFieldsCompleted" },
            { data: "MuteStatus" },
            { data: "ListsApprovalNeeded" },
            {
                data: "ListsApproved",
                render: function (data, type, full, meta) {
                    if (type === 'display' && data != '0') {
                        return '<span class="label label-danger">' + data + '</span>';
                    }
                    return data;
                }
            },
            { data: "AchievementsApprovalNeeded" },
            {
                data: "AchievementsApproved",
                render: function (data, type, full, meta) {
                    if (type === 'display' && data != '0') {
                        return '<span class="label label-danger">' + data + '</span>';
                    }
                    return data;
                }
            },
            { data: "RecentListApprovalDate" },
            { data: "RFQReceivedCount" },
            { data: "QuotesSentCount" },
            { data: "PartsCount" },
            { data: "AchievementsCount" },
            { data: "OEM" },
            { data: "PreferredCount" },
            { data: "BlockedCount" },
        ],
        order: [0, 'asc']
    });

    var resizeTimer = null;
    $(window).resize(function () {
        if (resizeTimer) {
            clearTimeout(resizeTimer);
        }
        resizeTimer = setTimeout(function () {
            dtApi.fixedHeader.headerOffset(calcOffset());
        }, 500);
    });
}

function ReSendVerificationEmail() {
    $.ajax({
        type: "POST",
        url: "/Account/SendVerificationEmail",
        success: function (res) {
            if (res.success) {
                alert("Email sent");
            } else {
                alert(res.errorMessage);
            }
        },
        fail: function () {
            alert("Failed to send email")
        }
    });
}