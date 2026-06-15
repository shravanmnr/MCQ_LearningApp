/**
 * quiz.js — Client-side logic for the ASP.NET MCQ Trainer
 *
 * Responsibilities:
 *  1. Loading overlay (shown during server round-trip to generate questions)
 *  2. Choice card selection UI
 *  3. AJAX form submission → JSON result from QuizController.Submit
 *  4. Visual feedback: green (correct) / red (wrong) with explanation reveal
 */

// ─── Loading overlay ─────────────────────────────────────────────────────────

function showLoadingOverlay() {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) overlay.classList.remove('d-none');
}

document.addEventListener('DOMContentLoaded', () => {
    // Attach loading overlay to any element with class `show-loading`
    document.querySelectorAll('.show-loading').forEach(el => {
        el.addEventListener('click', showLoadingOverlay);
    });
});

// ─── Quiz interaction ─────────────────────────────────────────────────────────

function initQuiz() {
    const form = document.getElementById('quiz-form');
    if (!form) return;

    const choiceCards    = document.querySelectorAll('.choice-card');
    const selectedInput  = document.getElementById('selected-index');
    const submitBtn      = document.getElementById('submit-btn');
    const validationHint = document.getElementById('validation-hint');
    const resultBanner   = document.getElementById('result-banner');
    const explanationPanel = document.getElementById('explanation-panel');
    const explanationText  = document.getElementById('explanation-text');
    const nextActions    = document.getElementById('next-actions');

    let selectedIndex = -1;
    let answered = false;

    // ── Choice selection ──────────────────────────────────────────────────────
    choiceCards.forEach((card, index) => {
        card.addEventListener('click', () => {
            if (answered) return;

            choiceCards.forEach(c => {
                c.classList.remove('selected');
                c.setAttribute('aria-pressed', 'false');
            });

            card.classList.add('selected');
            card.setAttribute('aria-pressed', 'true');
            selectedIndex = index;
            selectedInput.value = index;

            submitBtn.disabled = false;
            validationHint.classList.add('d-none');
        });
    });

    // ── Keyboard navigation for choice cards ─────────────────────────────────
    choiceCards.forEach((card, index) => {
        card.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                card.click();
            }
            if (e.key === 'ArrowDown' && index < choiceCards.length - 1) {
                e.preventDefault();
                choiceCards[index + 1].focus();
            }
            if (e.key === 'ArrowUp' && index > 0) {
                e.preventDefault();
                choiceCards[index - 1].focus();
            }
        });
    });

    // ── Form submission ───────────────────────────────────────────────────────
    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        if (selectedIndex === -1) {
            validationHint.classList.remove('d-none');
            return;
        }

        if (answered) return;

        setSubmitLoading(true);

        const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
        const formData = new FormData(form);

        try {
            const response = await fetch('/Quiz/Submit', {
                method: 'POST',
                body: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (!response.ok) {
                throw new Error(`Server returned ${response.status}`);
            }

            const result = await response.json();

            if (result.error) {
                showInlineError(result.error);
                setSubmitLoading(false);
                return;
            }

            answered = true;
            revealResult(result);

        } catch (err) {
            console.error('Submit error:', err);
            showInlineError('Something went wrong. Please refresh and try again.');
            setSubmitLoading(false);
        }
    });

    // ── Reveal result UI ──────────────────────────────────────────────────────
    function revealResult(result) {
        const { isCorrect, correctIndex, selectedIndex: si } = result;

        // Lock all choices
        choiceCards.forEach(c => c.classList.add('answered'));

        // Colour-code
        choiceCards.forEach((card, idx) => {
            card.classList.remove('selected');
            if (idx === correctIndex) {
                card.classList.add('correct');
            } else if (idx === si && !isCorrect) {
                card.classList.add('incorrect');
            }
        });

        // Result banner
        resultBanner.classList.remove('d-none');
        if (isCorrect) {
            resultBanner.className = 'result-banner result-correct';
            resultBanner.innerHTML = '<span class="result-icon">✓</span> Correct! Well done.';
        } else {
            resultBanner.className = 'result-banner result-incorrect';
            resultBanner.innerHTML = '<span class="result-icon">✗</span> Incorrect — see the correct answer above.';
        }

        // Explanation
        explanationText.textContent = result.explanation;
        explanationPanel.classList.remove('d-none');

        // Swap buttons
        submitBtn.classList.add('d-none');
        nextActions.classList.remove('d-none');

        // Scroll explanation into view smoothly
        setTimeout(() => {
            explanationPanel.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }, 150);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    function setSubmitLoading(loading) {
        submitBtn.disabled = loading;
        submitBtn.innerHTML = loading
            ? '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Checking…'
            : 'Submit Answer';
    }

    function showInlineError(message) {
        let alert = document.getElementById('inline-error');
        if (!alert) {
            alert = document.createElement('div');
            alert.id = 'inline-error';
            alert.className = 'alert alert-danger alert-dismissible mt-3';
            form.after(alert);
        }
        alert.innerHTML = `${message} <button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
    }
}
