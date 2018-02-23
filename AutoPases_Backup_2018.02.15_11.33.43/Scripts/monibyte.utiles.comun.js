/*******prototype*********/
String.prototype.padLeft = function (n, s) {
    var t = this, L = s.length;
    while (t.length + L <= n) t = s + t;
    if (t.length < n) t = s.substring(0, n - t) + t;
    return t;
}
String.prototype.padRight = function (n, s) {
    var t = this, L = s.length;
    while (t.length + L <= n) t += s;
    if (t.length < n) t += s.substring(0, n - t);
    return t;
}
/*******prototype*********/
$monibyte = {
    parseBool: function (value) {
        if (value) {
            return eval(value.toLowerCase());
        }
        return false;
    },
    parseDate: function (value) {
        return kendo.parseDate(value);
    },
    parseFloat: function (value) {
        return kendo.parseFloat(value);
    },
    parseInt: function (value) {
        return kendo.parseInt(value);
    },
    formatNumber: function (value, settings) {
        var options = jQuery.extend({
            float: true,
            group: true,
        }, settings);
        var result = kendo.toString(value, options.float ? "n" : "n0");
        if (!options.group) {
            if (result) {
                result = result.replace(kendo.culture().numberFormat[","], "");
            }
        }
        return result;
    },
    formatDate: function (value, settings) {
        var options = jQuery.extend({
            format: "d"
        }, settings);
        return kendo.toString(value ? value : "", options.format);
    },
    format: function (str, args) {
        args = typeof args === 'object' ? args : Array.prototype.slice.call(arguments, 1);
        return str.replace(/\{\{|\}\}|\{(\w+)\}/g, function (m, n) {
            if (m == "{{") { return "{"; }
            if (m == "}}") { return "}"; }
            return args[n];
        });
    },
    print: function () {
        try {
            document.execCommand('print', false, null);
        }
        catch (e) {
            window.print();
        }
    },
    daterange: function (idfrom, idto) {
        $(idto).rules('add', { daterange: idfrom });
    },
    clearFormElements: function (ele) {
        $(ele).find(':input').each(function () {
            switch (this.type) {
                case 'hidden':
                case 'password':
                case 'select-multiple':
                case 'select-one':
                case 'text':
                case 'textarea':
                    $(this).val('');
                    break;
                case 'checkbox':
                case 'radio':
                    this.checked = false;
            }
            var zeroi = $monibyte.formatNumber(0);
            var zerof = $monibyte.formatNumber(0, { float: true });
            $('.mb-numeric-int').val(zeroi);
            $('.mb-numeric-float').val(zerof);
        });
    },
    blockPage: function () {
        $.blockUI({
            message: $('div.op-espera'),
            css: { left: '48%', padding: '5px' }
        });
    },
    unblockPage: function () {
        $.unblockUI();
    },
    blockRedirect: function (url) {
        setTimeout(function () {
            $monibyte.blockPage();
            window.location = url;
        }, 0);
    },
    executeAjax: function (settings) {
        var _defaults = {
            name: "executeAjax",
            httpMethod: "POST",
            async: true,
            actionUrl: null,
            jsonData: null,
            callback: null,
            operation: "change",
            contentType: "application/json; charset=utf-8"
        };
        var options = jQuery.extend({}, _defaults, settings);
        //cambiar nombres de funciones a funciones reales
        if (!jQuery.isFunction(options.dataFunc)) {
            options.dataFunc = window[options.dataFunc];
        }
        if (!jQuery.isFunction(options.callback)) {
            options.callback = window[options.callback];
        }
        //obtener los datos requeridos para el ajax
        if (jQuery.isFunction(options.dataFunc)) {
            options.jsonData = options.dataFunc(options.jsonData);
        }
        var submitAjax = function (_settings) {
            if (_settings.contentType == _defaults.contentType) {
                _settings.jsonData = JSON.stringify(_settings.jsonData);
            }
            $.ajaxSetup({ 'async': _settings.async });
            $.ajax({
                contentType: _settings.contentType,
                type: _settings.httpMethod,
                url: _settings.actionUrl,
                data: _settings.jsonData,
                success: function (_data) {
                    var _callbackData = _data;
                    if (_settings.targetId) {
                        _callbackData = _settings.callbackData;
                        if (_data.length > 0) {
                            var targetId = _settings.targetId;
                            var selector = $.type(targetId) == "string" ?
                                document.getElementById(targetId) : targetId;
                            if (_settings.operation == "change") {
                                $(selector).html(_data);
                            } else if (_settings.operation == "prepend") {
                                $(selector).prepend(_data);
                            } else if (_settings.operation == "append") {
                                $(selector).append(_data);
                            } else if (_settings.operation == "replace") {
                                $(selector).replaceWith(_data);
                            }
                        }
                    }
                    if (jQuery.isFunction(_settings.callback)) {
                        _settings.callback(_callbackData);
                    }
                }
            });
            $.ajaxSetup({ 'async': true });
        };
        if (options.confirm) {
            $.jConfirm(options.confirmMsg, "highlight", function (conf) {
                if (conf) {
                    submitAjax(options);
                }
            }, options);
        } else {
            submitAjax(options);
        }
    },
    convertCurrency: function (amount, idOrigen, idDestiny, idConvert, operator, currency) {
        var result = 0;
        if (idOrigen == idDestiny) {
            return amount;
        }

        if (idOrigen == idConvert) {
            result = amount;
        } else {
            switch (true) {
                case (operator == "*"):
                    result = amount * currency;
                    break;
                case (operator == "/"):
                    result = amount / currency;
                    break;
                default:
                    result = 0;
            }
        }

        if (idDestiny != idConvert) {
            switch (true) {
                case (operator == "*"):
                    result = amount / currency;
                    break;
                case (operator == "/"):
                    result = amount * currency;
                    break;
                default:
                    result = 0;
            }
        }
        return result;
    },
    startTimeOut: function (settings) {
        var options = jQuery.extend({
            time: 0,
            done: null,
        }, settings);
        var timeout;
        var startup = function (_options) {
            setTimeout(function () {
                timeout = setTimeout(_options.done, _options.time);
            }, 0);
        };
        startup(options);
        return {
            cancel: function () {
                clearTimeout(timeout);
            },
            restart: function () {
                clearTimeout(timeout);
                startup(options);
            }
        }
    }
}
jQuery.fn.serializeFormJSON = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name]) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

