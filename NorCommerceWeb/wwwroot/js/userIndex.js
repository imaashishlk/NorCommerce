var token;

$(document).ready(function () {
    token = $('input[name="__RequestVerificationToken"]').val();

    const url = '/admin/user/getalluser'

    $('#tblData').DataTable({
        ajax: {
            url: url,
            dataSrc: 'data'
        },
        columns: [
            {
                data: 'name', width: "15%", render: function (data) {
                    return '<div class="d-flex align-items-center gap-2">' +
                        '<span style="width:32px;height:32px;border-radius:50%;background:#13294B;color:#fff;display:inline-flex;align-items:center;justify-content:center;font-weight:600;font-size:13px;">' + data.charAt(0).toUpperCase() + '</span>' +
                        '<span class="fw-semibold">' + data + '</span></div>';
                } },
            { data: 'email', width: "20%", render: function (data) { return '<span class="text-secondary">' + data + '</span>'; } },
            { data: 'phoneNumber', width: "15%", render: function (data) { return '<span class="text-secondary">' + (data ?? '-') + '</span>'; } },
            {
                data: 'role',
                width: "15%",
                render: function (data) {
                    var bg = '#f1f5f9', fg = '#475569';
                    if (data === 'Customer') { bg = '#d1fae5'; fg = '#047857'; }
                    else if (data === 'Employee') { bg = '#fef3c7'; fg = '#b45309'; }
                    else if (data === 'Admin') { bg = '#dbeafe'; fg = '#1d4ed8'; }
                    return '<span style="display:inline-flex;align-items:center;font-size:12px;font-weight:500;padding:3px 10px;border-radius:4px;background:' + bg + ';color:' + fg + '">' + data + '</span>';
                }
            },
            {
                data: 'id',
                width: "15%",
                orderable: false,
                render: function (data, type, row) {
                    const lockBtn = `
                    <form action="/Admin/User/UserLock/${data}" method="post" class="d-inline">
                        <input type="hidden" name="__RequestVerificationToken" value="${token}" />
                        <button type="submit" class="btn btn-sm ${row.locked ? 'btn-success' : 'btn-danger'}">
                            <i class="bi ${row.locked ? 'bi-unlock-fill' : 'bi-lock-fill'} me-1"></i>
                            ${row.locked ? 'Unlock' : 'Lock'}
                        </button>
                    </form>`;

                    return `
                        ${lockBtn}
                        <a href="/Admin/User/RoleManagement/${data}" class="btn btn-sm btn-warning">
                            <i class="bi bi-person-badge me-1"></i> Role
                        </a>
                        <a href="/Admin/User/ChangePassword/${data}" class="btn btn-sm btn-danger">
                            <i class="bi bi-fingerprint me-1"></i> Password
                        </a>
                        `;
                }
            }
        ]
    });
});



