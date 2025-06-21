function deleteCartItem(url) {
    Swal.fire({
        title: "Ви впевнені?",
        text: "Цю дію не можна скасувати!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Так, видалити!",
        cancelButtonText: "Скасувати"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST', // Використовуємо POST для DELETE операцій (або додайте токен)
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                data: { id: url.split('/').pop() }, // Вилучаємо ID з URL
                success: function (data) {
                    if (data.success) {
                        // Оновлюємо сторінку або таблицю
                        location.reload(); // або ваш спосіб оновлення
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                },
                error: function (xhr) {
                    toastr.error("Сталася помилка: " + xhr.responseText);
                }
            });
        }
    });
}