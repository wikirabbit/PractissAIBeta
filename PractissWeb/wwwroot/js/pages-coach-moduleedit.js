function updateSaveButtonState() {
	var moduleNameInput = document.getElementById('moduleNameTextbox').value;
	var avatarDropdown = document.getElementById('avatarDropdown').value;
	var moduleDescriptionTextArea = document.getElementById('moduleDescriptionTextArea').value;
	var situationTextArea = document.getElementById('situationTextArea').value;
	var saveButton = document.getElementById('saveChangesButton');


	var allFieldsFilled = (moduleNameInput && avatarDropdown && moduleDescriptionTextArea && situationTextArea);
	saveButton.disabled = !allFieldsFilled;
}