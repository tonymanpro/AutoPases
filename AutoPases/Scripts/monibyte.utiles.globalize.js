//$.validator.setDefaults({ ignore: ":hidden:not([data-role='combobox'])" });

var originalMethods = {
    min: $.validator.methods.min,
    max: $.validator.methods.max,
    range: $.validator.methods.range
};
// Tell the validator that we want numbers parsed using $monibyte
$.validator.methods.number = function (value, element) {
    var val = $monibyte.parseFloat(value);
    return this.optional(element) || ($.isNumeric(val));
};
// Tell the validator that we want dates parsed using $monibyte
$.validator.methods.date = function (value, element) {
    var val = $monibyte.parseDate(value);
    return this.optional(element) || (val);
};
// Tell the validator that we want numbers parsed using $monibyte, 
// then call into original implementation with parsed value
$.validator.methods.min = function (value, element, param) {
    var val = $monibyte.parseFloat(value);
    return originalMethods.min.call(this, val, element, param);
};
$.validator.methods.max = function (value, element, param) {
    var val = $monibyte.parseFloat(value);
    return originalMethods.max.call(this, val, element, param);
};
$.validator.methods.range = function (value, element, param) {
    var val = $monibyte.parseFloat(value);
    return originalMethods.range.call(this, val, element, param);
};

$("input[data-val-length-max]").each(function (index, element) {
    var length = parseInt($(this).attr("data-val-length-max"));
    $(this).prop("maxlength", length);
});

function refreshTypeValidators(id) {
    $(id).kendoValidator().data("kendoValidator").destroy();
    $(id).kendoValidator({
        rules: {
            comboBox: function (input) {
                if ((input.is("[role='combobox']") && input.is("[aria-autocomplete='both']"))
                    || input.is("[aria-autocomplete='list']")) {
                    var value = $(input).val();
                    var name = input[0].name.replace("_input", "");;
                    var cbx = $("#" + name).data("kendoComboBox");
                    if (value && cbx.selectedIndex == -1) {
                        return false;
                    }
                    else {
                        return true;
                    }
                }
                else {
                    return true;
                }
            }
        }
    });
}


$.validator.unobtrusive.observable = function (element) {
    $(function () {
        $(document).ready(function () {
            if (element) {
                window.refreshTypeValidators(element)
                $.validator.unobtrusive.parse(element);
            }
            var selectors = [
                "[data-role^=date]",
                "[data-role=combobox]",
                "[data-role=multiselect]",
                "[data-role=dropdownlist]",
                "[data-role=numerictextbox]"
            ];
            var elements = $(element).find(selectors.join());
            //correct mutation event detection
            var hasMutationEvents = ("MutationEvent" in window);
            var MutationObserver = window.WebKitMutationObserver || window.MutationObserver;
            var updateCssOnPropertyChange = function (e) {
                var _element = $(e.target);
                _element.siblings("span.k-dropdown-wrap")
                    .add(_element.siblings("div.k-multiselect-wrap"))
                    .add(_element.parent("span.k-numeric-wrap"))
                    .add(_element.parent("span.k-picker-wrap"))
                    .toggleClass("k-invalid", _element.hasClass("input-validation-error"));
            }
            if (MutationObserver) {
                var observer = new MutationObserver(function (mutations) {
                    var idx = 0,
                        mutation,
                        length = mutations.length;

                    for (; idx < length; idx++) {
                        mutation = mutations[idx];
                        if (mutation.attributeName === "class") {
                            updateCssOnPropertyChange(mutation);
                        }
                    }
                }),
                config = { attributes: true, childList: false, characterData: false };
                elements.each(function () {
                    observer.observe(this, config);
                });
            } else if (hasMutationEvents) {
                elements.bind("DOMAttrModified", updateCssOnPropertyChange);
            } else {
                elements.each(function () {
                    this.attachEvent("onpropertychange", updateCssOnPropertyChange);
                });
            }
        });
    });
};
$.validator.unobtrusive.observable();