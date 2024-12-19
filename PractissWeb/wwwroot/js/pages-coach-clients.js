function manageClient(clientId) {
    window.location.href = '/Coach/ClientSettings/' + clientId;
}

function showConfirmationModal(coachId, learnerId) {
    // Save the IDs in the confirm button for later use
    var confirmButton = document.getElementById('confirmDeleteButton');
    confirmButton.setAttribute('data-coach-id', coachId);
    confirmButton.setAttribute('data-learner-id', learnerId);

    // Show the modal
    var confirmationModal = new bootstrap.Modal(document.getElementById('confirmationModal'));
    confirmationModal.show();

    // Ensure we only bind the event once
    confirmButton.removeEventListener('click', confirmDelete);
    confirmButton.addEventListener('click', confirmDelete);
}

function confirmDelete() {
    var button = this;
    var coachId = button.getAttribute('data-coach-id');
    var learnerId = button.getAttribute('data-learner-id');

    // Call the original removal function
    removeLearner(coachId, learnerId);

    // Optionally close the modal after action
    var confirmationModal = bootstrap.Modal.getInstance(document.getElementById('confirmationModal'));
    confirmationModal.hide();
}

function removeLearner(coachId, learnerId) {
    fetch('/Coach/Clients?handler=RemoveLearner', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: `coachId=${coachId}&learnerId=${learnerId}`
    })
        .then(response => {
            if (response.ok) {
                location.reload(); // Reload the page or update the UI as needed
            }
        });
}

$(document).ready(function () {
    $('#emailPartialSubmit').on('click', function (e) {
        e.preventDefault();
        var email = $('#emailField').val();

        $.ajax({
            url: '/Coach/Clients?handler=NameFromEmail' + '&email=' + encodeURIComponent(email),
            method: 'GET', // Explicitly stating the method, though GET is usually the default
            success: function (response) {
                $('#firstNameField').val(response.firstName);
                $('#lastNameField').val(response.lastName);
                $('#descriptionPartialSubmit').prop('disabled', false);
                $('.btn-wizard-save').prop('disabled', !(response.firstName && response.lastName));
            },
            error: function () {
                alert('Error fetching name.');
            }
        });
    });
});


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