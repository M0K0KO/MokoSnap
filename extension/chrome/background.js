const HOST_NAME = "com.mokosnap.chrome_capture";

async function captureTabs() {
  const windows = await chrome.windows.getAll({ populate: true, windowTypes: ["normal"] });
  const capturedAt = new Date().toISOString();
  const capturedWindows = windows.map((window) => ({
    windowId: window.id,
    tabs: (window.tabs || []).map((tab) => ({
      windowId: window.id,
      tabId: tab.id,
      title: tab.title || "",
      url: tab.url || "",
      active: Boolean(tab.active),
      pinned: Boolean(tab.pinned),
      index: tab.index
    }))
  }));

  return {
    type: "captureTabs",
    capturedAt,
    windows: capturedWindows,
    tabs: capturedWindows.flatMap((window) => window.tabs)
  };
}

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (!message || message.type !== "captureTabs") {
    return false;
  }

  captureTabs()
    .then((capture) => chrome.runtime.sendNativeMessage(HOST_NAME, capture))
    .then((response) => sendResponse(response || { success: false, message: "Native host returned no response." }))
    .catch((error) => sendResponse({ success: false, message: error.message }));

  return true;
});
