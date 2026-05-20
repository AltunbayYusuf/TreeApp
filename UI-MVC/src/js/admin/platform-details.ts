async function loadTotalAiCost() {
    const costElement = document.getElementById("total-ai-cost");

    if (!costElement) return;

    const subPlatformId = Number(costElement.dataset.subplatformId);

    const response = await fetch(
        `/api/aiusage/total-cost?subPlatformId=${subPlatformId}`
    );

    if (!response.ok) return;

    const data = await response.json();

    costElement.innerText = `€ ${data.totalCost.toFixed(4)}`;
}

loadTotalAiCost();

setInterval(loadTotalAiCost, 5000);