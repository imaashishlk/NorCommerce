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
        order: [[0, 'desc']],
        columns: [
            { data: 'id', width: "10%", render: function (data) { return '<strong>#' + data + '</strong>'; } },
            { data: 'phoneNumber', width: "15%" },
            {
                data: 'orderDate',
                width: "15%",
                render: function (data) {
                    var d = new Date(data);
                    return d.toLocaleDateString('nb-NO') + ' <span class="text-secondary small">' +
                        d.toLocaleTimeString('nb-NO', { hour: '2-digit', minute: '2-digit' }) + '</span>';
                }
            },
            {
                data: 'orderTotal',
                width: "10%",
                render: function (data) { return data.toFixed(2) + ' kr'; }
            },
            {
                data: 'orderStatus',
                width: "15%",
                render: function (data) {
                    var bg = '#f1f5f9', fg = '#475569', icon = 'bi-hourglass-split';
                    if (data === 'Approved') { bg = '#d1fae5'; fg = '#047857'; icon = 'bi-check-circle'; }
                    else if (data === 'Processing') { bg = '#fef3c7'; fg = '#b45309'; icon = 'bi-gear'; }
                    else if (data === 'Shipped') { bg = '#dbeafe'; fg = '#1d4ed8'; icon = 'bi-truck'; }
                    else if (data === 'Cancelled') { bg = '#ffe4e6'; fg = '#be123c'; icon = 'bi-x-circle'; }
                    else if (data === 'Refunded') { bg = '#fce7f3'; fg = '#9f1239'; icon = 'bi-arrow-counterclockwise'; }
                    return '<span style="display:inline-flex;align-items:center;gap:6px;font-size:12px;font-weight:600;padding:4px 12px;border-radius:50px;background:' + bg + ';color:' + fg + '">' +
                        '<i class="bi ' + icon + '"></i>' + data + '</span>';
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