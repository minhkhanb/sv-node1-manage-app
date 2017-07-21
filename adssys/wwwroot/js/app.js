
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
    target: '',
    absolutePathFile: ''
};

adsSys.addAdsHandle = function (dataAds) {
    var settings = {
        url: "/ads/addAds",
        type: "POST",
        dataType: "json",
        async: false,
        data: dataAds,
        enctype:"multipart/form-data",
        success: function (res) {
            var tr = '<tr>';
            tr += '<td>' + res.title + '</td>';
            tr += '<td>' + res.description + '</td>';
            tr += '<td>' + res.icon + '</td>';
            tr += '</tr>';

            $('.adsList tbody').append(tr);
            $('.adsLink').val('');
            var option = '<option>' + res.title + '</option>';
            $('select.adsRefer').append(option);
            $('.modal select.ads').append(option);
        }
    };
    $.ajax(settings); 
}

adsSys.addAds = function () {
    $(document).delegate('.btnAddAdsLink', 'click', function (evt) {
        evt.preventDefault();
        var focus = $(this).closest('td');
        $('#myModal').modal('show');
        adsSys.variables.target = focus;
    });

    $('.btnAddProvider').on('click', function (evt) {
        var title = $('.providerTitle').val(),
            adsId = $('.adsRefer').find('option:selected').attr('value');
        if (title === '') {
            return;
        }
        var settings = {
            url: "/ads/addProvider",
            type: "POST",
            dataType: "json",
            async: false,
            data: {
                title: title,
                adsId: adsId
            },
            success: function (res) {

                if (res.adsTitle === '' || res.adsTitle === 'undefined' || res.adsTitle === null) {
                    res.adsTitle = '<a href="#" class="btnAddAdsLink">Add Ads</a>';
                }
                var tr = '<tr>';
                tr += '<td>' + res.provider.id + '</td>';
                tr += '<td>' + res.provider.title + '</td>';
                tr += '<td>' + res.provider['secret-key'] + '</td>';
                tr += '<td>' + res.adsTitle + '</td>';
                tr += '</tr>';

                $('.providerList tbody').append(tr);
                $('.providerTitle').val('');
                $('.adsRefer').val('');
                
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

            }
        };
        $.ajax(settings);  
    });
}

adsSys.updateProvider = function () {
    var title = adsSys.variables.target.prev().prev().text();
    var settings = {
        url: "/ads/updateProvider",
        type: "POST",
        dataType: "json",
        async: false,
        data: {
            title: title,
            adsId: adsSys.variables.ads
        },
        success: function (res) {
            adsSys.variables.target.html(res);
            adsSys.isUpdate = false;
        }
    };
    $.ajax(settings);
    //adsSys.addAdsHandle(adsSys.variables.ads);
}
adsSys.isUpdate = false;
adsSys.modalHandle = function () {
    $('#myModal').on('hide.bs.modal', function (evt) {
        if (!adsSys.isUpdate) {
            return;
        }
        adsSys.updateProvider();
    });
}

adsSys.adsMore = function () {
    var adsName = $('.modal').find('.adsName'),
        ads = $('.modal').find('.ads'),
        btnSelectAds = $('.btnAddProviderPop');

    adsName.on('keyup', function (evt) {
        ads.val('');
        //adsSys.variables.ads = $(this).find('option:selected').attr('value');
    });
    ads.on('change', function (evt) {
        
        adsSys.variables.ads = $(this).find('option:selected').attr('value');
        adsName.val('');
    });
    btnSelectAds.on('click', function (evt) {
        adsSys.isUpdate = true;
        adsSys.variables.target.html(ads.find('option:selected').text());
        $('#myModal').modal('hide');
        ads.val('');
        adsName.val('');
    });
    
}

$(document).ready(adsSys.init);
