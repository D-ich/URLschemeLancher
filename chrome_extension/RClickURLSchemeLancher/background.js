chrome.runtime.onInstalled.addListener(() => {
    chrome.contextMenus.create({
        id: "openMyEditor",
        title: "Open in my editor!",
        contexts: ["selection", "image"] //only on text and image
    });
    console.log("Context menu created");
});

// right click event
chrome.contextMenus.onClicked.addListener((info, tab) => {
    if (info.menuItemId === "openMyEditor") {
        if (info.selectionText) {
            console.log("Text selected:", info.selectionText);
            saveTextToFile(info.selectionText)
                .then((filePath) => {
                    const customUrl = `launchurl:exec=TextEditor;file=file://${filePath}`;
                    chrome.tabs.create({ url: customUrl });
                })
                .catch((error) => {
                    console.error("Error saving text:", error);
                });
        } else if (info.srcUrl) {
            console.log("Image selected:", info.srcUrl);
            const customUrl = `launchurl:exec=ImageEditor;file=${info.srcUrl}`;
            chrome.tabs.create({ url: customUrl });
        }
    }
});

// saving text
function saveTextToFile(text) {
    return new Promise((resolve, reject) => {
        try {
            const blob = new Blob([text], { type: "text/plain" });
            const fileName = `selected_text_${Date.now()}.txt`;

            // read Blob
            const reader = new FileReader();
            reader.onloadend = () => {
                const url = reader.result;

                chrome.downloads.download(
                    {
                        url: url,
                        filename: fileName,
                        saveAs: false
                    },
                    (downloadId) => {
                        if (chrome.runtime.lastError) {
                            reject(chrome.runtime.lastError.message);
                        } else {
                            waitForDownloadCompletion(downloadId)
                                .then((filePath) => resolve(filePath))
                                .catch((error) => reject(error));
                        }
                    }
                );
            };

            reader.onerror = () => {
                reject("Failed to read the blob");
            };

            reader.readAsDataURL(blob);
        } catch (error) {
            reject(error.message);
        }
    });
}

function waitForDownloadCompletion(downloadId) {
    return new Promise((resolve, reject) => {
        const interval = setInterval(() => {
            chrome.downloads.search({ id: downloadId }, (results) => {
                if (results && results[0]) {
                    const downloadItem = results[0];
                    if (downloadItem.state === "complete") {
                        clearInterval(interval);
                        resolve(downloadItem.filename); 
                    }
                }
            });
        }, 100); // confirm every 100ms

        // timeout（10s）
        setTimeout(() => {
            clearInterval(interval);
            reject("Download timed out");
        }, 10000);
    });
}
