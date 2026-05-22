const captureButton = document.getElementById("capture");
const statusElement = document.getElementById("status");

captureButton.addEventListener("click", async () => {
  captureButton.disabled = true;
  statusElement.textContent = "Capturing...";

  try {
    const response = await chrome.runtime.sendMessage({ type: "captureTabs" });
    if (response && response.success) {
      statusElement.textContent = "Captured tabs for MokoSnap.";
    } else {
      statusElement.textContent = response?.message || "Capture failed.";
    }
  } catch (error) {
    statusElement.textContent = error.message;
  } finally {
    captureButton.disabled = false;
  }
});
