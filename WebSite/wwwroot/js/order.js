var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll"
        },
        "order": [[2, "desc"]], // Нові замовлення будуть зверху
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.13.6/i18n/uk.json"
        },
        "columns": [
            { "data": "id", "width": "5%" },
            {
                "data": "user.userName",
                "width": "15%",
                "render": function (data) {
                    return data || "Гість";
                }
            },
            {
                "data": "orderDate",
                "width": "20%",
                "render": function (data, type) {
                    // Для сортування використовуємо сирі дані (ISO формат)
                    if (type === 'sort' || type === 'type') {
                        return data;
                    }
                    // Для відображення — український формат
                    let date = new Date(data);
                    return date.toLocaleString('uk-UA');
                }
            },
            {
                "data": "totalPrice",
                "width": "10%",
                "render": function (data) {
                    // Форматування ціни у злотих (PLN)
                    return parseFloat(data).toLocaleString('pl-PL', {
                        style: 'currency',
                        currency: 'PLN'
                    });
                }
            },
            {
                "data": "status",
                "width": "15%",
                "render": function (data) {
                    let badgeClass = 'badge rounded-pill ';
                    let statusText = data;
                    let lowerData = data.toLowerCase();

                    // Перевіряємо статус (і англійською, і українською)
                    if (lowerData === 'ordered' || lowerData === 'pending' || lowerData === 'замовлено' || lowerData === 'очікує') {
                        badgeClass += 'bg-warning text-dark';
                        statusText = "Замовлено";
                    }
                    else if (lowerData === 'approved' || lowerData === 'підтверджено') {
                        badgeClass += 'bg-info text-dark';
                        statusText = "Підтверджено";
                    }
                    else if (lowerData === 'sent' || lowerData === 'shipped' || lowerData === 'відправлено') {
                        badgeClass += 'bg-primary';
                        statusText = "Відправлено";
                    }
                    else if (lowerData === 'delivered' || lowerData === 'finished' || lowerData === 'завершено') {
                        badgeClass += 'bg-success';
                        statusText = "Завершено";
                    }
                    else if (lowerData === 'cancelled' || lowerData === 'rejected' || lowerData === 'скасовано') {
                        badgeClass += 'bg-danger';
                        statusText = "Скасовано";
                    }
                    else {
                        badgeClass += 'bg-secondary';
                    }

                    return `<span class="${badgeClass}" style="padding: 0.5em 1em; min-width: 100px; display: inline-block;">${statusText}</span>`;
                }
            },
            {
                "data": "id",
                "width": "25%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <a href="/Admin/Order/Upsert?id=${data}" class="btn btn-primary mx-1" title="Редагувати">
                                <i class="bi bi-pencil-square"></i>
                            </a>
                            <a href="/Admin/Order/Details/${data}" class="btn btn-info mx-1" title="Деталі">
                                <i class="bi bi-eye-fill"></i>
                            </a>
                            <a onClick=Delete('/Admin/Order/Delete/${data}') class="btn btn-danger mx-1" title="Видалити">
                                <i class="bi bi-trash-fill"></i>
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
        title: "Ви впевнені?",
        text: "Ви не зможете відновити це замовлення!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Так, видалити!",
        cancelButtonText: "Скасувати"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message || "Успішно видалено");
                },
                error: function (err) {
                    toastr.error(err.responseJSON?.message || "Помилка при видаленні");
                }
            });
        }
    });
}