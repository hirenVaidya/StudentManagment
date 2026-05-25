// Interactive behaviors for custom modals and sleek toast notifications

document.addEventListener("DOMContentLoaded", () => {
    // === SLEEK TOAST NOTIFICATIONS ===
    const toasts = document.querySelectorAll(".custom-toast");
    
    toasts.forEach((toast) => {
        // Automatically hide and remove toasts after 3 seconds
        const dismissTimeout = setTimeout(() => {
            dismissToast(toast);
        }, 3000);

        // Store timeout ID on the element in case it's manually dismissed earlier
        toast.dataset.dismissTimeout = dismissTimeout;
    });

    function dismissToast(toast) {
        if (!toast) return;
        
        // Clear any pending auto-dismiss timer
        if (toast.dataset.dismissTimeout) {
            clearTimeout(toast.dataset.dismissTimeout);
        }

        // Add class to trigger slideOut animation
        toast.classList.add("hide-toast");
        
        // Wait for the slide-out CSS transition to finish before removing from DOM
        toast.addEventListener("animationend", (e) => {
            if (e.animationName === "slideOutRight") {
                toast.remove();
            }
        });

        // Fallback removal in case animation events aren't caught
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 400);
    }

    // === CUSTOM CONFIRM DELETION MODAL ===
    const deleteModal = document.getElementById("deleteConfirmModal");
    const cancelDeleteBtn = document.getElementById("btnCancelDelete");
    const confirmDeleteBtn = document.getElementById("btnConfirmDelete");
    let activeDeleteForm = null;

    // Attach click events to all delete student buttons
    document.addEventListener("click", (e) => {
        const deleteBtn = e.target.closest(".btn-delete-student");
        if (deleteBtn) {
            e.preventDefault();
            
            // Find parent form
            activeDeleteForm = deleteBtn.closest(".form-delete-student");
            
            if (activeDeleteForm && deleteModal) {
                openModal(deleteModal);
            }
        }
    });

    function openModal(modal) {
        // Remove d-none to render in DOM
        modal.classList.remove("d-none");
        
        // Force reflow to allow transition to trigger
        modal.offsetHeight;
        
        // Add show-modal class to animate in
        modal.classList.add("show-modal");
    }

    function closeModal(modal) {
        if (!modal) return;
        
        // Remove animation class
        modal.classList.remove("show-modal");
        
        // Wait for transform/opacity transition to finish, then add d-none
        setTimeout(() => {
            modal.classList.add("d-none");
            activeDeleteForm = null;
        }, 300);
    }

    // Cancel deletion buttons
    if (cancelDeleteBtn && deleteModal) {
        cancelDeleteBtn.addEventListener("click", () => {
            closeModal(deleteModal);
        });
    }

    // Dismiss modal if clicking outside the card on the backdrop
    if (deleteModal) {
        deleteModal.addEventListener("click", (e) => {
            if (e.target === deleteModal) {
                closeModal(deleteModal);
            }
        });
    }

    // Confirm deletion
    if (confirmDeleteBtn && deleteModal) {
        confirmDeleteBtn.addEventListener("click", () => {
            if (activeDeleteForm) {
                activeDeleteForm.submit();
            }
            closeModal(deleteModal);
        });
    }

    // === COUNTRY FLAG SELECTOR & PHONE VALIDATION ===
    const phoneVisualInput = document.getElementById("phone-visual");
    const phoneHiddenInput = document.getElementById("phone-hidden");
    const countryDropdownList = document.getElementById("country-dropdown-list");
    const selectedFlagSpan = document.getElementById("selected-flag");
    const selectedCodeSpan = document.getElementById("selected-code");
    const phoneErrorMsg = document.getElementById("phone-error-msg");

    if (phoneVisualInput && phoneHiddenInput && countryDropdownList) {
        let currentCountryCode = "91"; // Default to India (91)

        // Function to update hidden input
        function updateHiddenPhone() {
            const digits = phoneVisualInput.value.replace(/\D/g, ""); // Keep only digits
            if (digits) {
                phoneHiddenInput.value = currentCountryCode + digits;
            } else {
                phoneHiddenInput.value = "";
            }
        }

        // Dropdown selection click handler
        countryDropdownList.addEventListener("click", (e) => {
            const item = e.target.closest(".dropdown-item");
            if (item) {
                e.preventDefault();
                const flag = item.dataset.flag;
                const code = item.dataset.code;

                // Update UI button
                if (selectedFlagSpan) selectedFlagSpan.textContent = flag;
                if (selectedCodeSpan) selectedCodeSpan.textContent = code;

                currentCountryCode = code;
                updateHiddenPhone();
                validatePhoneInput();
            }
        });

        // Numeric typing restriction: block non-numeric characters
        phoneVisualInput.addEventListener("keypress", (e) => {
            // Permit backspace, delete, tab, arrows
            if (e.key === "Backspace" || e.key === "Delete" || e.key === "Tab" || 
                e.key === "ArrowLeft" || e.key === "ArrowRight" || e.key === "Enter") {
                return;
            }
            // Block non-digits
            if (!/[0-9]/.test(e.key)) {
                e.preventDefault();
            }
        });

        // Filter and update on type / paste
        phoneVisualInput.addEventListener("input", () => {
            // Strip non-digits
            const cleaned = phoneVisualInput.value.replace(/\D/g, "");
            if (phoneVisualInput.value !== cleaned) {
                phoneVisualInput.value = cleaned;
            }

            updateHiddenPhone();
            validatePhoneInput();
        });

        // Phone validation helper
        function validatePhoneInput() {
            const val = phoneVisualInput.value;
            if (val.length === 0) {
                showError("The Phone field is required.");
                return false;
            } else if (val.length !== 10) {
                showError("Please enter exactly 10 digits.");
                return false;
            } else {
                clearError();
                return true;
            }
        }

        function showError(msg) {
            if (phoneErrorMsg) {
                phoneErrorMsg.textContent = msg;
                phoneErrorMsg.style.display = "block";
            }
            phoneVisualInput.classList.add("is-invalid");
        }

        function clearError() {
            if (phoneErrorMsg) {
                phoneErrorMsg.textContent = "";
                phoneErrorMsg.style.display = "none";
            }
            phoneVisualInput.classList.remove("is-invalid");
        }

        // Form submission hook
        const studentForm = phoneVisualInput.closest("form");
        if (studentForm) {
            studentForm.addEventListener("submit", (e) => {
                const isValid = validatePhoneInput();
                if (!isValid) {
                    e.preventDefault();
                    // Scroll to phone field
                    phoneVisualInput.scrollIntoView({ behavior: "smooth", block: "center" });
                    phoneVisualInput.focus();
                }
            });
        }

        // Initialize / Parse phone number on Page Load (Edit Mode)
        function initPhoneField() {
            const rawPhone = phoneHiddenInput.value.trim();
            if (rawPhone && rawPhone.length > 10) {
                // Extract last 10 digits
                const digits = rawPhone.slice(-10);
                const code = rawPhone.slice(0, -10);

                // Find dropdown item matching this code
                const items = countryDropdownList.querySelectorAll(".dropdown-item");
                let foundItem = null;
                items.forEach((item) => {
                    if (item.dataset.code === code) {
                        foundItem = item;
                    }
                });

                if (foundItem) {
                    // Update UI flags and codes
                    const flag = foundItem.dataset.flag;
                    if (selectedFlagSpan) selectedFlagSpan.textContent = flag;
                    if (selectedCodeSpan) selectedCodeSpan.textContent = code;
                    currentCountryCode = code;
                } else {
                    // Fallback country code if not in our list
                    if (selectedFlagSpan) selectedFlagSpan.textContent = "🌐";
                    if (selectedCodeSpan) selectedCodeSpan.textContent = code;
                    currentCountryCode = code;
                }

                phoneVisualInput.value = digits;
            } else if (rawPhone) {
                // If it is 10 digits or less, default to India (91) but put it in visual input
                phoneVisualInput.value = rawPhone;
                updateHiddenPhone();
            } else {
                // New form - set default hidden value for India (91) as soon as user types
                updateHiddenPhone();
            }
        }

        // Run initialization
        initPhoneField();
    }
});
