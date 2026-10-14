$(document).ready(function () {
    // DataTables fetches the data itself, so ajax.reload() works after a delete
    $('#productTable').DataTable({
        ajax: {
            url: '/admin/product/getall',
            dataSrc: '' // the API returns a plain array
        },
        columns: [
            { data: 'title', width: "25%", render: function (data) { return '<span class="fw-semibold">' + data + '</span>'; } },
            { data: 'isbn', width: "15%", render: function (data) { return '<span class="small text-secondary font-monospace">' + data + '</span>'; } },
            { data: 'author', width: "15%" },
            {
                data: 'price', width: "10%", className: 'text-end',
                render: function (data) { return '<span class="fw-semibold">' + data.toLocaleString('nb-NO', { style: 'currency', currency: 'NOK' }) + '</span>'; }
            },
            {
                data: 'category.name', width: "15%",
                render: function (data) {
                    return '<span style="font-size:12px;font-weight:600;padding:4px 12px;border-radius:50px;background:#EEF2FF;color:#13294B;">' + data + '</span>';
                }
            }, // Matcher den kategori f.eks. { category: { name: "Elektronik" } }
            {
                data: 'id',
                width: "20%",
                render: function (data) {
                    // 'data' holds the id from API

                    return `
                            <a href="/admin/product/upsert/${data}" class="btn btn-sm btn-outline-dark me-1">
                             <i class="bi bi-pencil-square me-1"></i> Edit
                            </a>

                            <button type="button" onclick="deleteProduct(${data})" class="btn btn-sm btn-outline-danger">
                             <i class="bi bi-trash me-1"></i> Delete
                            </button>
                            `;
                }
            },
        ]
    });
});
function deleteProduct(id) {
    Swal.fire({
        title: 'Delete this product?',
        text: 'This cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#C8102E',
        cancelButtonColor: '#13294B',
        confirmButtonText: 'Delete product'
    }).then((result) => {
        if (!result.isConfirmed) return;

        $.ajax({
            url: '/admin/product/delete/' + id,
            type: 'DELETE',
            success: function (data) {
                if (data.success) {
                    $('#productTable').DataTable().ajax.reload();
                    Swal.fire({ icon: 'success', title: data.message, timer: 1500, showConfirmButton: false });
                } else {
                    Swal.fire({ icon: 'error', title: data.message });
                }
            },
            error: function () {
                Swal.fire({ icon: 'error', title: 'The product could not be deleted' });
            }
        });
    });
}