
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
    console.log(JSON.stringify(dataAds, null, 2));
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
    //$('.btnAddAds').on('click', function (evt) {
    //    evt.preventDefault();
    //    var adsLink = $('.adsLink').val(),
    //        adsDesc = $('.adsDesc').val();

    //    var dataAds = {
    //        title: adsLink,
    //        description: adsDesc,
    //        icon: adsSys.variables.absolutePathFile
    //    };
    //    if (adsLink == '') {
    //        return;
    //    }
    //    adsSys.addAdsHandle(dataAds);
    //});

    //$('.adsIcon').on('change', function (evt) {
    //    console.log($('.adsIcon'));
    //    var tmpPath = URL.createObjectURL(evt.target.files[0]);
    //    //adsSys.variables.absolutePathFile = tmpPath;
    //    adsSys.variables.absolutePathFile = evt.target.files[0];
    //});
    $('.frmAds').submit(function (evt) {
        //alert('submited');
        //evt.preventDefault();
    });
    $('.btnAddProvider').on('click', function (evt) {
        var title = $('.providerTitle').val(),
            ads = $('.adsRefer').val();
        if (title == '') {
            return;
        }
        var settings = {
            url: "/ads/addProvider",
            type: "POST",
            dataType: "json",
            async: false,
            data: {
                title: title,
                ads: ads
            },
            success: function (res) {

                if (res.ads == '' || res.ads == 'undefined' || res.ads == null) {
                    res.ads = '<a href="#" class="btnAddAdsLink">Add Ads</a>';
                }
                var tr = '<tr>';
                tr += '<td>' + res.title + '</td>';
                tr += '<td>' + res['secret-key'] + '</td>';
                tr += '<td>' + res.ads + '</td>';
                tr += '</tr>';

                $('.providerList tbody').append(tr);
                $('.providerId').val('');
                
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
    var title = adsSys.variables.target.prev().prev().text();
    var settings = {
        url: "/ads/updateProvider",
        type: "POST",
        dataType: "json",
        async: false,
        data: {
            title: title,
            ads: adsSys.variables.ads
        },
        success: function (res) {
            
        }
    };
    $.ajax(settings);
    //adsSys.addAdsHandle(adsSys.variables.ads);
}

adsSys.modalHandle = function () {
    $('#myModal').on('hide.bs.modal', function (evt) {
        if (adsSys.variables.ads == '') {
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
