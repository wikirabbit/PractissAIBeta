function updateSubmitButtonState() {
  var learnerInput = document.getElementById('learnerEmail').value;
  var addLearnerButton = document.getElementById('addLearnerButton');

  addLearnerButton.disabled = !(learnerInput);
}

function removeLearnerFromModule(coachId, moduleId, learnerId) {
    fetch('/Coach/ModuleSettings/' + moduleId + '?handler=RemoveLearner', {
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
