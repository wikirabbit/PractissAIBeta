// User Response Management

let finalTranscript = "";
let interimTranscript = "";
let streaming = true;

async function processUserResponse() {
    if (interimTranscript != null && interimTranscript.trim() !== '') {
        finalTranscript += ' ' + interimTranscript;
    }

    console.log(moduleAssignmentId + ' : ' + finalTranscript);

    const urlParams = new URLSearchParams();
    urlParams.append('moduleAssignmentId', moduleAssignmentId);
    urlParams.append('userResponse', finalTranscript);

    valSpeakingTurnIndex++;
    if (valSpeakingTurnIndex > 0)
        SetValBackground("thinking");

    await SpeakAudioFromUrl(`/Interaction/GetNextResponseStream?${urlParams}`);
}

async function SpeakAudioFromUrl(audioUrl) {
    const audioPlayer = new Audio();
    audioPlayer.src = audioUrl;

    audioPlayer.addEventListener('ended', function () {
        SetValBackground("listening");
    });

    audioPlayer.addEventListener('canplay', async function () {
        console.log('data available to play');
        await waitForInteractionStart();
        SetValBackground("speaking");
    });

    audioPlayer.addEventListener('error', function (e) {
        console.error('Error loading or playing audio:', e);
        showAudioClarityIssueModal();
    });

    // Load the audio from the URL
    audioPlayer.load();

    //// Wait for user to press the start button
    await waitForInteractionStart();

    // Play the audio
    audioPlayer.play();
}
