// Disable next button on invalid inputs

$(document).ready(function () {
    // Initially disable the submit button
    $('#emailPartialSubmit').prop('disabled', true);

    $('#emailField').on('input', function () {
        // Basic regex for email validation
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        var isValidEmail = emailRegex.test($(this).val());

        // Enable the submit button if the email is valid, otherwise disable it
        $('#emailPartialSubmit').prop('disabled', !isValidEmail);
    });
});

$(document).ready(function () {
    // Initially disable the submit button
    $('.btn-wizard-save').prop('disabled', true);

    // Function to check the state of the first name and last name fields
    function updateSubmitButtonState() {
        var firstName = $('#firstNameField').val().trim();
        var lastName = $('#lastNameField').val().trim();

        // Enable the submit button if both fields are not empty
        $('.btn-wizard-save').prop('disabled', !(firstName && lastName));
    }

    // Run the state update once on document ready to set the initial state of the button
    updateSubmitButtonState();

    // Attach the update function to the 'input' event for both fields
    $('#firstNameField, #lastNameField').on('input', updateSubmitButtonState);
});


document.addEventListener('DOMContentLoaded', function () {
    // Search function
    function searchUsers(inputId, tableId) {
        var searchText = document.getElementById(inputId).value.toLowerCase();
        document.querySelectorAll(`${tableId} tbody tr`).forEach(function (row) {
            var title = row.querySelector('td:nth-child(1)').textContent.toLowerCase();
            row.style.display = title.includes(searchText) ? '' : 'none';
        });
    }

    // Event listeners for search inputs
    document.getElementById('allUsersSearchTextbox').addEventListener('keyup', function () {
        searchUsers('allUsersSearchTextbox', '#allUsersTable');
    });
});

function showConfirmationModal(userId, firstName, lastName) {
    // Save the IDs in the confirm button for later use
    var confirmButton = document.getElementById('confirmDeleteButton');
    confirmButton.setAttribute('data-user-id', userId);
    var userNameLabel = document.getElementById('userNameLabel');
    userNameLabel.innerText = firstName + ' ' + lastName;

    // Show the modal
    var confirmationModal = new bootstrap.Modal(document.getElementById('confirmationModal'));
    confirmationModal.show();

    // Ensure we only bind the event once
    confirmButton.removeEventListener('click', confirmDelete);
    confirmButton.addEventListener('click', confirmDelete);
}

function confirmDelete() {
    var button = this;
    var userId = button.getAttribute('data-user-id');

    // Call the original removal function
    deleteUser(userId);

    // Optionally close the modal after action
    var confirmationModal = bootstrap.Modal.getInstance(document.getElementById('confirmationModal'));
    confirmationModal.hide();
}

function deleteUser(userId) {
    fetch('/Admin/Whitelist?handler=DeleteUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: `userId=${userId}`
    })
        .then(response => {
            if (response.ok) {
                location.reload(); // Reload the page or update the UI as needed
            }
        });
}