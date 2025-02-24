/**
 * Scrolls the given element to the bottom.
 * @param {HTMLElement} element - The element to scroll.
 */
export function scrollToBottom(element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}