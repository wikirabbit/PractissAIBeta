function viewInsightDetails(insightId) {
	window.open('/Learner/InsightDetails/' + insightId);
}

function showConfirmationModal(insightId) {
    // Save the IDs in the confirm button for later use
    var confirmButton = document.getElementById('confirmDeleteButton');
    confirmButton.setAttribute('data-insight-id', insightId);

    // Show the modal
    var confirmationModal = new bootstrap.Modal(document.getElementById('confirmationModal'));
    confirmationModal.show();

    // Ensure we only bind the event once
    confirmButton.removeEventListener('click', confirmDelete);
    confirmButton.addEventListener('click', confirmDelete);
}

function confirmDelete() {
    var button = this;
    var insightId = button.getAttribute('data-insight-id');

    // Call the original removal function
    deleteInsight(insightId);

    // Optionally close the modal after action
    var confirmationModal = bootstrap.Modal.getInstance(document.getElementById('confirmationModal'));
    confirmationModal.hide();
}

function deleteInsight(insightId) {
    fetch('/Learner/CommunicationInsights?handler=DeleteInsight', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: `insightId=${insightId}`
    })
        .then(response => {
            if (response.ok) {
                location.reload(); // Reload the page or update the UI as needed
            }
        });
}