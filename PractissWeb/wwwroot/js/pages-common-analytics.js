document.addEventListener('DOMContentLoaded', function () {
    // Select the "Quantitative Feedback" section
    const feedbackSection = document.querySelector('#quantitativeFeedbackSection');
    const overallScoreSection = document.querySelector('#overallScoreSection');
    const scoreInfoSection = document.querySelector('#scoreInfoSection');

    // Get the height of the "Quantitative Feedback" section
    const feedbackSectionHeight = feedbackSection.offsetHeight;
    const scoreInfoSectionHeight = scoreInfoSection.offsetHeight;

    const overallScoreElement = document.querySelector('#overallScoreRadialChart');
    if (overallScoreElement) {
        const overallScore = parseFloat(overallScoreElement.getAttribute('data-overall-score')); // Ensure it's a number
        const scoreColor = overallScore >= 75 ? '#28a745' : overallScore >= 50 ? '#ffc107' : '#dc3545'; // Example color logic

        const overallScoreRadialChartConfig = {
            chart: {
                height: feedbackSectionHeight - scoreInfoSectionHeight, // Match this with the visual height of your quantitative feedback section
                type: 'radialBar',
            },
            colors: [scoreColor], // Dynamic color based on score
            series: [overallScore], // Use the overall score here
            plotOptions: {
                radialBar: {
                    offsetY: -10,
                    hollow: {
                        size: '45%'
                    },
                    track: {
                        margin: 10,
                        background: '#f0f0f0' // Example track color, adjust as needed
                    },
                    dataLabels: {
                        name: {
                            fontSize: '15px',
                            color: '#666',
                            fontFamily: 'IBM Plex Sans',
                            offsetY: 25
                        },
                        value: {
                            fontSize: '2rem',
                            fontFamily: 'Rubik',
                            fontWeight: 500,
                            color: scoreColor,
                            offsetY: -15,
                            formatter: function (val) {
                                return val; // Display the score without appending '%'
                            }
                        },
                        total: {
                            show: true,
                            fontSize: '15px',
                            fontWeight: 400,
                            fontFamily: 'IBM Plex Sans',
                            color: '#666',
                            formatter: function (w) {
                                return w.globals.seriesTotals.reduce((a, b) => {
                                    return a + b
                                }, 0) // Display total score without '%'
                            }
                        }
                    }
                }
            },
            grid: {
                padding: {
                    top: -10,
                    bottom: -10
                }
            },
            stroke: {
                lineCap: 'round',
                width: 8
            },
            labels: ['Score'],
            legend: {
                show: false // Hide legend if not needed
            }
        };

        const overallScoreRadialChart = new ApexCharts(overallScoreElement, overallScoreRadialChartConfig);
        overallScoreRadialChart.render();
    }
});