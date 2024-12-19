let moduleAssignmentId = document.getElementById('moduleAssignmentIdDiv').getAttribute('data-my-variable');
console.log(moduleAssignmentId);

// Prefetch first response
document.addEventListener('DOMContentLoaded', (event) => {
    processUserResponse();

    let modal = new bootstrap.Modal(document.getElementById('preloadModal'), {
        backdrop: 'static', // Prevent closing by clicking outside or pressing escape
        keyboard: false // Prevent closing with the keyboard
    });
    modal.show();

    // Initialize progress bar
    let progress = 0;
    const progressBar = document.getElementById('preloadProgressBar');
    const interval = setInterval(() => {
        progress += 16.67; // Increment progress to reach 100% in 6 seconds
        progressBar.style.width = `${progress}%`;
        progressBar.setAttribute('aria-valuenow', progress);

        if (progress >= 100) {
            clearInterval(interval);
            setTimeout(() => { modal.hide(); }, 500); // Add slight delay before closing modal for smooth transition
        }
    }, 1000); // Update every second

    // Listen for the modal to be completely hidden, then start intro.js
    document.getElementById('preloadModal').addEventListener('hidden.bs.modal', function (event) {
        if (showUsageGuide) {
            // Start intro.js guide
            introJs().setOptions({
                steps: [
                    {
                        element: document.getElementById('startBtn'),
                        intro: 'Press the play button to begin the roleplay. ' + avatarName + ' will be in character and ready to begin the interaction with you.',
                        position: 'right'
                    }]
            }).start();
        }
    });
});

let showMicButtonGuide = true;
function ShowMicButtonGuide() {
    if (showUsageGuide) {
        // Start intro.js guide
        introJs().setOptions({
            steps: [
                {
                    element: document.getElementById('microphoneBtnPassive'),
                    intro: 'Press the Mic button to speak your response to ' + avatarName + ', and again when finished.',
                    position: 'right'
                },
                {
                    element: document.getElementById('stopBtn'),
                    intro: 'Press the stop button to end this roleplay.',
                    position: 'right'
                }]
        }).start();
    }
}

function showAudioClarityIssueModal() {
    $('#modalAudioClarityIssue').modal('show');
}

// Trigger Loading Error Modal
function showLoadingErrorModal() {
    $('#modalLoadingError').modal('show');
}

// Trigger Unexpected Error Modal
function showUnexpectedErrorModal() {
    $('#modalUnexpectedError').modal('show');
}

  // Hide modals on "Ok" button click
$('#modalAudioClarityIssue .btn-primary, #modalUnexpectedError .btn-primary').click(function () {
    $(this).closest('.modal').modal('hide');
});



const valSpeakingBackground = document.getElementById('valSpeakingBackground');
valSpeakingBackground.style.display = 'none';

const valThinkingBackground = document.getElementById('valThinkingBackground');
valThinkingBackground.style.display = 'none';

const valListeningBackground = document.getElementById('valListeningBackground');
valListeningBackground.style.display = 'none';

const microphoneBtnActive = document.getElementById('microphoneBtnActive');
microphoneBtnActive.style.display = 'none';

const microphoneBtnPassive = document.getElementById('microphoneBtnPassive');
microphoneBtnPassive.style.display = 'block';

const stopBtn = document.getElementById('stopBtn');
stopBtn.style.display = 'none';

const viewReportBtn = document.getElementById('viewReportBtn');
viewReportBtn.style.display = 'none';


microphoneBtnActive.addEventListener('click', function (e) {
    e.preventDefault();
    toggleMic();
});

microphoneBtnPassive.addEventListener('click', function (e) {
    e.preventDefault();
    toggleMic();
});

startBtn.addEventListener('click', function (e) {
    e.preventDefault();
    toggleInteraction();
});

// Show a modal to check if user did indeed want to end the roleplay
const endInteractionModalInstance = new bootstrap.Modal(document.getElementById('modalEndRoleplay'), {
    keyboard: false // Optional: set to false if you want to force the user to interact with the modal's buttons
});

stopBtn.addEventListener('click', function (e) {
    e.preventDefault();
    endInteractionModalInstance.show(); // Show the confirmation modal when the stop button is clicked
});

document.querySelector('#modalEndRoleplay .btn-primary').addEventListener('click', function () {
    // Here you can call the stopInteraction function or any logic you want to execute when user confirms the action
    stopInteraction();
    endInteractionModalInstance.hide(); // Optionally, hide the modal after the action is confirmed
});


// Azure Speech SDK Management
let interactionStarted = false;
let speechSdkLodingBlocked = false;
let recognizer = "";

function checkSDKLoaded() {
    if (window.SpeechSDK) {
        // SDK is loaded
        // Create SpeechConfig and AudioConfig
        const speechConfig = SpeechSDK.SpeechConfig.fromSubscription("7c6125baeb634f6ab5f259e33444d21e", "eastus");
        const audioConfig = SpeechSDK.AudioConfig.fromDefaultMicrophoneInput();

        // Create speech recognizer
        recognizer = new SpeechSDK.SpeechRecognizer(speechConfig, audioConfig);
    } else {
        setTimeout(checkSDKLoaded, 50);
    }
}

