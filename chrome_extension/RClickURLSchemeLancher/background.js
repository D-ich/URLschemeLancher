// 右クリックメニューを作成
chrome.runtime.onInstalled.addListener(() => {
    chrome.contextMenus.create({
        id: "openMyEditor",
        title: "★マイエディタで開く",
        contexts: ["selection", "image"] // テキスト選択または画像に表示
    });
    console.log("Context menu created");
});

// 右クリックメニューのクリックイベントを処理
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

// テキストを保存する関数
function saveTextToFile(text) {
    return new Promise((resolve, reject) => {
        try {
            const blob = new Blob([text], { type: "text/plain" });
            const fileName = `selected_text_${Date.now()}.txt`;

            // FileReaderでBlobを読み込む
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
                            // ダウンロード完了を待つ
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

            reader.readAsDataURL(blob); // BlobをData URLとして読み込む
        } catch (error) {
            reject(error.message);
        }
    });
}

// ダウンロード完了を待つ関数
function waitForDownloadCompletion(downloadId) {
    return new Promise((resolve, reject) => {
        const interval = setInterval(() => {
            chrome.downloads.search({ id: downloadId }, (results) => {
                if (results && results[0]) {
                    const downloadItem = results[0];
                    if (downloadItem.state === "complete") {
                        clearInterval(interval);
                        resolve(downloadItem.filename); // フルパスを返す
                    }
                }
            });
        }, 100); // 100msごとに確認

        // タイムアウト処理（例: 10秒）
        setTimeout(() => {
            clearInterval(interval);
            reject("Download timed out");
        }, 10000);
    });
}
