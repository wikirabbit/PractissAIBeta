function showProgressBar() {
	// Reset all fields
	$('#moduleNameTextbox').val('');
	$('#moduleDescriptionTextArea').val('');
	$('#avatarDropdown').val('');
	$('#situationTextArea').val('');
	$('#evaluationTextArea').val('');

	// Unhide the progress bar div
	const progressBarDiv = document.getElementById('progressBarDiv');
	progressBarDiv.hidden = false;

	// Initialize progress bar
	let progress = 0;
	const progressBar = document.getElementById('reportProgressBar');
	progressBar.style.display = 'block';
	const interval = setInterval(() => {
		progress += 1; // Increment progress
		if (progress >= 100) {
			progress = 100; // Ensure progress does not exceed 100%
		}

		// Check if generation is complete
		if ($('#moduleNameTextbox').val() != null && $('#moduleNameTextbox').val() != '') {
			clearInterval(interval); // Stop the interval
		}

		progressBar.style.width = `${progress}%`;
		progressBar.setAttribute('aria-valuenow', progress);
	}, 300);
}

$(document).ready(function () {
	$('#rawInputPartialSubmit').on('click', function (e) {
		e.preventDefault();
		var rawInput = $('#rawInputTextArea').val();

		showProgressBar();

		$.ajax({
			url: '/Coach/ModuleCreate?handler=ProcessRawInput' + '&rawInput=' + encodeURIComponent(rawInput),
			method: 'GET', // Explicitly stating the method, though GET is usually the default
			success: function (response) {
				// Hide the progress bar div
				const progressBarDiv = document.getElementById('progressBarDiv');
				progressBarDiv.hidden = true;

				$('#moduleNameTextbox').val(response.moduleName);
				$('#moduleDescriptionTextArea').val(response.moduleDescription);
				$('#avatarDropdown').val(response.selectedAvatar);
				$('#situationTextArea').val(response.situation);
				$('#evaluationTextArea').val(response.evaluation);

				$('#descriptionPartialSubmit').prop('disabled', false);
				$('#avatarPartialSubmit').prop('disabled', false)
				$('#situationPartialSubmit').prop('disabled', false);
				$('.btn-wizard-save').prop('disabled', false);

				if (response.selectedAvatar == 'Patrick' || response.selectedAvatar == 'Nelly') {
					$('#angryAvatarWarning').prop('hidden', false);
				}
			},
			error: function () {
				alert('Error fetching name.');
			}
		});
	});
});


// Disable the Next buttons if the invalid values

$(document).ready(function () {
	$('#rawInputTextArea').on('input', function () {
		// Enable or disable the button based on the text length
		var isLongEnough = $(this).val().length > 10;
		$('#rawInputPartialSubmit').prop('disabled', !isLongEnough);
	});
});

$(document).ready(function () {
	// Function to check the state of both fields and update the button's disabled property
	function updateSubmitButtonState() {
		var moduleName = $('#moduleNameTextbox').val();
		var moduleDescription = $('#moduleDescriptionTextArea').val();

		// Check if both fields are not just "Generating..." or empty
		var enableButton = moduleName !== '' && moduleName !== 'Generating...' &&
			moduleDescription !== '' && moduleDescription !== 'Generating...';

		$('#descriptionPartialSubmit').prop('disabled', !enableButton);
	}

	// Run the state update once on document ready to set the initial state of the button
	updateSubmitButtonState();

	// Attach the update function to the 'input' event for both fields
	$('#moduleNameTextbox, #moduleDescriptionTextArea').on('input', updateSubmitButtonState);
});

$(document).ready(function () {
	$('#avatarDropdown').on('change', function () {
		// Check if the selected option is not the default
		var isValidSelection = $(this).val() !== '';
		$('#avatarPartialSubmit').prop('disabled', !isValidSelection);
	});
});

$(document).ready(function () {
	// Disable the submit button initially
	$('#situationPartialSubmit').prop('disabled', true);

	$('#situationTextArea').on('input', function () {
		// Enable the submit button if the textarea is not empty
		var textAreaContent = $(this).val();
		$('#situationPartialSubmit').prop('disabled', !textAreaContent.trim());
	});
});