$.getScript("../../js/microsoft.cognitiveservices.speech.sdk.bundle.js")
    .done(function (script, textStatus) {
        console.log("Speech SDK fetched successfully. Loading it now...");
        checkSDKLoaded();
    })
    .fail(function (jqxhr, settings, exception) {
        // Script loading or execution failed
        speechSdkLodingBlocked = true;
        console.error("Speech SDK loading failed:", exception);
        showLoadingErrorModal();
    });

function stopInteraction() {
    startBtn.style.display = 'none';
    stopBtn.style.display = 'none';
    microphoneBtnActive.style.display = 'none';
    microphoneBtnPassive.style.display = 'none';

    showReportGenerationModal();

    const urlParams = `moduleAssignmentId=${moduleAssignmentId}`;

    fetch('/Interaction/WrapupInteraction?' + urlParams, {
        method: 'GET',
        headers: {
            'Content-Type': 'text/plain'
        }
    })
        .then(response => response.text()) // Extract the text from the response object
        .then(reportId => {
            if (reportId != "") {
                viewReportBtn.style.display = 'block';
                hideReportGenerationModal();
                viewReportBtn.addEventListener('click', function (e) {
                    e.preventDefault();
                    window.location.href = '/Common/ReportDetails/' + reportId;
                });
            }
        });

    interactionStarted = false;
}

function showReportGenerationModal() {
    let modal = new bootstrap.Modal(document.getElementById('reportGenerationModal'), {
        backdrop: 'static',
        keyboard: false
    });
    modal.show();

    // Initialize progress bar to fill over 75 seconds
    let progress = 0;
    const progressBar = document.getElementById('reportProgressBar');
    const interval = setInterval(() => {
        progress += 1;
        progressBar.style.width = `${progress}%`;
        progressBar.setAttribute('aria-valuenow', progress);

        if (progress >= 100) {
            clearInterval(interval);
        }
    }, 750);
}

function hideReportGenerationModal() {
    let modalElement = document.getElementById('reportGenerationModal');
    let modalInstance = bootstrap.Modal.getInstance(modalElement); // Retrieve the modal instance
    if (modalInstance) {
        modalInstance.hide();
    }
}


function toggleMic() {
    // If a user has already pressed the mic button, no need to show them the guide
    showMicButtonGuide = false;

    if (interactionStarted == false)
        return;

    if (listening == false) {
        // Listen for results

        // Begin recursive loop to keep recognizing 
        interimTranscript = "";
        finalTranscript = "";
        startContinuousRecognition(toggleMicUI);
    } else {
        // Stop recognition

        if (finalTranscript + interimTranscript == '' || finalTranscript + interimTranscript == null) {
            // user pressed the mic button accidentally or too soon
            // return, so they can actually speak something and hit
            // the mic button
            return;
        }

        recognizer.stopContinuousRecognitionAsync();

        toggleMicUI();

        setTimeout(processUserResponse, 200);
    }
}

function toggleMicUI() {
    if (listening == false) {
        listening = true;

        microphoneBtnActive.style.display = 'block';
        microphoneBtnPassive.style.display = 'none';

        SetValBackground("listening");
    }
    else {
        listening = false;

        microphoneBtnActive.style.display = 'none';
        microphoneBtnPassive.style.display = 'block';

        SetValBackground("thinking");
    }
}

let shouldProcessUserResponse = false;
let listening = false;

function startContinuousRecognition(onFirstSuccessUICallback) {
    recognizer.startContinuousRecognitionAsync();

    recognizer.recognizing = (s, e) => {
        // This event is fired while recognizing (interim results)
        // console.log(`RECOGNIZING: Text=${e.result.text}`);
        interimTranscript = e.result.text;
    };

    recognizer.recognized = (s, e) => {
        // This event is fired when a final result is recognized
        if (e.result.reason === SpeechSDK.ResultReason.RecognizedSpeech) {
            console.log(`RECOGNIZED: Text=${e.result.text}`);
            interimTranscript = '';
            finalTranscript += e.result.text;
        } else if (e.result.reason === SpeechSDK.ResultReason.NoMatch) {
            console.log("No speech recognized.");
        }
    };

    recognizer.canceled = (s, e) => {
        console.log(`CANCELED: Reason=${e.reason}`);
        if (e.reason === SpeechSDK.CancellationReason.Error) {
            console.error(`CANCELED: ErrorCode=${e.errorCode} ErrorDetails=${e.errorDetails}`);
        }

        recognizer.stopContinuousRecognitionAsync();
        startContinuousRecognition(null);
    };

    recognizer.sessionStopped = (s, e) => {
        console.log("Session stopped.");
        recognizer.stopContinuousRecognitionAsync();
    };

    // Update UI if this is the first in recursion
    if (onFirstSuccessUICallback != null) {
        onFirstSuccessUICallback();
    }
}

