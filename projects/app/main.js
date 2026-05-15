// Simple fetch to get categories from .NET backend
const backendURL = "http://localhost:8083/api";

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
    console.error("Error fetching roomTypes:", error);
    return [];
  }
}

document.addEventListener("DOMContentLoaded", async () => {
  await populateCategoryFilter();
  await populateRoomTypeFilter();
  await populateStylesFilter();
});

async function populateCategoryFilter() {
  const categories = await fetchCategories();
  const filterRef = document.querySelector("#category-filter");

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

  styles.forEach((style) => {
    let op = document.createElement("option");
    op.value = style.styleId;
    op.textContent = style.description;
    filterRef.appendChild(op);
  });
}