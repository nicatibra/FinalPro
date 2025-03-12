const agreeCheckbox = document.getElementById("agreeToTerms");
const submitButton = document.getElementById("submitButton");

function toggleSubmitButton() {
    if (agreeCheckbox.checked) {
        submitButton.removeAttribute("disabled");
        submitButton.classList.remove("btn-secondary");
        submitButton.classList.add("btn-fill-out");
    } else {
        submitButton.setAttribute("disabled", "true");
        submitButton.classList.remove("btn-fill-out");
        submitButton.classList.add("btn-secondary");
    }
}

agreeCheckbox.addEventListener("change", toggleSubmitButton);

toggleSubmitButton();
