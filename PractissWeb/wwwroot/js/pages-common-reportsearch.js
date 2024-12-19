function viewReportDetails(reportId) {
  window.open('/Common/ReportDetails/' + reportId, '_blank');
}

document.addEventListener('DOMContentLoaded', function () {
    // Search function
    function searchModules(inputId, tableId) {
        var searchText = document.getElementById(inputId).value.toLowerCase();
        document.querySelectorAll(`${tableId} tbody tr`).forEach(function (row) {
            // Concatenate the text content of all cells in the row
            var rowText = Array.from(row.querySelectorAll('td')).map(function (td) {
                return td.textContent.toLowerCase();
            }).join(' ');

            // Display the row only if the concatenated text includes the search text
            row.style.display = rowText.includes(searchText.toLowerCase()) ? '' : 'none';
        });
    }

    // Event listeners for search inputs
    document.getElementById('reportsSearchTextbox').addEventListener('keyup', function () {
        searchModules('reportsSearchTextbox', '#roleplayReportsTable');
    });
});