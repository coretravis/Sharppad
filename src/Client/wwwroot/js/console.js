/**
 * Initializes the console.
 * @param {HTMLElement} element - The console element.
 */
export function initConsole(element) {
    console.log("Console initialized");
}

/**
 * Scrolls the console element to the bottom.
 * @param {HTMLElement} element - The console element.
 */
export function scrollToBottom(element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}

/**
 * Sets focus on the specified element.
 * @param {HTMLElement} element - The element to focus on.
 */
export function focusElement(element) {
    if (element) {
        element.focus();
    }
}

/**
 * Requests fullscreen mode for the specified element.
 * @param {HTMLElement} element - The element to request fullscreen for.
 * @param {boolean} isFullscreen - Indicates whether to enter fullscreen mode or exit it.
 */
export function requestFullscreenForElement(element, isFullscreen) {
    if (!element) return;

    if (isFullscreen) {
        if (element.requestFullscreen) {
            element.requestFullscreen();
        } else if (element.webkitRequestFullscreen) {
            element.webkitRequestFullscreen();
        } else if (element.msRequestFullscreen) {
            element.msRequestFullscreen();
        }
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
    }
}

/**
 * Scrolls the console element to the specified entry index.
 * @param {HTMLElement} element - The console element.
 * @param {number} entryIndex - The index of the entry to scroll to.
 */
export function scrollToEntry(element, entryIndex) {
    if (!element) return;

    const entries = element.querySelectorAll('.console-entry');
    if (entries.length > entryIndex) {
        entries[entryIndex].scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

/**
 * Exports the console data as a text file.
 * @param {string} content - The content to export.
 */
export function exportConsoleData(content) {
    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `console-export-${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// Input handling
/**
 * The task for input completion.
 */
export let inputCompletionTask = null;

/**
 * The .NET helper for input completion.
 */
export let dotNetHelper = null;

/**
 * Sets the input completion task and .NET helper.
 * @param {object} helper - The .NET helper object.
 * @param {string} task - The input completion task.
 */
export function setInputCompletionTask(helper, task) {
    dotNetHelper = helper;
    inputCompletionTask = task;
}

/**
 * Completes the input using the .NET helper.
 * @param {string} input - The input to complete.
 */
export function completeInput(input) {
    if (dotNetHelper && inputCompletionTask) {
        dotNetHelper.invokeMethodAsync('CompleteInput', input, inputCompletionTask);
    }
}