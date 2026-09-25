const searchInput = document.getElementById("search-input");
const suggestions = document.getElementById("suggestions")

let timeout;

searchInput.addEventListener("input", function() {
    clearTimeout(timeout);

    const query = this.value.trim();

    if (query.length < 2) {
        suggestions.innerHTML="";
        suggestions.style.display = "none"
        return;
    }

    timeout = setTimeout(async() => {
        try{
            const response = await fetch(`/Home/Suggestions?query=${encodeURIComponent(query)}`);

            if (!response.ok){
                throw new Error("Failed to fetch suggestions through controller")
            }

            const cities = await response.json();

            suggestions.innerHTML = "";

            if (cities.length === 0) {
                suggestions.style.display ="none"
                return
            }

            cities.forEach(city => {
                const item = document.createElement("div");
                item.classList.add("suggestion-item");
                item.textContent = `${city.name}, ${city.country}`;
                item.addEventListener("click", () => {
                searchInput.value = city.name;

                document.getElementById("lat-input").value = city.latitude;
                document.getElementById("lng-input").value = city.longitude;
                document.getElementById("country-input").value = city.country;
                document.getElementById("state-input").value = city.stateOrRegion ?? "";

                suggestions.innerHTML = "";
                suggestions.style.display = "none";

                searchInput.closest("form").submit();
            });

                suggestions.appendChild(item);
            })
            suggestions.style.display = "block";
      
        } 
        catch (error){
            console.error("Error getting city suggestions:", error);
        }
    }, 150);
});

 document.addEventListener("click", function (event) {

        if (!event.target.closest(".autocomplete-container")) {
            suggestions.style.display = "none";

        }

    });