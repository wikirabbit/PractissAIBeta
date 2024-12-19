document.addEventListener('DOMContentLoaded', addReactionButtons);
window.currentlyHighlighted = null;

function addReactionButtons() {
    const cardBodies = document.querySelectorAll('.card-body');
    cardBodies.forEach((cardBody, cardBodyIndex) => {
        if (cardBodyIndex < reportCardBodyIndex) {
            // Ratings & Additional Questions Cards. Reaction buttons mess up the charts. Don't add them.
            return;
        }

        cardBody.querySelectorAll('ul > li, ol > li').forEach((li, index) => {
            // Adjusted to work with provided JSON structure
            const reactionData = getReactionFromReportData(index, cardBodyIndex);

            const thumbsUpButton = createReactionButton(reactionData.ThumbsUp, 'thumbsUp', index, false, '', cardBodyIndex);
            const thumbsDownButton = createReactionButton(reactionData.ThumbsDown, 'thumbsDown', index, false, '', cardBodyIndex);
            const commentButton = createReactionButton(reactionData.Comments !== '', 'comment', index, true, reactionData.Comments, cardBodyIndex);

            const nestedUl = li.querySelector(':scope > ul, :scope > ol');
            if (nestedUl) {
                li.insertBefore(thumbsUpButton, nestedUl);
                li.insertBefore(thumbsDownButton, nestedUl);
                li.insertBefore(commentButton, nestedUl);
            } else {
                li.appendChild(thumbsUpButton);
                li.appendChild(thumbsDownButton);
                li.appendChild(commentButton);
            }

            if (reactionData.Comments != null && reactionData.Comments != '') {
                listItem = cardBody.querySelectorAll('ul > li, ol > li')[index];
                displayComment(listItem, reactionData.Comments); // Call displayComment directly
            }

        });
    });
}

function getReactionFromReportData(feedbackIndex, cardBodyIndex) {
    // Start with the assumption of no reaction
    let compiledReaction = { ThumbsUp: false, ThumbsDown: false, Comments: [] };

    if (preloadedFeedback.length > 0) {
        // Process the first feedback entry for thumbs up/down if it exists
        const firstFeedbackEntry = preloadedFeedback[0];
        if (firstFeedbackEntry) {
            const firstReactionsList = cardBodyIndex === reportCardBodyIndex ? firstFeedbackEntry.ReportReactions : firstFeedbackEntry.InteractionReactions;
            const firstReaction = firstReactionsList ? firstReactionsList.find(r => r.Index === feedbackIndex) : null;
            if (firstReaction) {
                compiledReaction.ThumbsUp = firstReaction.ThumbsUp;
                compiledReaction.ThumbsDown = firstReaction.ThumbsDown;
            }
        }
    }

    // Compile comments from all feedback providers
    preloadedFeedback.forEach(feedbackEntry => {
        const reactionsList = feedbackEntry ? (cardBodyIndex === reportCardBodyIndex ? feedbackEntry.ReportReactions : feedbackEntry.InteractionReactions) : null;
        if (reactionsList) {
            const reaction = reactionsList.find(r => r.Index === feedbackIndex);
            if (reaction && reaction.Comments) {
                const providerName = `[${feedbackEntry.FeedbackProvider.FirstName} ${feedbackEntry.FeedbackProvider.LastName}]`;
                compiledReaction.Comments.push(`${providerName} ${reaction.Comments}`);
            }
        }
    });

    // Join all comments into a single string to be displayed
    compiledReaction.Comments = compiledReaction.Comments.join("\n");

    return compiledReaction;
}



