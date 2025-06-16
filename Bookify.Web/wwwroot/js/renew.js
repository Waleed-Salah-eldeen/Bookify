$(document).ready(function () {
    $('.js-renew').on('click', function () {

        var subscriberKey = $(this).data('key'); 
        var result = confirm('Are you sure that you need to renew this subscription?');
        if (result) {
            $.post({
                url: `/Subscribers/RenewSubscription?sKey=${subscriberKey}`,
                data: {
                    '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (row) {
                    $('#SubscriptionsTable').find('tbody').append(row);

                    var activeIcon = $('#ActiveStatusIcon');
                    activeIcon.removeClass('d-none');
                    activeIcon.siblings('svg').remove();
                    activeIcon.parents('.card').removeClass('bg-warning').addClass('bg-success');

                    $('#CardStatus').text('Active subscriber');
                    $('#StatusBadge').removeClass('badge-light-warning').addClass('badge-light-success').text('Active subscriber');
                    showSuccessMessage();
                },
                error: function () {
                    showErrorMessage();
                }
            });
        }
    });
});