jQuery.fn.exists = function () { return this.length > 0; };

/**Control de eventos de ajax**/
$(document).ajaxStart(function (event, request, settings) {
    $monibyte.blockPage();
});
$(document).ajaxStop(function (event, request, settings) {
    $monibyte.unblockPage();
    if (jQuery.isFunction(window.onAjaxStop)) {
        window.onAjaxStop();
    }
});
$(document).ajaxError(function (event, request, settings) {
    event.preventDefault();
    try {
        if (request.responseText) {
            var responsetext = request.responseText;
            var responsejson = $.parseJSON(responsetext);
            var callback = request.status == 403 ? function () {
                $monibyte.blockRedirect(window.location.href);
            } : null;
            if (jQuery.isFunction(window.onAjaxError)) {
                window.onAjaxError(responsejson, callback);
            }
        }
    } catch (err) {
        console.error(err);
    }
});

var notify = null;
$(function () {
    notify = $("<div/>").kendoNotification()
        .data("kendoNotification");
});
function mostrarResultadoExitoso() {
    notify.setOptions({
        show: function (e) {
            e.element.parent().css({
                zIndex: 20000
            });
        },
        position: { pinned: true, right: 20, top: 207, left: null }
    });
    notify.show($(".op-exitoso").html(), "success");
}
/**Control de eventos de ajax**/

/**Rango de fechas en kendo**/
$(function () {
    jQuery.validator.addMethod("daterange", function (value, element, params) {
        var date1 = $monibyte.parseDate(value);
        var date2 = $monibyte.parseDate($(params).val());
        if (!/Invalid|NaN/.test(date1)) {
            return date1 >= date2;
        }
        return isNaN(value) && isNaN($(params).val()) ||
            (parseFloat(value) >= parseFloat($(params).val()));
    });
});

