/// ============================================================
/// Original Code Daniel Turan aka Liero  - https://github.com/Liero
/// Modified By: Shaun Curtis, Cold Elm Coders
/// License:  Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================


let blazr_PreventNavigation = false;  // main control bool to stop/allow navigation
let idCounter = 0;
let lastHistoryItemId = 0; //needed to detect back/forward, in popstate event handler

window.blazr_setPageLock = function (lock) {
    //if (lock) {
    //    window.addEventListener('popstate', blazr_popStateListener, { capture: true });
    //}
    //else {
    //    window.removeEventListener('popstate', blazr_popStateListener, { capture: true });
    //}
    blazr_PreventNavigation = lock;
}

window.history.pushState = (function (basePushState) {
    return function (...args) {
        if (blazr_PreventNavigation) {
            return;
        }
        basePushState.apply(this, args);
    }
})(window.history.pushState);

window.addEventListener('beforeunload', e => {
    if (blazr_PreventNavigation) {
        e.returnValue = 'There are unsaved changes'
    }
});


//let blazr_resetUrlRun = false;

//window.blazr_popStateListener = function (e) {
//    let lockNavigation = false;

//    //popstate can be triggered twice, but we want to show confirm dialog only once
//    lockNavigation = blazr_PreventNavigation && !blazr_resetUrlRun;

//    if (lockNavigation) {
//        //this will cancel Blazor navigation, but the url is already changed
//        e.stopImmediatePropagation();
//        e.preventDefault();
//        e.returnValue = false;
//    }

//    if (blazr_resetUrlRun) {
//        //Resets resetUrlRun on second run
//        blazr_resetUrlRun = false;
//    }
//}


window.addEventListener('load', () => {
    let resetUrlRun = false;
    function popStateListener(e) {
        if (blazr_PreventNavigation) {
            let lockNavigation = false;

            //popstate can be triggered twice, but we want to show confirm dialog only once
            lockNavigation = blazr_PreventNavigation && !resetUrlRun;

            if (lockNavigation) {
                //this will cancel Blazor navigation, but the url is already changed
                e.stopImmediatePropagation();
                e.preventDefault();
                e.returnValue = false;
            }

            if (resetUrlRun) {
                //Resets resetUrlRun on second run
                resetUrlRun = false;
            }

            // Only runs on the first pass through    
            if (lockNavigation) {
                //detect back vs forward 
                const currentId = e.state && e.state.__incrementalId;
                const navigatingForward = currentId > lastHistoryItemId;

                //revert url
                if (navigatingForward) {
                    history.back();
                }
                else {
                    history.forward();
                }
                resetUrlRun = true;
            }
        }
    }


    window.addEventListener('popstate', popStateListener, { capture: true });

    window.history.pushState = (function (basePushState) {
        return function (...args) {
            //if (blazr_PreventNavigation && confirm('There are unsaved changes. Would you like to stay?')) {
            if (blazr_PreventNavigation) {
                return;
            }
            if (!args[0]) {
                args[0] = {};
            }
            lastHistoryItemId = history.state && history.state.__incrementalId;
            args[0].__incrementalId = ++idCounter; //track order of history items            
            basePushState.apply(this, args);
        }
    })(window.history.pushState);
});
