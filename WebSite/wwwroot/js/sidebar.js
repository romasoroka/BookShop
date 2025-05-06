// wwwroot/js/sidebar.js
function toggleSidebar() {
    const sidebar = document.getElementById("mySidebar");
    const overlay = document.querySelector('.main-content-overlay');
    const body = document.body;

    if (sidebar.style.width === "250px") {
        sidebar.style.width = "0";
        overlay.classList.remove('active');
        body.classList.remove('sidebar-open');
    } else {
        sidebar.style.width = "250px";
        overlay.classList.add('active');
        body.classList.add('sidebar-open');
    }
}

// Close sidebar when clicking outside
document.addEventListener('click', function (event) {
    const sidebar = document.getElementById("mySidebar");
    const burgerBtn = document.querySelector('.burger-menu-btn');
    if (sidebar.style.width === "250px" &&
        !sidebar.contains(event.target) &&
        event.target !== burgerBtn &&
        !burgerBtn.contains(event.target)) {
        sidebar.style.width = "0";
        document.getElementById("main").style.marginLeft = "0";
    }
});

document.addEventListener('DOMContentLoaded', function () {
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        const navbarHeight = navbar.offsetHeight;
        document.documentElement.style.setProperty('--navbar-height', `${navbarHeight}px`);
    }
});