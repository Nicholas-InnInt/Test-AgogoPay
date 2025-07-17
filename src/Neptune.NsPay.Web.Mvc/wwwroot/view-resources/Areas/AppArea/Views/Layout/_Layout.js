(function ($) {
    $(function () {
        function scrollToCurrentMenuElement() {
            if (!$('#kt_aside_menu').length) {
                return;
            }

            var path = location.pathname;
            var menuItem = $("a[href='" + path + "']");
            if (menuItem && menuItem.length) {
                menuItem[0].scrollIntoView({ block: 'center' });
            }
        }

        scrollToCurrentMenuElement();

        // Hide tooltips when item is clicked
        $('[data-bs-toggle="tooltip"]').click(function () {
            var tooltip = bootstrap.Tooltip.getInstance(this);
            tooltip.hide();
        });

        (function () {
            const merchantconnection = new signalR.HubConnectionBuilder()
                .withUrl("/signalr-merchant")  // URL to your SignalR Hub
                .withAutomaticReconnect()  // Enable auto reconnection
                .configureLogging(signalR.LogLevel.Information)
                .build();

            const vnCurrencyFormatter = new Intl.NumberFormat('vi-VN', {
                style: 'currency',
                currency: 'VND',
                maximumFractionDigits: 0
            });

            const $MerchantLockedBalance = $(".flex-shrink-0 #lblMerchantLockedBalance");
            const $MerchantBalance = $(".flex-shrink-0 #lblMerchantBalance");

            merchantconnection.start().then(() => {
                console.log("Connected to SignalR hub!");
                getLatestMerchantInfo();
            }).catch((err) => {
                console.error("Error connecting to SignalR hub:", err);
            });

            merchantconnection.onreconnected(connectionId => {
                console.log("Reconnected to SignalR Order Tracker hub with connection ID:", connectionId);
                getLatestMerchantInfo();
            });

            merchantconnection.on("MerchantInfoUpdate", function (merchantInfo) {
                const selectedCurrency = $("#currencyDisplay").val();

                if (selectedCurrency === "vnd") {
                    $MerchantLockedBalance.text(vnCurrencyFormatter.format(merchantInfo.lockedBalance));
                    $MerchantBalance.text(vnCurrencyFormatter.format(merchantInfo.currentBalance));
                }
                else if (selectedCurrency === "usdt") {
                    $MerchantLockedBalance.text(`${merchantInfo.usdtLockedBalance} USD₮`);
                    $MerchantBalance.text(`${merchantInfo.usdtCurrentBalance} USD₮`);
                }
                //console.log("MerchantInfoUpdate:", merchantInfo);
            });

            $("#currencyDisplay").change(() => {
                $MerchantLockedBalance.text("-- --");
                $MerchantBalance.text("-- --");

                getLatestMerchantInfo();
            });

            setInterval(function () {
                merchantconnection.invoke("Heartbeat").catch(function (err) {
                    console.error("Heartbeat Error : ", err);
                });
            }, 30000);

            function getLatestMerchantInfo() {
                merchantconnection
                    .invoke("GetMerchantInfo")
                    .catch(function (err) {
                        console.error("Error invoking method: ", err);
                    });
            }
        })();
    });
})(jQuery);