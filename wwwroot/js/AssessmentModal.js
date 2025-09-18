function viewDetails(assessmentId) {
    fetch(`/Quiz/GetAssessmentDetails?id=${assessmentId}`)
        .then(response => {
            if (!response.ok) throw new Error("Failed to load assessment details");
            return response.text();
        })
        .then(html => {
            const modalContainer = document.getElementById("assessmentModalContainer");
            modalContainer.innerHTML = html;

            openAssessmentModal();
        })
        .catch(error => {
            console.error("Error loading assessment details:", error);
            alert("Could not load assessment details.");
        });
}

function openAssessmentModal() {
    const modal = document.querySelector(".assessment-modal");
    if (modal) {
        modal.style.display = "block";
        document.body.style.overflow = "hidden";
    }
}

function closeAssessmentModal() {
    const modal = document.querySelector(".assessment-modal");
    if (modal) {
        modal.style.display = "none";
        document.body.style.overflow = "auto";
    }
}

document.addEventListener("click", function (e) {
    if (e.target.classList.contains("modal-overlay")) {
        closeAssessmentModal();
    }
});

document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
        closeAssessmentModal();
    }
});