function createReactionButton(isActive, type, index, isComment = false, commentText = '', cardBodyIndex) {
    if (type === 'thumbsDown') type = 'thumbs-down';
    if (type === 'thumbsUp') type = 'thumbs-up';

    const button = document.createElement('button');
    button.className = 'reaction-button';
    button.innerHTML = `<i class="${isActive ? 'fa-solid' : 'fa-regular'} fa-${type}"></i>`;
    button.dataset.index = index;
    button.dataset.type = type;
    button.dataset.solid = isActive.toString();
    button.dataset.cardBodyIndex = cardBodyIndex; // Store card body index in dataset

    // Adjust the index for reactions from the second card body
    let listItemIndex = button.dataset.index;
    if (cardBodyIndex == reportCardBodyIndex + 1) {
        listItemIndex = parseInt(button.dataset.index, 10) + document.querySelectorAll('.card-body')[0].querySelectorAll('ul > li, ol > li').length;
    }

    if (isComment) {
        button.dataset.comment = commentText;
        button.onclick = function () {
            openCommentPopup(index, this.dataset.comment, this.dataset.cardBodyIndex);
        };

        if (commentText) {
            // Find the corresponding list item
            const cardBody = document.querySelectorAll('.card-body')[cardBodyIndex];
            const lis = cardBody.querySelectorAll('ul > li, ol > li');
            if (listItemIndex >= 0 && listItemIndex < lis.length) {
                const listItem = lis[listItemIndex];
            }
        }
    } else {
        button.onclick = function () {
            toggleReactionState(this);
        };
    }

    return button;
}

function getReaction(feedbackIndex, callback, cardBodyIndex) {
    fetch(`/Common/ReportReactions?handler=Reaction&reportId=${document.getElementById("reportId").value}&userId=${document.getElementById("userId").value}&cardBodyIndex=${cardBodyIndex - reportCardBodyIndex}&feedbackIndex=${feedbackIndex}`, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
        },
    })
        .then(response => response.json())
        .then(data => callback(data, feedbackIndex))
        .catch(error => console.error('Error fetching reaction data:', error));
}

function toggleReactionState(button) {
    const isCurrentlySolid = button.dataset.solid === "true";
    const listItem = button.closest('li'); // Get the closest parent list item of the button
    const cardBodyIndex = button.dataset.cardBodyIndex;

    // Find the opposite button within the same list item
    const oppositeType = button.dataset.type === 'thumbs-up' ? 'thumbs-down' : 'thumbs-up';
    const oppositeButton = listItem.querySelector(`button[data-type="${oppositeType}"][data-card-body-index="${cardBodyIndex}"]`);

    // Toggle the current button's state
    button.dataset.solid = !isCurrentlySolid;
    const icon = button.querySelector('i');
    icon.classList.toggle('fa-solid', !isCurrentlySolid);
    icon.classList.toggle('fa-regular', isCurrentlySolid);

    // Adjust the index for reactions from the second card body
    const listItemIndex = cardBodyIndex === (reportCardBodyIndex + 1) ? parseInt(button.dataset.index, 10) - document.querySelectorAll('.card-body')[0].querySelectorAll('ul > li, ol > li').length : parseInt(button.dataset.index, 10);

    // Send reaction for the clicked button
    sendReaction({
        index: listItemIndex,
        type: button.dataset.type.replace('-', ''),
        isSolid: !isCurrentlySolid, // Reflects the new state
        cardBodyIndex: cardBodyIndex
    });

    // If the clicked button is being activated, deactivate the opposite button and send its reaction
    if (!isCurrentlySolid && oppositeButton && oppositeButton.dataset.solid === "true") {
        oppositeButton.dataset.solid = "false";
        const oppositeIcon = oppositeButton.querySelector('i');
        oppositeIcon.classList.replace('fa-solid', 'fa-regular');
    }
}

function sendReaction(details) {
    const payload = {
        ReportId: document.getElementById("reportId").value,
        UserId: document.getElementById("userId").value,
        CardBodyIndex: details.cardBodyIndex - reportCardBodyIndex,
        FeedbackIndex: details.index,
        Type: details.type,
        IsSolid: details.isSolid,
        Comment: details.comment || ''
    };

    fetch(`/Common/ReportReactions?handler=Reaction`, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: JSON.stringify(payload),
    })
        .then(response => response.json())
        .then(data => console.log('Reaction updated successfully:', data))
        .catch(error => console.error('Error sending reaction:', error));
}

