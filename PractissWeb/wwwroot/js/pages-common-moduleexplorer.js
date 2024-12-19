function toggleBookmark(element, userId, moduleId, isBookmarked) {
    const handlerName = isBookmarked ? 'RemoveBookmark' : 'AddBookmark';
    const url = `/Common/ModuleExplorer?handler=${handlerName}`;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
        },
        body: JSON.stringify({ userId: userId, moduleId: moduleId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Toggle icon class based on current state
                if (isBookmarked) {
                    element.classList.remove('bxs-heart');
                    element.classList.add('bx-heart');
                    element.setAttribute('onclick', `toggleBookmark(this, '${userId}', '${moduleId}', false)`);
                } else {
                    element.classList.remove('bx-heart');
                    element.classList.add('bxs-heart');
                    element.setAttribute('onclick', `toggleBookmark(this, '${userId}', '${moduleId}', true)`);
                }
            } else {
                console.error('Failed to toggle bookmark');
            }
        })
        .catch(error =>
            console.error('Error:', error));
}

function startModule(mode, moduleId) {
    window.location.href = '/Common/' + mode + '/' + moduleId;
}
