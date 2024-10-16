document.addEventListener("DOMContentLoaded", () => {
    const content = document.getElementById("content");

    document.getElementById("homeLink").addEventListener("click", () => {
        window.location.href = 'index.html';
    });

    document.getElementById("reviewsLink").addEventListener("click", () => {
        content.innerHTML = `<h2>Customer Reviews</h2><p>Here customers can leave reviews and comments.</p>`;
    });

    document.getElementById("ordersLink").addEventListener("click", () => {
        content.innerHTML = `<h2>Purchase Orders</h2><p>Functionality to create, send, update, or cancel orders.</p>`;
    });

    document.getElementById("paymentsLink").addEventListener("click", () => {
        content.innerHTML = `<h2>Payments</h2><p>Functionality to facilitate payments for orders.</p>`;
    });

    document.getElementById("quotationsLink").addEventListener("click", () => {
        content.innerHTML = `<h2>Request Quotations</h2><p>Functionality for new customers to request quotations.</p>`;
    });
});
