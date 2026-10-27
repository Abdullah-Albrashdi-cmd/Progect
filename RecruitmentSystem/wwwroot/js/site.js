/* TalentBridge - jQuery behaviour (Requirement 8) */
$(function () {

    /* 1. Live job search with AJAX: only the job cards are reloaded, not the whole page */
    var $form = $('#searchForm');
    if ($form.length) {
        var timer;
        var runSearch = function () {
            var $results = $('#jobResults');
            $results.addClass('opacity-50');
            $.get($form.data('search-url'), $form.serialize() + '&handler=Search')
                .done(function (html) { $results.html(html); })
                .fail(function () {
                    $results.html('<div class="alert alert-danger">Could not load jobs. Please try again.</div>');
                })
                .always(function () { $results.removeClass('opacity-50'); });
        };

        $form.on('input change', 'input, select', function () {
            clearTimeout(timer);
            timer = setTimeout(runSearch, 300);   // small delay while the user is typing
        });
        $form.on('submit', function (e) { e.preventDefault(); runSearch(); });
    }

    /* 2. Instant table filter (admin pages) */
    $('#tableFilter').on('input', function () {
        var query = $(this).val().toString().toLowerCase();
        $('.js-filterable tbody tr').each(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(query) > -1);
        });
    });

    /* 3. Click-to-sort table headers (admin pages) */
    $('.js-sortable th[data-sort]').on('click', function () {
        var $th = $(this);
        var index = $th.index();
        var asc = !$th.hasClass('sorted-asc');

        $th.siblings().removeClass('sorted-asc sorted-desc');
        $th.removeClass('sorted-asc sorted-desc').addClass(asc ? 'sorted-asc' : 'sorted-desc');

        var $tbody = $th.closest('table').find('tbody');
        var rows = $tbody.find('tr').get();

        var cellValue = function (row) {
            var $cell = $(row).children().eq(index);
            var raw = $cell.attr('data-value');
            return raw !== undefined ? raw : $cell.text().trim();
        };

        rows.sort(function (a, b) {
            var x = cellValue(a), y = cellValue(b);
            var nx = parseFloat(x), ny = parseFloat(y);
            var result = (!isNaN(nx) && !isNaN(ny)) ? nx - ny : x.localeCompare(y);
            return asc ? result : -result;
        });
        $.each(rows, function (_, row) { $tbody.append(row); });
    });

    /* 4. Confirmation dialog for destructive actions: <form data-confirm="Are you sure?"> */
    $(document).on('submit', 'form[data-confirm]', function (e) {
        if (!window.confirm($(this).data('confirm'))) { e.preventDefault(); }
    });

    /* 5. Success / info alerts fade away automatically */
    setTimeout(function () {
        $('.alert-success.alert-dismissible, .alert-info.alert-dismissible').each(function () {
            bootstrap.Alert.getOrCreateInstance(this).close();
        });
    }, 6000);
});
