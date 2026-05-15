// Simple fetch to get categories from .NET backend
async function fetchCategories() {
    try {
        const response = await fetch('http://localhost:8083/api/categories');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const categories = await response.json();
        return categories;
    } catch (error) {
        console.error('Error fetching categories:', error);
        return [];
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    const categories = await fetchCategories();
    // You can use the categories data here to populate your UI
    console.log('Fetched categories:', categories);
});


const profile = html`
  <div class="card">
    <span>User ID: ${12345}</span>
  </div>
`;