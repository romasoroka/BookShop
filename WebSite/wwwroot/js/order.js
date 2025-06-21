var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll"
        },
        "columns": [
            { "data": "id", "width": "10%" },
            {
                "data": "user.userName",
                "width": "15%",
                "render": function (data) {
                    return data || "Guest";
                }
            },
            {
                "data": "orderDate",
                "width": "15%",
                "render": function (data) {
                    return new Date(data).toLocaleDateString();
                }
            },
            {
                "data": "totalPrice",
                "width": "15%",
                "render": function (data) {
                    return parseFloat(data).toLocaleString('pl-PL', {
                        style: 'currency',
                        currency: 'PLN',
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    });
                }
            },
            {
                "data": "status",
                "width": "15%",
                "render": function (data) {
                    let badgeClass = 'badge ';
                    switch (data.toLowerCase()) {
                        case 'Ordered': badgeClass += 'bg-warning'; break;
                        case 'Sent': badgeClass += 'bg-primary'; break;
                        case 'Delivered': badgeClass += 'bg-success'; break;
                        case 'Finished': badgeClass += 'bg-success'; break;
                        default: badgeClass += 'bg-secondary';
                    }
                    return `<span class="${badgeClass}">${data}</span>`;
                }
            },
            {
                "data": "id",
                "width": "30%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <a href="/Admin/Order/Upsert?id=${data}" class="btn btn-primary">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onClick=Delete('/Admin/Order/Delete/${data}') class="btn btn-danger">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                            <a href="/Admin/Order/Details/${data}" class="btn btn-info">
                                <i class="bi bi-eye-fill"></i> Details
                            </a>
                        </div>
                    `;
                }
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                },
                error: function (err) {
                    toastr.error(err.responseJSON.message);
                }
            });
        }
    });
}