function viewReportDetails(reportId) {
  window.open('/Common/ReportDetails/' + reportId, '_blank');
}

document.addEventListener("DOMContentLoaded", function () {
  var progressBar = document.getElementById("progressBar");
  var width = 1;
  var interval = setInterval(frame, 600); // Adjust the time according to your needs

  // Parse the moduleAssignmentId from the URL
  const pathSegments = window.location.pathname.split('/');
  const moduleAssignmentIndex = pathSegments.findIndex(segment => segment === "ReportPreparation") + 1;
  const moduleAssignmentId = pathSegments[moduleAssignmentIndex];

  const urlParams = `moduleAssignmentId=${moduleAssignmentId}`;
  let url = '/Interaction/WrapupInteraction?' + urlParams;

  fetch(url, {
    method: 'GET',
    headers: {
      'Content-Type': 'text/plain'
    }
  })
    .then(response => response.text()) // Extract the text from the response object
    .then(reportId => {
      clearInterval(interval);
      if (reportId != "") {
        // Hide the progress bar or indicate completion
        progressBar.style.display = 'none';
        // Redirect to the report details page
        window.location.href = '/Common/ReportDetails/' + reportId;
      }
    });

  function frame() {
    if (width >= 100) {
      clearInterval(interval);
      // Consider keeping this line if you expect the fetch to potentially complete sooner than the progress bar.
      // width = 100;
      // progressBar.style.width = width + '%';
    } else {
      width++;
      progressBar.style.width = width + '%';
    }
  }
});
