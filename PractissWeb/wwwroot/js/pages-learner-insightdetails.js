document.addEventListener('DOMContentLoaded', (event) => {
    let elem = document.getElementById('quotedFeedback');
  if(elem) {
    let text = elem.innerHTML;
  // Attempt to match quoted text more accurately, avoiding contractions
  // This is a heuristic and might not be perfect for all cases.
  let updatedText = text.replace(/'([^']{3,})'/g, function(match, p1) {
            // If the match is not a word (i.e., it's likely a quote), italicize it.
            // This checks if the match is surrounded by spaces or is at the start/end of the string, which might indicate it's quoted text.
            if (/\s/.test(match[0]) || match.indexOf("'") === 0 || match.lastIndexOf("'") === match.length - 1) {
                return `<em>${p1}</em>`;
            }
  return match; // Return the original match if it doesn't seem like quoted text.
        });
  elem.innerHTML = updatedText;
    }
});

$(document).ready(function () {
    $(".btn-label-primary").click(function () {
        // Disable the button immediately after click to prevent multiple submissions
        $(this).prop('disabled', true);

        // Use the data-target attribute to dynamically select the roleplay description
        var targetSelector = $(this).data("target");
        var roleplayDetailsText = $(targetSelector).text();

        fetch('?handler=StartRoleplay', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
            },
            body: JSON.stringify({ roleplayDetails: roleplayDetailsText }),
            credentials: 'include'
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.isValid) {
                    window.location.href = "/Common/Interaction/" + data.moduleAssignmentId;
                } else {
                    // Optionally re-enable the button if the operation was not successful
                    $(this).prop('disabled', false);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                // Re-enable the button in case of error so the user can try again
                $(this).prop('disabled', false);
            });
    });
});
