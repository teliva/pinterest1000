// Simple fetch to get categories from .NET backend
const backendURL = "http://localhost:8083/api";

// State variables
let selectedCategory = 0;
let selectedRoomType = 0;
let selectedStyle = 0;

async function fetchCategories() {
  try {
    const response = await fetch(`${backendURL}/categories`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const categories = await response.json();
    return categories;
  } catch (error) {
    console.error("Error fetching categories:", error);
    return [];
  }
}

async function fetchRoomTypes() {
  try {
    const response = await fetch(`${backendURL}/roomtypes`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const roomTypes = await response.json();
    return roomTypes;
  } catch (error) {
    console.error("Error fetching roomTypes:", error);
    return [];
  }
}

async function fetchStyles() {
  try {
    const response = await fetch(`${backendURL}/styles`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const styles = await response.json();
    return styles;
  } catch (error) {
    console.error("Error fetching styles:", error);
    return [];
  }
}

async function populateCategoryFilter() {
  const categories = await fetchCategories();
  const filterRef = document.querySelector("#category-filter");

  // add appropriate event listener
  filterRef.addEventListener("change", (event) => {
    selectedCategory = Number(event.target.value);
  });

  categories.forEach((category) => {
    let op = document.createElement("option");
    op.value = category.categoryId;
    op.textContent = category.description;

    filterRef.appendChild(op);
  });
}

async function populateRoomTypeFilter() {
  const roomTypes = await fetchRoomTypes();
  const filterRef = document.querySelector("#room-type-filter");

  // add appropriate event listener
  filterRef.addEventListener("change", (event) => {
    selectedRoomType = Number(event.target.value);
  });

  roomTypes.forEach((roomType) => {
    let op = document.createElement("option");
    op.value = roomType.roomTypeId;
    op.textContent = roomType.description;
    filterRef.appendChild(op);
  });
}

async function populateStylesFilter() {
  const styles = await fetchStyles();
  const filterRef = document.querySelector("#styles-filter");

  // add appropriate event listener
  filterRef.addEventListener("change", (event) => {
    selectedStyle = Number(event.target.value);
  });

  styles.forEach((style) => {
    let op = document.createElement("option");
    op.value = style.styleId;
    op.textContent = style.description;
    filterRef.appendChild(op);
  });
}

async function getImages() {
  try {
    const url = new URL(`${backendURL}/images`);
    // Only append if the value is truthy
    if (selectedCategory)
      url.searchParams.append("categoryId", selectedCategory);
    if (selectedRoomType)
      url.searchParams.append("roomTypeId", selectedRoomType);
    if (selectedStyle) url.searchParams.append("styleId", selectedStyle);

    const response = await fetch(url);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const images = await response.json();
    return images;
  } catch (error) {
    console.error("Error fetching images:", error);
    return [];
  }
}

async function populateImagesFromSearch(){
  let images = await getImages();

  const container = document.getElementById('image-output-container');
  container.replaceChildren();

  images.slice(0,3).forEach(image => {
    container.appendChild(insertImageCard(image.id));
  });
  

  document.querySelector('#totalNumResults').textContent = images.length;
}

function insertImageCard(imageUrl) {
  // 1. Get references to the template and the target container
  const template = document.getElementById('custom-image');
  

  // 2. Clone the template content (true means deep clone, including all child elements)
  const clone = template.content.cloneNode(true);

  // 3. Find the elements inside the clone and inject your data
  const imgElement = clone.querySelector('#card-img');
  imgElement.src = `http://localhost:8081/${imageUrl}.png`;

  // 4. Append the fully populated clone into the DOM container
  return clone;
}










// Application entry point
document.addEventListener("DOMContentLoaded", async () => {
  await populateCategoryFilter();
  await populateRoomTypeFilter();
  await populateStylesFilter();
  await populateImagesFromSearch();

  document.querySelector('#search-button').addEventListener('click', (event) => populateImagesFromSearch());
});




