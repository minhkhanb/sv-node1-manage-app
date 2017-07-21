
var adsSys = {};

adsSys.chosen = function () {
    $('.chosen-select-deselect').chosen({ allow_single_deselect: true });
};

adsSys.init = function () {
    adsSys.addAds();
    adsSys.changeInfoUser();
    adsSys.adsMore();
    adsSys.modalHandle();
};

adsSys.variables = {
    ads: '',
    target: ''
};

adsSys.addAdsHandle = function (adsLink) {
    var settings = {
        url: "/ads/addAds",
        type: "POST",
        dataType: "json",
        async: false,
        data: {
            name: adsLink
        },
        success: function (res) {
            var tr = '<tr>';
            tr += '<td>' + res.name + '</td>';
            tr += '</tr>';

            $('.adsList tbody').append(tr);
            $('.adsLink').val('');
            var option = '<option>' + res.name + '</option>';
            $('select.adsRefer').append(option);
        }
    };
    $.ajax(settings); 
}

adsSys.addAds = function () {
    
    $('.btnAddAds').on('click', function (evt) {
        evt.preventDefault();
        var adsLink = $('.adsLink').val();
        if (adsLink == '') {
            return;
        }
        adsSys.addAdsHandle(adsLink); 
    });

    $('.btnAddProvider').on('click', function (evt) {
        var id = $('.providerId').val(),
            ads = $('.adsRefer').val();
        if (id == '') {
            return;
        }
        var settings = {
            url: "/ads/addProvider",
            type: "POST",
            dataType: "json",
            async: false,
            data: {
                id: id,
                ads: ads
            },
            success: function (res) {
                
                if (res.ads == '') {
                    res.ads = '<a href="#" class="btnAddAdsLink">Add Ads</a>';
                }
                var tr = '<tr>';
                tr += '<td>' + res.id + '</td>';
                tr += '<td>' + res.ads + '</td>';
                tr += '</tr>';

                $('.providerList tbody').append(tr);
                $('.providerId').val('');
                $(document).on('click', '.btnAddAdsLink', function (evt) {
                    evt.preventDefault();
                    console.log('click');
                    var focus = $(this).closest('td');
                    $('#myModal').modal('show');
                    adsSys.variables.target = focus;
                });
            }
        };
        $.ajax(settings);
    });

}

adsSys.changeInfoUser = function () {
    var btnChangePassword = $('.btnChangePassword');
    btnChangePassword.on('click', function (evt) {
        //evt.preventDefault();
        var settings = {
            url: "/changepassword",
            type: "POST",
            dataType: "json",
            async: false,
            data: {
                isChangePassword: true
            },
            success: function (res) {
                //$(location).attr('href', "/changepassword");
            }
        };
        $.ajax(settings);  
    });
}

adsSys.updateProvider = function () {
    var id = adsSys.variables.target.prev().text();
    var settings = {
        url: "/ads/updateProvider",
        type: "POST",
        dataType: "json",
        async: false,
        data: {
            id: id,
            ads: adsSys.variables.ads
        },
        success: function (res) {
            
        }
    };
    $.ajax(settings);
    adsSys.addAdsHandle(adsSys.variables.ads);
}

adsSys.modalHandle = function () {
    $('#myModal').on('hide.bs.modal', function (evt) {
        adsSys.updateProvider();
    });
}

adsSys.adsMore = function () {
    var adsName = $('.modal').find('.adsName'),
        ads = $('.modal').find('.ads'),
        btnSelectAds = $('.btnAddProviderPop');

    adsName.on('keyup', function (evt) {
        ads.val('');
        adsSys.variables.ads = $(this).val();
    });
    ads.on('change', function (evt) {
        adsName.val('');
        adsSys.variables.ads = $(this).val();
    });
    btnSelectAds.on('click', function (evt) {
        ads.val('');
        adsName.val('');
        adsSys.variables.target.html(adsSys.variables.ads);
        $('#myModal').modal('hide');
    });
    
}

$(document).ready(adsSys.init);
