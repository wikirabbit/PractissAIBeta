// Add your custom script in this file
document.addEventListener('DOMContentLoaded', function () {
    var cancelButton = document.querySelector('button[type="reset"]');
    if (cancelButton) {
        cancelButton.addEventListener('click', function (event) {
            event.preventDefault(); // Prevent the default form reset behavior
            window.history.back(); // Go back to the previous page
        });
    }

    // aside toggle button
    var toggleButton = document.getElementById('toggle-btn');
    toggleButton.addEventListener('click', function () {
        $("#layout-menu, .layout-page").toggleClass("active");
        if (window.matchMedia('(max-width: 1024.98px)').matches) {
            $("body").toggleClass("overflow-hidden");
        }
    });

    // close aside on clicking the main-layout
    var layoutPage = document.querySelector('.layout-page');
    if (layoutPage != null) {
        layoutPage.addEventListener('click', function () {
            if (window.matchMedia('(max-width: 1023.98px)').matches) {
                $("#layout-menu, .layout-page").addClass("active");
            }
        });
    }

    // close aside by default on small devices
    if (window.matchMedia('(max-width: 1023.98px)').matches) {
        $("#layout-menu, .layout-page").addClass("active");
    }
});

