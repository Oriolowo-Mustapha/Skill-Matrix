/**
 * Quiz Navigation and Management System
 * Handles single-question display, navigation, progress tracking, and validation
 */

// Quiz Navigation Module
const QuizNavigation = {
    // Quiz state variables
    currentQuestion: 0,
    totalQuestions: 0,
    answeredQuestions: new Set(),

    init() {
        this.totalQuestions = document.querySelectorAll('.question-card').length;
        this.setupEventListeners();
        this.updateUI();
    },

    /**
     * Set up all event listeners
     */
    setupEventListeners() {
        // Form submission validation
        document.getElementById('quizForm').addEventListener('submit', this.handleFormSubmit.bind(this));

        // Track answered questions when radio buttons change
        document.querySelectorAll('input[type="radio"]').forEach(radio => {
            radio.addEventListener('change', this.handleAnswerChange.bind(this));
        });

        // Keyboard navigation
        document.addEventListener('keydown', this.handleKeyboardNavigation.bind(this));
    },

    /**
     * Update progress bar display
     */
    updateProgress() {
        const progress = (this.currentQuestion / (this.totalQuestions - 1)) * 100;
        document.getElementById('progressFill').style.width = progress + '%';
        document.getElementById('currentQuestionText').textContent = 
            `Question ${this.currentQuestion + 1} of ${this.totalQuestions}`;
    },

    /**
     * Update question status dots
     */
    updateQuestionStatus() {
        const statusDots = document.querySelectorAll('.status-dot');
        statusDots.forEach((dot, index) => {
            dot.classList.remove('current');
            if (index === this.currentQuestion) {
                dot.classList.add('current');
            }
            if (this.answeredQuestions.has(index)) {
                dot.classList.add('answered');
            }
        });
    },

    /**
     * Update navigation button states
     */
    updateNavigationButtons() {
        const prevBtn = document.getElementById('prevBtn');
        const nextBtn = document.getElementById('nextBtn');
        const submitBtn = document.getElementById('submitBtn');

        prevBtn.disabled = this.currentQuestion === 0;
        
        if (this.currentQuestion === this.totalQuestions - 1) {
            nextBtn.style.display = 'none';
            submitBtn.style.display = 'block';
        } else {
            nextBtn.style.display = 'block';
            submitBtn.style.display = 'none';
        }
    },

    updateUI() {
        this.updateProgress();
        this.updateQuestionStatus();
        this.updateNavigationButtons();
    },

    
    changeQuestion(direction) {
        const currentCard = document.querySelector(`.question-card[data-question="${this.currentQuestion}"]`);
        
        // Check if current question is answered before moving forward
        if (direction > 0) {
            const currentAnswers = document.querySelectorAll(`input[name="answers[${this.currentQuestion}]"]:checked`);
            if (currentAnswers.length > 0) {
                this.answeredQuestions.add(this.currentQuestion);
            }
        }

        // Hide current question with animation
        currentCard.classList.remove('active');
        
        setTimeout(() => {
            // Update current question index
            this.currentQuestion += direction;
            
            // Ensure bounds
            if (this.currentQuestion < 0) this.currentQuestion = 0;
            if (this.currentQuestion >= this.totalQuestions) this.currentQuestion = this.totalQuestions - 1;
            
            // Show new question
            const nextCard = document.querySelector(`.question-card[data-question="${this.currentQuestion}"]`);
            nextCard.classList.add('active');
            
            // Update UI
            this.updateUI();
            
            // Scroll to top of form
            document.querySelector('.quiz-form').scrollIntoView({ 
                behavior: 'smooth', 
                block: 'start' 
            });
        }, 150);
    },

 
    goToQuestion(questionIndex) {
        if (questionIndex < 0 || questionIndex >= this.totalQuestions) return;

        const currentCard = document.querySelector(`.question-card[data-question="${this.currentQuestion}"]`);
        currentCard.classList.remove('active');
        
        setTimeout(() => {
            this.currentQuestion = questionIndex;
            const targetCard = document.querySelector(`.question-card[data-question="${this.currentQuestion}"]`);
            targetCard.classList.add('active');
            
            this.updateUI();
        }, 150);
    },

    
    getUnansweredQuestions() {
        const unansweredQuestions = [];
        
        for (let i = 0; i < this.totalQuestions; i++) {
            const answers = document.querySelectorAll(`input[name="answers[${i}]"]:checked`);
            if (answers.length === 0) {
                unansweredQuestions.push(i + 1);
            }
        }
        
        return unansweredQuestions;
    },

    /**
     * Handle form submission
     * @param {Event} e - Submit event
     */
    handleFormSubmit(e) {
        const unansweredQuestions = this.getUnansweredQuestions();
        
        if (unansweredQuestions.length > 0) {
            e.preventDefault();
            alert(`Please answer all questions before submitting. Unanswered questions: ${unansweredQuestions.join(', ')}`);
            
            // Navigate to first unanswered question
            const firstUnanswered = unansweredQuestions[0] - 1;
            this.goToQuestion(firstUnanswered);
        }
    },

    /**
     * Handle answer selection changes
     * @param {Event} e - Change event
     */
    handleAnswerChange(e) {
        const questionIndex = parseInt(e.target.name.match(/\d+/)[0]);
        this.answeredQuestions.add(questionIndex);
        this.updateQuestionStatus();
    },

    handleKeyboardNavigation(e) {
        if (e.key === 'ArrowLeft' && this.currentQuestion > 0) {
            this.changeQuestion(-1);
        } else if (e.key === 'ArrowRight' && this.currentQuestion < this.totalQuestions - 1) {
            this.changeQuestion(1);
        }
    },

    getQuizStats() {
        return {
            currentQuestion: this.currentQuestion + 1,
            totalQuestions: this.totalQuestions,
            answeredQuestions: this.answeredQuestions.size,
            completionPercentage: Math.round((this.answeredQuestions.size / this.totalQuestions) * 100)
        };
    }
};

// Initialize quiz when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    QuizNavigation.init();
});

// Export for global access (if needed)
window.QuizNavigation = QuizNavigation;