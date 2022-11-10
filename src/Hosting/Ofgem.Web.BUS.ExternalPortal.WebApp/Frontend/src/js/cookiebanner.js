(function () {
    "use strict";
    let root = this;
    if (typeof root.GOVUK === 'undefined') { root.GOVUK = {}; }

    /*
      Cookie methods
      ==============
  
      Usage:
  
        Setting a cookie:
        GOVUK.cookie('hobnob', 'tasty', { days: 30 });
  
        Reading a cookie:
        GOVUK.cookie('hobnob');
  
        Deleting a cookie:
        GOVUK.cookie('hobnob', null);
    */
    GOVUK.cookie = function (name, value, options) {
        if (typeof value !== 'undefined') {
            if (value === false || value === null) {
                return GOVUK.setCookie(name, '', { days: -1 });
            } else {
                return GOVUK.setCookie(name, value, options);
            }
        } else {
            return GOVUK.getCookie(name);
        }
    };
    GOVUK.setCookie = function (name, value, options) {
        if (typeof options === 'undefined' || options === null) {
            options = {};
        }
        let cookieString = name + "=" + value + "; path=/";
        if (options.days) {
            let date = new Date();
            date.setTime(date.getTime() + (options.days * 24 * 60 * 60 * 1000));
            cookieString = cookieString + "; expires=" + date.toGMTString();
        }
        if (document.location.protocol === 'https:') {
            cookieString = cookieString + "; Secure";
        }
        document.cookie = cookieString;
    };
    GOVUK.getCookie = function (name) {
        let nameEq = name + "=";
        let cookies = document.cookie.split(';');
        for (let i = 0, len = cookies.length; i < len; i++) {
            let cookie = cookies[i];
            while (cookie.charAt(0) === ' ') {
                cookie = cookie.substring(1, cookie.length);
            }
            if (cookie.indexOf(nameEq) === 0) {
                return decodeURIComponent(cookie.substring(nameEq.length));
            }
        }
        return null;
    };
}).call(this);


/* Set cookie on accept */

$("button[value='accept']").click(function () {
    GOVUK.cookie('cookies.analytics', 'true', { days: 365 });
    GOVUK.cookie('cookies.essential', 'true', { days: 365 });
    $("#govuk-cookie-banner__message").attr("hidden", "true");
    $("#govuk-cookie-banner__accepted").removeAttr("hidden").focus();
});


/* Set cookie on reject */

$("button[value='reject']").click(function () {
    GOVUK.cookie('cookies.analytics', 'false', { days: 365 });
    GOVUK.cookie('cookies.essential', 'true', { days: 365 });
    $("#govuk-cookie-banner__message").attr("hidden", "true");
    $("#govuk-cookie-banner__rejected").removeAttr("hidden").focus();

});

/* Cookie Policy Radios */
$("#analytics--submit").click(function () {
    if ($("input:radio[name='analytics-cookies']:checked").val() == 'yes') {
        GOVUK.cookie('cookies.analytics', 'true', { days: 365 });
        GOVUK.cookie('cookies.essential', 'true', { days: 365 });
    }

    if ($("input:radio[name='analytics-cookies']:checked").val() == 'no') {
        GOVUK.cookie('cookies.analytics', 'false', { days: 365 });
        GOVUK.cookie('cookies.essential', 'true', { days: 365 });
    }

    $(".govuk-notification-banner--success").removeClass("bus--hidden").focus();
});

function checkCookie() {
    let analyticsCookies = GOVUK.cookie('cookies.analytics');

    if (analyticsCookies === "true") {
        $("input:radio[name='analytics-cookies'][value='yes']").prop("checked", true);
    }
    else {
        $("input:radio[name='analytics-cookies'][value='no']").prop("checked", true);
    }

}

/* Show Hide Banner */

let essentialCookies = GOVUK.cookie('cookies.essential');

$(document).ready(function () {
    $(".govuk-cookie-banner").removeClass("bus--hidden");

    if (essentialCookies === "true") {
        $(".govuk-cookie-banner").attr("hidden", "true");
    }
});

$("button[value='hide']").click(function () {
    $(".govuk-cookie-banner").attr("hidden", "true");
});