function toggleInteraction() {
    if (interactionStarted) {
        microphoneBtnActive.style.display = 'none';
        microphoneBtnPassive.style.display = 'none';
        stopInteraction();
    }
    else {
        // we're prefetching. so don't need to do it here
        // processUserResponse();
        // but we need to set the background to thinking
        SetValBackground("thinking");

        setTimeout(() => {
            // Delay execution within setTimeout
            interactionStarted = true;
            startBtn.style.display = 'none';
            stopBtn.style.display = 'block';
        }, 500); // Delay for 100 ms
    }
}


function SetValBackground(mode) {
    console.log("Setting Val's background to " + mode);

    if (mode == "speaking") {
        valSpeakingBackground.style.display = 'block';
        valListeningBackground.style.display = 'none';
        valThinkingBackground.style.display = 'none';
    }

    if (mode == "listening") {
        valSpeakingBackground.style.display = 'none';

        // Currently we're using the speaking lottie for the listening html element.
        // So just blank everything. Once we have a separate lottie, 
        valListeningBackground.style.display = 'none';
        valThinkingBackground.style.display = 'none';

        if (showMicButtonGuide) {
            ShowMicButtonGuide();
            showMicButtonGuide = false;
        }
    }

    if (mode == "thinking") {
        valSpeakingBackground.style.display = 'none';
        valListeningBackground.style.display = 'none';
        valThinkingBackground.style.display = 'block';
    }
}

// Utility function that returns a promise which resolves when interactionStarted becomes true
async function waitForInteractionStart() {
    return new Promise(resolve => {
        const checkInteractionStarted = setInterval(() => {
            if (interactionStarted) {
                clearInterval(checkInteractionStarted);
                resolve();
            }
        }, 100); // Check every 100 milliseconds
    });
}


// Add global error handler
window.onerror = function (error) {

    // Log error 
    console.error(error);

    showUnexpectedErrorModal();

    // Prevent default handler
    return true;
};


// Listen for the beforeunload event on the window
window.addEventListener('beforeunload', function (e) {
    if (interactionStarted == false) {
        return;
    }

    // Call stopInteraction() when the user tries to leave the page
    stopInteraction();

    // Optionally, you can display a confirmation dialog to the user
    // Note: Some browsers might ignore this message and display their own
    e.preventDefault(); // Prevent the default unload behavior
    e.returnValue = ''; // Chrome requires returnValue to be set
});



// Feedback

function toggleChat() {
    var chatContent = document.getElementById("chatContent");
    var chatOverlay = document.getElementById("chatOverlay");
    var chatPopup = document.getElementById("chatFeedbackPopup");
    var minimizeButton = document.getElementById("minimizeButton");
    var reactionButtons = document.getElementById("reactionButtons");

    // Toggle visibility of chat content and overlay
    if (chatContent.classList.contains("d-none")) {
        chatContent.classList.remove("d-none");
        chatOverlay.style.display = "block"; // Make overlay visible
        chatPopup.classList.add("chat-expanded");
        minimizeButton.classList.remove("d-none");
        reactionButtons.classList.add("d-none");
        document.getElementById("feedbackText").focus();
    } else {
        chatContent.classList.add("d-none");
        chatOverlay.style.display = "none"; // Hide overlay
        chatPopup.classList.remove("chat-expanded");
        minimizeButton.classList.add("d-none");
        reactionButtons.classList.remove("d-none");
    }
}

let valSpeakingTurnIndex = -1;

function sendReaction(event, reactionType, button) {
    event.preventDefault();

    // Apply temporary style to indicate the button was clicked
    // Determine and apply the class based on the reaction type
    const reactionClass = reactionType === 'up' ? 'reaction-up-clicked' : 'reaction-down-clicked';
    button.classList.add(reactionClass);

    // Example payload. Adjust according to your backend requirements.
    const payload = {
        moduleAssignmentId: moduleAssignmentId,
        index: valSpeakingTurnIndex,
        reaction: reactionType,
        // Include other necessary data, like user ID or feedback ID
    };

    fetch('/Common/InteractionReactions?handler=Reaction', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: JSON.stringify(payload),
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log('Reaction sent successfully:', data);
            // Remove temporary style after a short delay
            setTimeout(() => {
                button.classList.remove(reactionClass);
            }, 1000); // Adjust time as needed
        })
        .catch(error => console.error('Error sending reaction:', error));
}

function submitFeedback() {
    var feedbackText = document.getElementById("feedbackText").value;
    // Assuming the feedback endpoint is different from the reaction endpoint
    const payload = {
        moduleAssignmentId: moduleAssignmentId,
        index: valSpeakingTurnIndex,
        comment: feedbackText,
        // Include other necessary data
    };

    fetch('/Common/InteractionReactions?handler=Comment', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: JSON.stringify(payload),
    })
        .then(response => response.json())
        .then(data => {
            console.log('Feedback submitted successfully:', data);
            document.getElementById("feedbackText").value = ''; // Clear the textarea
            // Close the chat or handle UI update
        })
        .catch(error => console.error('Error submitting feedback:', error));

    toggleChat();
}

