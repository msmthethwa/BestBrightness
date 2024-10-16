document.addEventListener('DOMContentLoaded', function () {
    // Add event listeners to remove buttons
    const removeButtons = document.querySelectorAll('.remove-btn');

    removeButtons.forEach(button => {
        button.addEventListener('click', function () {
            this.closest('.cart-item').remove();
            updateCartSummary();
        });
    });

    // Add event listeners to quantity inputs
    const quantityInputs = document.querySelectorAll('.quantity-input');

    quantityInputs.forEach(input => {
        input.addEventListener('change', function () {
            const productId = this.dataset.productId;
            const quantity = parseInt(this.value, 10);
            const pricePerUnit = parseFloat(this.dataset.pricePerUnit);

            if (quantity <= 0) {
                alert('Quantity must be greater than 0.');
                this.value = 1; // Reset to default quantity
                return;
            }

            updateTotalPrice(this, pricePerUnit);

            // Update quantity on the server
            fetch('@Url.Action("UpdateQuantity", "Cart")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': '@(ViewData["__RequestVerificationToken"])'
                },
                body: JSON.stringify({ productId: productId, quantity: quantity })
            })
                .then(response => response.json())
                .then(data => {
                    if (!data.success) {
                        alert(data.message);
                    }
                    updateCartSummary(); // Refresh the cart summary
                })
                .catch(error => console.error('Error updating quantity:', error));
        });
    });

    // Function to update the total price for an item
    function updateTotalPrice(input, pricePerUnit) {
        var quantity = parseInt(input.value, 10);
        var totalPriceElement = input.closest('.cart-item').querySelector('.total-price');
        var newTotalPrice = quantity * pricePerUnit;

        // Update the total price for this item
        totalPriceElement.textContent = newTotalPrice.toFixed(2);

        // Update the subtotal and total amounts
        updateCartSummary();
    }

    // Function to update the cart summary
    function updateCartSummary() {
        var total = 0;

        document.querySelectorAll('.total-price').forEach(function (priceElement) {
            total += parseFloat(priceElement.textContent);
        });

        // Update the subtotal and grand total
        var subtotalElement = document.getElementById('subtotal');
        var taxElement = document.getElementById('tax');
        var grandTotalElement = document.getElementById('grandTotal');

        var subtotal = total;
        var tax = subtotal * 0.05; // 5% tax for example
        var grandTotal = subtotal + tax;

        subtotalElement.textContent = subtotal.toFixed(2);
        taxElement.textContent = tax.toFixed(2);
        grandTotalElement.textContent = grandTotal.toFixed(2);
    }
});