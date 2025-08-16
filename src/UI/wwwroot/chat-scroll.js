(function () {
    // Avoid re-defining during hot reloads
    if (window.chatScroll) return;

    function _isNearBottom(el, px = 120) {
        if (!el) return true;
        const remaining = el.scrollHeight - el.scrollTop - el.clientHeight;
        return remaining <= px;
    }

    const api = {
        toBottom(el, opts) {
            if (!el) return;
            const smooth = opts?.smooth === true;
            const onlyIfNearBottom = opts?.onlyIfNearBottom ?? 0;

            if (onlyIfNearBottom && !_isNearBottom(el, onlyIfNearBottom)) return;

            const top = el.scrollHeight;
            if (el.scrollTo) {
                el.scrollTo({ top, behavior: smooth ? 'smooth' : 'auto' });
            } else {
                el.scrollTop = top;
            }
        },
        // Convenience: ensure last child is visible
        lastIntoView(el, opts) {
            if (!el || !el.lastElementChild) return;
            el.lastElementChild.scrollIntoView({ behavior: opts?.smooth ? 'smooth' : 'auto', block: 'end' });
        }
    };

    // Expose
    window.chatScroll = api;
})();