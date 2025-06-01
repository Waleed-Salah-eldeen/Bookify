// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.



// Write your JavaScript code.

var updatedRow;
var table;
var datatable;
var exportedcols = [];

function showSuccessMessage(message = 'Saved Successfully!') {
    Swal.fire({
        icon: 'success',
        title: 'success',
        text: message
    });
}
function showErrorMessage(message = 'Something went wrong!') {
    Swal.fire({
        icon: 'error',
        title: 'Oops...',
        text: message.responseText !== undefined ? message.responseText : message
    });
}

function DisableTheButton() {
    $('body :submit').attr('disabled', 'disabled').attr('data-kt-indicator', 'on');
}
function onModalBegin()
{
    DisableTheButton();   
}
function onModalSuccess(item)
{
    showSuccessMessage();
    $('#modal').modal('hide');

    if (updatedRow !== undefined) {
        datatable.row(updatedRow).remove().draw();
        updatedRow = undefined;
    }
    var newRow = $(item);
    datatable.row.add(newRow).draw();

}

function onModalComplete()
{
    $('body :submit').removeAttr('disabled').removeAttr('data-kt-indicator');   
}

function applySelect2() {
    $('.js-select2').select2();
    $('.js-select2').on('select2:select', function (e) {
        $('form').not('#SignOut').validate().element('#' + $(this).attr('id'));
    });
}


//Data Tables
var headers = $('th');
$.each(headers, function (i) {
    if (!$(this).hasClass('js-export-execlude'))
        exportedcols.push(i);
});


// Class definition
var KTDatatables = function () {

    // Private functions
    var initDatatable = function () {

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
            'drawCallback': function () {
                KTMenu.createInstances();
            }
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-datatables').data('export-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedcols
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedcols
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedcols
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedcols
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatables');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();

$(document).ready(function ()
{

    $('form').not('#SignOut').on('submit', function () {

        if ($('.js-tinyMce').length > 0) {
            $('.js-tinyMce').each(function () {
                var input = $(this);

                var content = tinyMCE.get(input.attr('id')).getContent();
                input.val(content);
            });
        }
        var isValid = $(this).valid();
        if (isValid) DisableTheButton();

    });


    if ($('.js-tinyMce').length > 0) {  // it means that if the view has an item that has the 'js-tinyMce' class.

        var options = { selector: ".js-tinyMce", height: "430" };

        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }

        tinymce.init(options);

    }

    applySelect2();

    $('.js-datepicker').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'up',
        maxDate: new Date()
    });
    

    KTUtil.onDOMContentLoaded(function () {
        KTDatatables.init();
    });

                        
    $('body').delegate('.js-render-modal', 'click', function ()
    {
        var btn = $(this);
        var modal = $('#modal');
        modal.find('#modalLabel').text(btn.data('title'));

        if (btn.data('update') !== undefined)
        {
            updatedRow = btn.parents('tr');
        }

        $.get(
        {
                url: btn.data('url'),
                success: function (form)
                {
                    modal.find('.js-modal-body').html(form);
                    $.validator.unobtrusive.parse(modal);
                    applySelect2();

                },
                error: function ()
                {
                    showErrorMessage();
                }

        });

        modal.modal('show');

    });

    $('body').delegate('.js-toggle-status', 'click', function () {
        var btn = $(this);
        var result = confirm('Are you sure you want to toggle this category?');
        if (result) {
            $.post(
                {
                    url: btn.data('url'),
                    data: {
                        '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (LastUpdatedOn) {
                        var row = btn.parents('tr');
                        var status = row.find('.js-status');
                        var newStatusText = status.text().trim() === 'Deleted' ? 'Avilable' : 'Deleted';
                        status.text(newStatusText).toggleClass('badge-light-danger badge-light-success');
                        row.find('.js-last-updated-on').html(LastUpdatedOn);
                        row.addClass('animate__animated animate__flash');
                        showSuccessMessage();
                        row.one('animationend', function () {
                            row.removeClass('animate__animated animate__flash');
                        });
                    },

                    error: function () {
                        showErrorMessage();
                    }

                });
        }

    });

    $('body').delegate('.js-confirm', 'click', function () {
        var btn = $(this);
        var result = confirm(btn.data('message'));
        if (result) {

            $.post({
                url: btn.data('url'),
                data: {
                    '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function () {
                    showSuccessMessage();
                },
                error: function () {
                    showErrorMessage();
                }

            })
        }



    });

    $('.js-signout').on('click', function () {
        $('#SignOut').submit();
    });


});
