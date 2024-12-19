document.addEventListener('DOMContentLoaded', function () {
    if (window.alertOnError) {
        // Save the original console.error function
        const originalConsoleError = console.error;

        // Override console.error
        console.error = function (message) {
            // Optionally, call the original console.error function
            originalConsoleError.apply(console, arguments);

            // Display an alert with the error message
            alert(Array.prototype.join.call(arguments, " "));
        };
    }
});
