function updateSubmitButtonState() {
  var moduleInput = document.getElementById('moduleId').value;
  var addModuleButton = document.getElementById('addModuleButton');

  addModuleButton.disabled = !(moduleInput);
}

function showConfirmationModal(coachId, moduleId, learnerId) {
    // Save the IDs in the confirm button for later use
    var confirmButton = document.getElementById('confirmDeleteButton');
    confirmButton.setAttribute('data-module-id', moduleId);
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
    var moduleId = button.getAttribute('data-module-id');
    var coachId = button.getAttribute('data-coach-id');
    var learnerId = button.getAttribute('data-learner-id');

    // Call the original removal function
    removeLearnerFromModule(coachId, moduleId, learnerId);

    // Optionally close the modal after action
    var confirmationModal = bootstrap.Modal.getInstance(document.getElementById('confirmationModal'));
    confirmationModal.hide();
}


function removeLearnerFromModule(coachId, moduleId, learnerId) {
    fetch('/Coach/ClientSettings/' + learnerId + '?handler=RemoveModule', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: `coachId=${coachId}&moduleId=${moduleId}&learnerId=${learnerId}`
    })
        .then(response => {
            if (response.ok) {
                location.reload(); // Reload the page or update the UI as needed
            }
        });
}