function regenerateReport(reportId) {
    // Assuming your button has an id of 'regenerateReportButton'
    const button = document.getElementById('regenerateReportButton');
    button.disabled = true; // Disable the button

    const urlParams = `reportId=${reportId}`;

    fetch('/Interaction/RegenerateReport?' + urlParams, {
        method: 'GET',
        headers: {
            'Content-Type': 'text/plain'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            // Wait for 2 seconds before reloading
            setTimeout(() => {
                window.location.reload();
            }, 2000); // 2000 milliseconds = 2 seconds
        })
        .catch(error => {
            console.error('Error:', error);
            button.disabled = false; // Re-enable the button if there's an error
        });
}

function openCommentPopup(index, commentText, cardBodyIndex) {
    const chatPopup = document.getElementById("chatFeedbackPopup");
    document.getElementById("feedbackIndex").value = index;
    document.getElementById("feedbackText").value = '';
    document.getElementById("cardBodyIndex").value = cardBodyIndex;

    toggleChat(); // Show the chat popup

    // Correctly identify the list item using both index and cardBodyIndex
    const cardBodies = document.querySelectorAll('.card-body');
    if (cardBodyIndex >= 0 && cardBodyIndex < cardBodies.length) {
        const lis = cardBodies[cardBodyIndex].querySelectorAll('ul > li, ol > li');
        const listItem = lis[index];
        listItem.classList.add('highlighted'); // Highlight the <li>

        // Adjusting popup position based on listItem position
        adjustPopupPosition(listItem, chatPopup);
    }
}

function adjustPopupPosition(listItem, chatPopup) {
    // Get bounding rectangle of the <li>
    const rect = listItem.getBoundingClientRect();

    // Increase this value for more space between the <li> and the popup
    const verticalBuffer = 30; // Adjusted from 5 to 15 for more vertical space

    // Calculate top position to place the popup just below the <li> with added buffer
    let topPosition = rect.bottom + verticalBuffer;
    let bottomPosition = rect.top - verticalBuffer;

    // Ensure the popup does not float off the bottom of the screen
    const maxTopPosition = window.innerHeight - chatPopup.offsetHeight;
    if (topPosition > maxTopPosition) {
        topPosition = bottomPosition - chatPopup.offsetHeight;
    }

    // Calculate left position to horizontally center the popup
    const leftPosition = Math.max((window.innerWidth / 2), 0);

    // Apply calculated positions
    chatPopup.style.top = `${topPosition}px`;
    chatPopup.style.left = `${leftPosition}px`;

    // Store the highlighted item for later removal of the class
    window.currentlyHighlighted = listItem;
}

function toggleChat() {
    var chatPopup = document.getElementById("chatFeedbackPopup");
    var chatOverlay = document.getElementById("chatOverlay");

    var isVisible = chatPopup.style.display === "block";
    chatPopup.style.display = isVisible ? "none" : "block";
    chatOverlay.style.display = isVisible ? "none" : "block";

    // If closing the popup, remove the highlighted class from the previously highlighted <li>
    if (isVisible && window.currentlyHighlighted) {
        window.currentlyHighlighted.classList.remove('highlighted');
        window.currentlyHighlighted = null; // Reset the reference
    }
}


function submitFeedback() {
    var feedbackText = document.getElementById("feedbackText").value;
    var feedbackIndex = document.getElementById("feedbackIndex").value;
    var cardBodyIndex = document.getElementById("cardBodyIndex").value;

    if (feedbackText && feedbackIndex !== undefined) {
        // Existing functionality to send reaction
        // After sending, append the comment
        const cardBody = document.querySelectorAll('.card-body')[cardBodyIndex];
        const listItem = cardBody.querySelectorAll('ul > li, ol > li')[feedbackIndex];
        displayComment(listItem, feedbackText); // Call a new function to display the comment

        sendReaction({ index: feedbackIndex, type: 'comment', cardBodyIndex: cardBodyIndex, comment: feedbackText, isSolid: false });
        document.getElementById("feedbackText").value = ""; // Clear text area
        toggleChat(); // Hide the chat popup
    }
}

function displayComment(listItem, commentText) {
    const comments = commentText.split("\n");
    comments.forEach(comment => {
        if (comment) {
            const commentElement = document.createElement('p');
            commentElement.textContent = comment;
            commentElement.style.fontStyle = 'italic';
            commentElement.style.color = 'blue'; // Displaying in blue as per previous instructions
            commentElement.className = 'inline-comment';
            listItem.appendChild(commentElement);
        }
    });
}


