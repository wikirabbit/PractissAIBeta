function createNewModule() {
	window.location.href = '/Coach/ModuleCreate';
}

function editModule(moduleId) {
	window.location.href = '/Coach/ModuleEdit/' + moduleId;
}

function manageModule(moduleId) {
	window.location.href = '/Coach/ModuleSettings/' + moduleId;
}

function demoModule(mode, moduleId) {
	window.location.href = '/Common/' + mode + '/' + moduleId;
}

document.addEventListener('DOMContentLoaded', function () {
    // Search function
    function searchModules(inputId, tableId) {
        var searchText = document.getElementById(inputId).value.toLowerCase();
        document.querySelectorAll(`${tableId} tbody tr`).forEach(function (row) {
            var title = row.querySelector('td:nth-child(1)').textContent.toLowerCase();
            row.style.display = title.includes(searchText) ? '' : 'none';
        });
    }

    // Event listeners for search inputs
    document.getElementById('authoredModulesSearchTextbox').addEventListener('keyup', function () {
        searchModules('authoredModulesSearchTextbox', '#authoredModulesTable');
    });

    document.getElementById('bookmarkedModulesSearchTextbox').addEventListener('keyup', function () {
        searchModules('bookmarkedModulesSearchTextbox', '#bookmarkedModulesTable');
    });
});

function showConfirmationModal(moduleId) {
    // Save the IDs in the confirm button for later use
    var confirmButton = document.getElementById('confirmDeleteButton');
    confirmButton.setAttribute('data-module-id', moduleId);

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

    // Call the original removal function
    deleteModule(moduleId);

    // Optionally close the modal after action
    var confirmationModal = bootstrap.Modal.getInstance(document.getElementById('confirmationModal'));
    confirmationModal.hide();
}

function deleteModule(moduleId) {
    fetch('/Coach/ModuleLibrary?handler=DeleteModule', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: `moduleId=${moduleId}`
    })
        .then(response => {
            if (response.ok) {
                location.reload(); // Reload the page or update the UI as needed
            }
        });
}