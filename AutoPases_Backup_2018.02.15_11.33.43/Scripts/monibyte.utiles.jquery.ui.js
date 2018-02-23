/**********************DIALOGOS**********************/
jQuery.fn.okDialog = function (options) {
    $(this).dialog(options);
};
jQuery.fn.okcancelDialog = function (options) {
    $(this).dialog(options);
};
jQuery.fn.dialog = function (settings) {
    var dialog = $(this);
    if (settings == "open") {
        var win = $(dialog).data("kendoWindow");
        if (win.element.is(":hidden")) {
            win.center().open();
        }
        return;
    }
    else if (settings == "close") {
        $(dialog).data("kendoWindow").close();
        return;
    }
    var options = $.extend({
        modal: true, autoOpen: false, autoRemove: true, resizable: false,
        closeOnEscape: false, title: "", width: "auto", height: "auto",
    }, settings);
    options.title = options.title;
    options.visible = false;
    if (options.autoRemove) {
        options.deactivate = function () {
            this.destroy();
        }
    }
    options.close = function () {
        var postFunction = $(dialog).data("postFunction");
        if ($(dialog).data("success") && postFunction) {
            if ($(dialog).data("postData")) {
                postFunction($(dialog).data("postData"));
            } else {
                postFunction();
            }
        }
    };
    $(dialog).find(".ui-cancel-btn").on("click", function () {
        $(dialog).data("success", false).dialog("close");
    });
    var kendoWindow = $(dialog).kendoWindow(options).data("kendoWindow");
    if (options.autoOpen) {
        kendoWindow.center().open();
    }
};
/**********************DIALOGOS**********************/
/***********************JPOPUP***********************/
window.alert = function (msj) {
    $.jAlert(msj);
};
jQuery.fn.confirmSubmit = function (options) {
    var defaults = $.extend({
        Message: "Confirmar"
    }, options);
    var submitting = false;
    $(this).bind("submit", function (e) {
        if ($(this).valid()) {
            e.preventDefault();
            if (submitting) {
                submitting = false;
                return true;
            };
            var readySubmit = false;
            if (defaults.Condition) {
                if (defaults.Condition()) {
                    readySubmit = true;
                }
            } else {
                readySubmit = true;
            }
            if (readySubmit) {
                var form = this;
                $.jConfirm(defaults.Message, "info", function (conf) {
                    if (conf) {
                        submitting = true;
                        $(form).submit();
                    } else {
                        if (defaults.OnCancel) {
                            defaults.OnCancel();
                        }
                    }
                }, defaults);
            }
        }
        return false;
    });
};
(function ($) {
    $.buildMarkup = function (message, errorLevel) {
        var cssClass =
            (errorLevel == "error") ? "ui-state-error" :
            (errorLevel == "highlight") ? "ui-state-highlight" : 
            (errorLevel == "normal") ? "ui-state-normal" : "";
        var th = $("<th/>", { html: $("<span/>", { "class": "k-icon k-i-note" }) });
        var td = $("<td/>", { valign: "middle", html: message, "class": cssClass });
        return $("<div/>").append($("<table/>").append($("<tr/>").append(th).append(td)));
    };
    $.buildWindow = function (options) {
        options = $.extend({
            modal: true,
            width: "auto",
            minWidth: 200,
            resizable: false,
            errorLevel: "info",
            close: function () {
                this.destroy();
            }
        }, options);
        options.actions = [];
        var divmarkup = $.buildMarkup(options.message, options.errorLevel);
        var mywindow = $(divmarkup).kendoWindow(options).data("kendoWindow");
        mywindow.wrapper.addClass("jpopup no-close" + (options.title ? "" : " no-title"));
        var buildButton = function (text, callbackresp) {
            return $("<button/>", {
                type: "button", role: "button", text: text,
                "class": callbackresp ? "ui-accept-btn" : "ui-cancel-btn",
                click: function () {
                    mywindow.close();
                    if (options.callback) {
                        options.callback(callbackresp);
                    }
                }
            });
        };
        var buttonsBar = $("<div/>", { "class": "ui-dialog-buttonpane clearLine" });
        if (options.okBtn) {
            buttonsBar.append(buildButton(options.okText, true));
        }
        if (options.cancelBtn) {
            buttonsBar.append(buildButton(options.cancelText, false));
        }
        divmarkup.append(buttonsBar);
        mywindow.center();
        mywindow.open();
    };
    $.jConfirm = function (message, errorLevel, callback, options) {
        var defaults = {
            okBtn: true,
            cancelBtn: true,
            message: message,
            callback: callback,
            errorLevel: errorLevel,
            okText: options ? options.OkText : "Aceptar",
            cancelText: options ? options.CancelText : "Cancelar",
        }
        $.buildWindow(defaults);
    };
    $.jAlert = function (message, errorLevel, callback, options) {
        var defaults = {
            okBtn: true,
            message: message,
            callback: callback,
            errorLevel: errorLevel,
            okText: options ? options.OkText : "Aceptar"
        };
        $.buildWindow(defaults);
    };
})(jQuery);
/***********************JPOPUP***********************/
