$(document).ready(function () {
    // DataTables fetches the data itself, so ajax.reload() works after a delete
    $('#productTable').DataTable({
        ajax: {
            url: '/admin/product/getall',
            dataSrc: '' // the API returns a plain array
        },
        columns: [
            { data: 'title', width: "25%" },
            { data: 'isbn', width: "15%" },
            { data: 'author', width: "15%" },
            { data: 'price', render: function (data) { return '$' + data.toFixed(2); }, width: "10%" },
            { data: 'category.name', width: "15%" }, // Matcher den relaterede kategori f.eks. { category: { name: "Elektronik" } }
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