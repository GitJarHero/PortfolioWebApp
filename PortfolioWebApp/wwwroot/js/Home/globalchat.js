window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
    else {
        document.querySelector('.scrollable').scrollTop = document.querySelector('.scrollable').scrollHeight
    }
};
