  document.addEventListener('DOMContentLoaded', function() {
            const searchInput = document.getElementById('searchInput');
            const suggestionsGrid = document.getElementById('suggestionsGrid');
            const noResultsMessage = document.getElementById('noResultsMessage');
            const loadingIndicator = document.getElementById('loadingIndicator');
            const suggestionCards = document.querySelectorAll('.suggestion-card');

            let searchTimeout;

            searchInput.addEventListener('input', function() {
                clearTimeout(searchTimeout);
                
                searchTimeout = setTimeout(() => {
                    filterSuggestions();
                }, 300);
            });

            function filterSuggestions() {
                const searchTerm = searchInput.value.toLowerCase().trim();
                let visibleCount = 0;

                loadingIndicator.classList.add('show');
                
                setTimeout(() => {
                    suggestionCards.forEach(card => {
                        const skillName = card.getAttribute('data-skill') || '';
                        const suggestionText = card.getAttribute('data-text') || '';
                        
                        if (searchTerm === '' || 
                            skillName.includes(searchTerm) || 
                            suggestionText.includes(searchTerm)) {
                            card.style.display = 'block';
                            visibleCount++;
                        } else {
                            card.style.display = 'none';
                        }
                    });

                    loadingIndicator.classList.remove('show');

                    if (visibleCount === 0 && searchTerm !== '') {
                        noResultsMessage.style.display = 'block';
                        suggestionsGrid.style.display = 'none';
                    } else {
                        noResultsMessage.style.display = 'none';
                        suggestionsGrid.style.display = 'grid';
                    }
                }, 200);
            }

            searchInput.addEventListener('focus', function() {
                this.scrollIntoView({ behavior: 'smooth', block: 'center' });
            });
        });