document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("formAuthentication");
    const emailInput = document.getElementById("email");
    const codeInput = document.getElementById("code");
    const submitEmailButton = document.getElementById("submitEmailButton");
    const submitLoginCodeButton = document.getElementById("submitLoginCodeButton");
    const invalidCodeMessage = document.getElementById("invalidCodeMessage");
    const resendCodeLink = document.getElementById("resendCode");

    resendCodeLink.addEventListener("click", function (e) {
        document.getElementById("submitEmail").style.display = "block";
        document.getElementById("inputLoginCode").style.display = "none";
        document.getElementById("submitLoginCode").style.display = "none";
        invalidCodeMessage.style.display = "none";
        submitEmailButton.disabled = false;
    });

    submitEmailButton.addEventListener("click", function () {
        event.preventDefault(); // Prevent the form from submitting traditionally

        // Check if the email input field is empty
        if (!emailInput.value.trim()) {
            // Optionally, alert the user or handle the empty input case more gracefully
            console.log("Email input is empty. Please enter an email address.");
            return; // Exit the function early if the email input is empty
        }

        const formData = new FormData();
        formData.append("email", emailInput.value);
        formData.append("code", codeInput.value);

        // Add the antiforgery token to the request
        const token = document.getElementsByName("__RequestVerificationToken")[0].value;
        formData.append("__RequestVerificationToken", token);

        submitEmailButton.disabled = true;
        emailInput.disabled = true;
        // Replace '/Auth/Login' with the correct endpoint that validates the login code
        fetch('/Auth/Login?handler=Email', {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': token // This might not be necessary depending on your setup
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.isValid) {
                    document.getElementById("submitEmail").style.display = "none";
                    document.getElementById("inputLoginCode").style.display = "block";
                    document.getElementById("submitLoginCode").style.display = "block";
                    document.getElementById("code").value = "";
                }
                else {
                    if (data.message == 'whitelist') {
                        window.location.href = "/Auth/Register"
                    }
                    else {
                        window.location.href = "/Misc/ClosedBeta";
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                // Handle any errors that occur during the fetch operation
            });
    });

    submitLoginCodeButton.addEventListener("click", function (event) {
        event.preventDefault(); // Prevent the form from submitting traditionally

        // Check if the email input field is empty
        if (!emailInput.value.trim()) {
            // Optionally, alert the user or handle the empty input case more gracefully
            console.log("Email input is empty. Please enter an email address.");
            return; // Exit the function early if the email input is empty
        }

        // Check if the email input field is empty
        if (!codeInput.value.trim()) {
            // Optionally, alert the user or handle the empty input case more gracefully
            console.log("Login code input is empty. Please enter a login code.");
            return; // Exit the function early if the email input is empty
        }

        const formData = new FormData();
        formData.append("email", emailInput.value);
        formData.append("code", codeInput.value);

        // Add the antiforgery token to the request
        const token = document.getElementsByName("__RequestVerificationToken")[0].value;
        formData.append("__RequestVerificationToken", token);

        submitLoginCodeButton.disabled = true;

        // Replace '/Auth/Login' with the correct endpoint that validates the login code
        fetch('/Auth/Login?handler=Code', {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': token // This might not be necessary depending on your setup
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.isValid) {
                    switch (data.message) {
                        case "usernotinwhitelist":
                            window.location.href = "/Misc/ClosedBeta";
                            break;
                        case "userdeletedfromwhitelist":
                            window.location.href = "/Misc/ClosedBeta";
                            break;
                        case "usernotfound":
                            window.location.href = "/Auth/Register";
                            break;
                        case "subscriptionexpired":
                            window.location.href = "/Common/Billing";
                            break;
                        case "licenseunassigned":
                            window.location.href = "/Common/Billing";
                            break;
                        default:
                            window.location.href = "/Misc/Disclaimer";
                    }
                } else {
                    // Show the "Invalid Code" message if the login code is incorrect
                    invalidCodeMessage.style.display = "block";
                    submitLoginCodeButton.disabled = false;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                // Handle any errors that occur during the fetch operation
            });
    });

    // Trigger form submit on enter keypress
    // Function to handle key press in the email input field
    emailInput.addEventListener('keypress', function (event) {
        if (event.key === "Enter") {
            event.preventDefault(); // Prevent the default behavior of the Enter key
            submitEmailButton.click(); // Programmatically click the email submit button
        }
    });

    // Function to handle key press in the code input field
    codeInput.addEventListener('keypress', function (event) {
        if (event.key === "Enter") {
            event.preventDefault(); // Prevent the default behavior of the Enter key
            submitLoginCodeButton.click(); // Programmatically click the code submit button
        }
    });
});
