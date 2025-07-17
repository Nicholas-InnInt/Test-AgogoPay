(function () {
    $(function () {

        var _nsNPayMaintenanceService = abp.services.app.nsPayMaintenance;
		
        //Caching
        function clearCache(cacheName) {
            _nsNPayMaintenanceService
                .clearCache({
                    id: cacheName,
                })
                .done(function () {
                    abp.notify.success(app.localize('RedisSuccessfullyReset'));
                });
        }

        function clearAllCaches() {
            _nsNPayMaintenanceService.clearAllCaches().done(function () {
                abp.notify.success(app.localize('AllRedisSuccessfullyReset'));
            });
        }

        $('.btn-clear-cache').click(function (e) {
            e.preventDefault();
            var cacheName = $(this).attr('data-cache-name');
            clearCache(cacheName);
        });

        $('#ClearAllCachesButton').click(function (e) {
            e.preventDefault();
            clearAllCaches();
        });

    });
})();
