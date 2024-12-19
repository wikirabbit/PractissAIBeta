// User Response Management

let finalTranscript = "";
let interimTranscript = "";
let streaming = false;

async function processUserResponse() {
    finalTranscript += ' ' + interimTranscript;
    console.log(moduleAssignmentId + ' : ' + finalTranscript);

    const urlParams = new URLSearchParams();
    urlParams.append('moduleAssignmentId', moduleAssignmentId);
    urlParams.append('userResponse', finalTranscript);

    if (valSpeakingTurnIndex > 0)
        SetValBackground("thinking");

    try {
        // Make the fetch call and wait for the response
        const response = await fetch(`/Interaction/GetNextResponse?${urlParams}`, {
            method: 'GET',
        });
        const base64String = await response.text(); // Extract the text from the response object

        if (base64String !== "") {
            // Convert base64 string to byte array
            const responseAudioBytes = Uint8Array.from(atob(base64String), c => c.charCodeAt(0));
            await SpeakAudioFromBytes(responseAudioBytes); // Wait for the audio to be spoken
            valSpeakingTurnIndex++;
        } else {
            // Handle the case where the response is empty
            console.log('No audio response received.');
            SetValBackground("listening");
        }
    } catch (error) {
        console.error('Error:', error);
        SetValBackground("error");
    }

    interimTranscript = "";
    finalTranscript = "";
}

async function SpeakAudioFromBytes(audioBytes) {
    try {
        console.log('data available to play');
        // Wait for user to press the start button
        await waitForInteractionStart();
        SetValBackground("speaking");

        // Convert the byte array to a Blob
        const audioBlob = new Blob([audioBytes], { type: 'audio/mp3' });

        // Create an Object URL from the Blob
        const audioUrl = URL.createObjectURL(audioBlob);

        // Use the Audio constructor to play the audio
        const audio = new Audio(audioUrl);

        audio.onended = () => {
            SetValBackground("listening");
        };

        audio.oncanplay = () => {
            SetValBackground("speaking");
        };

        audio.onerror = (e) => {
            console.error('An error occurred with the audio playback.', e);
            SetValBackground("error");
        };

        audio.play();
    } catch (error) {
        console.error('Error in SpeakAudioFromBytes:', error);
        SetValBackground("error");
    }
}
