// DevHub Gear - Client-side interactive helpers
document.addEventListener("DOMContentLoaded", function () {
    // Auto-dismiss alert banners after 5 seconds if desired
    const alerts = document.querySelectorAll(".gh-alert");
    alerts.forEach(function (alert) {
        setTimeout(function () {
            alert.style.transition = "opacity 0.5s ease";
            alert.style.opacity = "0";
            setTimeout(() => alert.remove(), 500);
        }, 5000);
    });
});
