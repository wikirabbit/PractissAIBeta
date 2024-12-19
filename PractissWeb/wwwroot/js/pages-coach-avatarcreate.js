function updateSubmitButtonState() {
    var avatarName = document.getElementById('avatarNameTextbox').value;
    var avatarImage = document.getElementById('avatarImageDropDown').value;
    var avatarVoice = document.getElementById('avatarVoice').value;
    var submitButton = document.getElementById('submitButton');

    var allFieldsFilled = avatarName && avatarImage && avatarVoice;
    submitButton.disabled = !allFieldsFilled;

    // Mapping of dropdown values to image paths
    var imageMappings = {
        "cm": "/img/avatars/reference-cm.png",
        "cf": "/img/avatars/reference-cf.png",
        "hm": "/img/avatars/reference-hm.png",
        "hf": "/img/avatars/reference-hf.png",
        "cmk": "/img/avatars/reference-cmk.png",
        "cfk": "/img/avatars/reference-cfk.png"
    };

    // Update the avatar image display
    avatarImageDisplay.src = imageMappings[avatarImage] || "/img/avatars/reference-blank.png";
}

function createNewAvatar() {
    window.location.href = '/Coach/AvatarCreate';
}

function playAvatarVoice(voiceId, personality) {
    var audioSource = "/audio/" + voiceId + (personality == '' ? '' : '-' + personality) + "-sample.mp3";

    var audio = document.getElementById('avatarAudio');
    audio.src = audioSource;
    audio.play();
}

function editAvatar(avatarId) {
    window.location.href = '/Coach/AvatarEdit/' + avatarId;
}


