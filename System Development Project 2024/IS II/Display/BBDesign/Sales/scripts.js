// scripts.js
document.addEventListener('DOMContentLoaded', function() {
    // Sales Form Submission
    const salesForm = document.getElementById('salesForm');
    if (salesForm) {
        salesForm.addEventListener('submit', function(event) {
            event.preventDefault();
            const product = document.getElementById('product').value;
            const quantity = document.getElementById('quantity').value;
            const price = document.getElementById('price').value;

            const salesRecords = document.getElementById('salesRecords');
            salesRecords.innerHTML += `<p>Product: ${product}, Quantity: ${quantity}, Price per Unit: ${price}</p>`;
            salesForm.reset();
        });
    }

    // Inventory Form Submission
    const inventoryForm = document.getElementById('inventoryForm');
    if (inventoryForm) {
        inventoryForm.addEventListener('submit', function(event) {
            event.preventDefault();
            const itemName = document.getElementById('itemName').value;
            const itemQuantity = document.getElementById('itemQuantity').value;
            const itemPrice = document.getElementById('itemPrice').value;

            const inventoryRecords = document.getElementById('inventoryRecords');
            inventoryRecords.innerHTML += `<p>Item: ${itemName}, Quantity: ${itemQuantity}, Price per Unit: ${itemPrice}</p>`;
            inventoryForm.reset();
        });
    }

    // Report Form Submission
    const reportForm = document.getElementById('reportForm');
    if (reportForm) {
        reportForm.addEventListener('submit', function(event) {
            event.preventDefault();
            const reportType = document.getElementById('reportType').value;
            const startDate = document.getElementById('startDate').value;
            const endDate = document.getElementById('endDate').value;

            const reportResults = document.getElementById('reportResults');
            reportResults.innerHTML += `<p>Report Type: ${reportType}, Start Date: ${startDate}, End Date: ${endDate}</p>`;
            reportForm.reset();
        });
    }
});

// scripts.js
document.addEventListener("DOMContentLoaded", () => {
    // Handle showing and hiding the container with animation
    const container = document.querySelector('.container');
    container.classList.add('hidden');
    setTimeout(() => {
        container.classList.remove('hidden');
        container.classList.add('show');
    }, 100);

    // Modal functionality
    const modals = document.querySelectorAll('.modal');
    const openModalBtns = document.querySelectorAll('.open-modal');
    const closeModalBtns = document.querySelectorAll('.modal-close');

    openModalBtns.forEach((btn, index) => {
        btn.addEventListener('click', () => {
            modals[index].style.display = "flex";
        });
    });

    closeModalBtns.forEach((btn) => {
        btn.addEventListener('click', () => {
            btn.closest('.modal').style.display = "none";
        });
    });

    window.addEventListener('click', (event) => {
        modals.forEach(modal => {
            if (event.target == modal) {
                modal.style.display = "none";
            }
        });
    });
});