function startDateChange(control) {
    var endPicker = $(control.sender.element).closest(".k-datepicker")
        .nextAll(".k-datepicker").find("input").first().data("kendoDatePicker");
    var startDate = this.value();
    if (startDate) {
        startDate = new Date(startDate);
        startDate.setDate(startDate.getDate() + 1);
        endPicker.min(startDate);
        if (startDate > this.max()) {
            this.value(this.max());
        }
    }
    else {
        this.value(this.max());
    }
}
function endDateChange(control) {
    var startPicker = $(control.sender.element).closest(".k-datepicker")
            .prevAll(".k-datepicker").find("input").first().data("kendoDatePicker");
    var endDate = this.value();
    if (endDate) {
        endDate.setDate(endDate.getDate());
        startPicker.max(endDate);
        if (startPicker.value() > startPicker.max()) {
            startPicker.value(startPicker.max());
        }
    }
    else {
        if (this.min().getFullYear() <= 1900) {
            this.value(startPicker.max());
        } else { this.value(this.min()); }
    }
}
/**Rango de fechas en kendo**/
/**Control de numéricos**/
jQuery.fn.numericInput = function (settings) {
    var options = jQuery.extend({
        value: 0,
        float: true,
        callback: function () { }
    }, settings);
    setNumericValue($(this));
    $(this).addClass(options.float ? "mb-numeric-float" : "mb-numeric-int");
    $(this).on("keydown", function (e) {
        //console.log(e.keyCode);
        if ($.inArray(e.keyCode, [8, 9, 13, 27, 46, 110, 188, 190]) !== -1 ||
            // Allow: Ctrl+A
            (e.keyCode == 65 && e.ctrlKey === true) ||
            // Allow: home, end, left, right
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        else {
            // Ensure that it is a number and stop the keypress
            if (e.shiftKey || (e.keyCode < 48 || e.keyCode > 57) && (e.keyCode < 96 || e.keyCode > 105)) {
                e.preventDefault();
            }
        }
    });
    $(this).on("keyup", function (e) {
        //Allow: Ctrl+A, home, end, left, right  
        if ((e.keyCode == 65 && e.ctrlKey === true) || (e.keyCode >= 35 && e.keyCode <= 40)) {
            // let it happen, don't do anything
            return;
        }
        if ($.inArray(e.keyCode, [8, 9, 13, 27, 46, 110, 188, 190]) !== -1 ||
            (e.keyCode >= 48 && e.keyCode <= 57) ||
            (e.keyCode >= 96 && e.keyCode <= 105)) {
            $(this).trigger("numericValueChange");
        }
    });
    $(this).on("focusin", function () {
        var _options = { float: options.float, group: false };
        if (options.float) {
            var _number = $monibyte.parseFloat($(this).val());
            $(this).val($monibyte.formatNumber(_number, _options));
        } else {
            var _number = $monibyte.parseInt($(this).val());
            $(this).val($monibyte.formatNumber(_number, _options));
        }
    });
    $(this).on("focusout", function () {
        setNumericValue($(this));
    });
    $(this).on("callback", options.callback);
    $(this).on("change", function () {
        setNumericValue($(this));
        $(this).trigger("callback");
    });
    function setNumericValue(numericInput) {
        var txtValue = $(numericInput).val() ? $(numericInput).val() : options.value;
        var numberValue = options.float ? $monibyte.parseFloat(txtValue) : $monibyte.parseInt(txtValue);
        var formattedValue = $monibyte.formatNumber(numberValue, { float: options.float });
        $(numericInput).attr("value", formattedValue);
        $(numericInput).val(formattedValue);
        $(numericInput).data("value", numberValue);
        if (numberValue == options.value) {
            $(numericInput).addClass("default-val");
        } else {
            $(numericInput).removeClass("default-val");
        }
    }
};
/**Control de numéricos**/

/*Renombrar atributos*/
jQuery.fn.renameAttr = function (name, newName, removeData) {
    var val;
    return this.each(function () {
        val = jQuery.attr(this, name);
        jQuery.attr(this, newName, val);
        jQuery.removeAttr(this, name);
    });
};
/*Renombrar atributos*/

/**Acciones del grid**/
$monibyteGridActions = {
    doexport: function (settings) {
        var options = jQuery.extend({
            gridId: null,
            tipo: "excel",
            separator: "\t"
        }, settings);
        options.dataFunc = window[options.dataFunc];
        options.modelFunc = window[options.modelFunc];
        var _executeExport = function (_export) {
            if (jQuery.isFunction(_export.dataFunc)) {
                _export.jsonData.dynamicData = _export.dataFunc(_export.jsonData);
            }
            _export.jsonData.separator = _export.separator;
            $.post(_export.urlExportar, _export.jsonData, function (data) {
                if (data && data.success) {
                    // Download the file.
                    var nuevaUrl = $monibyte.format("{0}?nombreArchivo={1}",
                        _export.urlDescarga, data.fileName);
                    window.location.replace(nuevaUrl);
                }
            });
        };
        if (options.gridId) {
            var exportableCols = [];
            var selector = document.getElementById(options.gridId);
            var columns = $(selector).data("kendoGrid").columns;
            $.each(columns, function (index, value) {
                var unexportable = false;
                if (value.attributes) {
                    var x = value.attributes.unexportable;
                    unexportable = $monibyte.parseBool(x);
                }
                if (!unexportable && value.field) {
                    exportableCols.push({
                        field: value.field,
                        title: value.title,
                        width: value.width
                    });
                }
            });
            options.jsonData = {
                model: JSON.stringify(exportableCols)
            };
        }
        if (jQuery.isFunction(options.modelFunc)) {
            options.exportFunc = _executeExport;
            options.modelFunc(options);
        } else {
            _executeExport(options);
        }
    },
    boundFilteredColumns: function (grid) {
        var filter = grid.dataSource.filter();
        grid.thead.find(".k-header-column-menu").removeClass("k-state-active");
        grid.thead.find(".k-header-column-menu span").removeClass("k-filter");
        if (filter) {
            var filteredMembers = {};
            setFilteredMembers(filter, filteredMembers);
            grid.thead.find("th[data-field]").each(function () {
                var cell = $(this);
                var filtered = filteredMembers[cell.data("field")];
                if (filtered) {
                    cell.find(".k-header-column-menu").addClass("k-state-active");
                    cell.find(".k-header-column-menu span").addClass("k-filter");
                }
            });
        }
        function setFilteredMembers(filter, members) {
            if (filter.filters) {
                for (var i = 0; i < filter.filters.length; i++) {
                    setFilteredMembers(filter.filters[i], members);
                }
            }
            else {
                members[filter.field] = true;
            }
        }
    },
    groupHeaderTemplateRender: function (settings) {
        var options = jQuery.extend({
            gridId: "grid",
            template: null,
            selectFunc: null
        }, settings);
        var $templateRender = {
            template: kendo.template(options.template),
            getItems: function (itemes, data) {
                $(data).each(function (item) {
                    if (!this.hasSubgroups) {
                        itemes = $.merge(itemes, this.items);
                    } else {
                        return $templateRender.getItems(itemes, this.items);
                    }
                });
                return itemes;
            },
            renderText: function () {
                var selector = document.getElementById(options.gridId);
                var gridData = $(selector).data("kendoGrid").dataSource.data();
                var result = $templateRender.getItems([], gridData);
                var component = $.grep(result, function (item, indx) {
                    if (jQuery.isFunction(options.selectFunc)) {
                        return options.selectFunc({ Index: indx, Item: item });
                    } return false;
                })[0]; //get the first record
                return $templateRender.template(component);
            }
        };
        return $templateRender.renderText();
    },
    checkBound: function (settings) {
        var options = jQuery.extend({ grid: null, bound: null, cbxClass: "chkbxq" }, settings);
        options.cbxClass = $monibyte.format(".{0}", options.cbxClass);
        options.grid.tbody.find('>tr').each(function () {
            var _current = this;
            var _dataItem = options.grid.dataItem(_current);
            if (_dataItem) {
                if (jQuery.isFunction(options.bound)) {
                    if (options.bound(_dataItem, _current)) {
                        $(_current).find(options.cbxClass).prop('checked', true);
                        return true;
                    }
                }
            }
        });
    },
    checkAction: function (settings) {
        $(function () {
            var options = jQuery.extend({
                gridId: "grid", onCheck: null, cbxClass: "chkbxq"
            }, settings);
            options.cbxClass = $monibyte.format(".{0}", options.cbxClass);
            var selector = document.getElementById(options.gridId);

            $(selector).on("change", options.cbxClass, function (e) {
                var grid = $(this).closest("[data-role='grid']")
                    .data("kendoGrid");
                var row = $(this).closest("tr");
                var _dataItem = grid.dataItem(row);
                if (jQuery.isFunction(options.onCheck)) {
                    options.onCheck($(this).is(':checked')
                        , _dataItem, this);
                }
            });
        });
    },
    checkAllAction: function (settings) {
        $(function () {
            var options = jQuery.extend({
                gridId: "grid",
                onCheck: null,
                cbxClass: "chkbxq",
                cbxAllClass: "chkSelectAll"
            }, settings);
            options.cbxClass = $monibyte.format(".{0}", options.cbxClass);
            options.cbxAllClass = $monibyte.format(".{0}", options.cbxAllClass);
            var selector = document.getElementById(options.gridId);
            $(selector).on("change", options.cbxAllClass, function () {
                var _checked = $(this).is(':checked');
                $(selector).find(options.cbxClass).each(function () {
                    $(this).prop('checked', _checked);
                    $(this).trigger("change");
                });
                if (jQuery.isFunction(options.onCheck)) {
                    options.onCheck(_checked);
                }
            });
        });
    },
    genericAction: function (settings) {
        var options = jQuery.extend({
            httpMethod: "POST", actionUrl: "",
            dataFunc: null, confirm: false, confirmMsg: null,
            callback: null, callbackData: null, targetId: null
        }, settings);
        var grid = $(options.source).closest("[data-role='grid']")
            .data("kendoGrid");
        var row = $(options.source).closest("tr");
        var dataItem = grid.dataItem(row);

        if (options.jsonData) {
            options.jsonData = jQuery.extend(
                options.jsonData, dataItem);
        } else {
            options.jsonData = dataItem;
        }
        $monibyte.executeAjax(options);
    },
    deleteItem: function (settings) {
        var grid = $(settings.source).closest("[data-role='grid']")
            .data("kendoGrid");
        var callbackFunction = window[settings.callback];
        settings.callback = function (_data) {
            grid.removeRow($(settings.source).closest("tr"));
            if (jQuery.isFunction(callbackFunction)) {
                callbackFunction(_data);
            }
            window.mostrarResultadoExitoso();
        }
        $monibyteGridActions.genericAction(settings);
    }
}
/**Acciones del grid**/

/**SORT ELEMENTS**/
jQuery.fn.sortElements = (function () {
    var sort = [].sort;
    return function (comparator, getSortable) {
        getSortable = getSortable || function () { return this; };
        var placements = this.map(function () {
            var sortElement = getSortable.call(this),
                parentNode = sortElement.parentNode,
            // Since the element itself will change position, we have
            // to have some way of storing its original position in
            // the DOM. The easiest way is to have a 'flag' node:
                nextSibling = parentNode.insertBefore(
                    document.createTextNode(''),
                    sortElement.nextSibling
                );
            return function () {
                if (parentNode === this) {
                    throw new Error("You can't sort elements if any one is a descendant of another.");
                }
                // Insert before flag:
                parentNode.insertBefore(this, nextSibling);
                // Remove flag:
                parentNode.removeChild(nextSibling);
            };
        });
        return sort.call(this, comparator).each(function (i) {
            placements[i].call(getSortable.call(this));
        });
    };
})();
/**SORT ELEMENTS**/


$(document).on("mousedown", " .k-header .k-header-column-menu", function (e) {
    debugger;
    var fieldname = this.parentNode.getAttribute("data-index");

    if ($("th[data-index=" + fieldname + "]").hasClass("ColumnaSeleccionada"))//tiene la clase que se la quite
    {
        $("th[data-index=" + fieldname + "]").removeClass('ColumnaSeleccionada');
    } else {
        $("th").removeClass('ColumnaSeleccionada');
        $("th[data-index=" + fieldname + "]").toggleClass("ColumnaSeleccionada");
    }

});
