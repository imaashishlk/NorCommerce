$(document).ready(function () {
    const status = new URLSearchParams(window.location.search).get('status');
    const url = status
        ? '/admin/order/getall?status=' + encodeURIComponent(status)
        : '/admin/order/getall';

    $('#orderTable').DataTable({
        ajax: {
            url: url,
            dataSrc: 'data'
        },
        columns: [
            { data: 'id', width: "10%" },
            { data: 'phoneNumber', width: "15%" },
            {
                data: 'orderDate',
                width: "15%",
                render: function (data) { return new Date(data).toLocaleDateString(); }
            },
            {
                data: 'orderTotal',
                width: "10%",
                render: function (data) { return '$' + data.toFixed(2); }
            },
            {
                data: 'orderStatus',
                width: "15%",
                render: function (data) {
                    var bg = '#f1f5f9', fg = '#475569';
                    if (data === 'Approved') { bg = '#d1fae5'; fg = '#047857'; }
                    else if (data === 'Processing') { bg = '#fef3c7'; fg = '#b45309'; }
                    else if (data === 'Shipped') { bg = '#dbeafe'; fg = '#1d4ed8'; }
                    else if (data === 'Cancelled') { bg = '#ffe4e6'; fg = '#be123c'; }
                    else if (data === 'Refunded') { bg = '#fce7f3'; fg = '#9f1239'; }
                    return '<span style="display:inline-flex;align-items:center;font-size:12px;font-weight:500;padding:3px 10px;border-radius:4px;background:' + bg + ';color:' + fg + '">' + data + '</span>';
                }
            },
            {
                data: 'id',
                width: "10%",
                orderable: false,
                render: function (data) {
                    return `
                        <a href="/Admin/Order/OrderDetails/${data}" class="btn btn-sm btn-outline-dark">
                            <i class="bi bi-eye me-1"></i> Details
                        </a>`;
                }
            }
        ]
    });
});