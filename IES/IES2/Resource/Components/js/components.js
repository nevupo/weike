﻿'use strict';

var app = angular.module('app.custom.directives', []);

app.directive('folderList', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        itemName: '@',
        onSelected: '&',
        onCancel: '&'
    }

    directive.replace = true;

    directive.templateUrl = '/Components/templates/folderList.html';

    directive.compile = function () {
        var c = {};

        c.pre = function(scope, iElem, iAttrs){
            $('.cancel').bind('click', function () {
                console.log('33333333333333333');
            });

            $('.select_box').live('click', function () {
                if (!$(this).hasClass('click')) {
                    $(this).addClass('click');
                    $('.folder_list').show();
                } else {
                    $(this).removeClass('click');
                    $('.folder_list').hide();
                    directive.scope.itemName = 'fffff';
                } 
            }); 

            $('.folder_list li').hover(function () {
                $(this).addClass('active').siblings().removeClass('active');
            }, function () {
                $(this).removeClass('active');
            });

            $('li a').bind('click', function () {
                $('.select_box').click();
            });

        };

        c.post = function(scope, iElem, iAttrs){
           
        };

        return c; 
    }

    return directive;
});



app.directive('fileOperation', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        onProperty: '&',
        onRename: '&',
        onDownload: '&',
        onVideo: '&',
        onRemove: '&'
    }

    directive.templateUrl = '/Components/templates/fileOperation.html';
 
    directive.link = function (scope, elem, iAttrs) {
        //弹出右键菜单
        elem.find('.more_operation').hover(function () {
            $(this).find('.mouse_right').toggle();
        });

        //右键菜单表现形式
        elem.find('.mouse_right li').hover(function () {
            $(this).addClass('active').siblings().removeClass('active');
            $(this).find('.right_obj').show();
        }, function () {
            $(this).removeClass('active');
            $(this).find('.right_obj').hide();
        });
    }

    return directive;
});

app.directive('addKnowledge', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        name: '=',
        chapter: '=',
        chapters: '=',
        importance: '=',
        importances: '=',
        onSaveAdd: '&',
        onSave: '&',
        onCancel: '&'
    }

    directive.templateUrl = '/Components/templates/addKnowledge.html';

    directive.link = function (scope, elem, iAttrs) {
        //弹出右键菜单
        var oHeight = $(document).height();
        var oScroll = $(window).scrollTop();
        var bgCls = '.' + scope.bgClass;
        var popCls = '.' + scope.popClass;
        elem.find('.pop_bg').show().css('height', oHeight);
        elem.find('.pop_400').show().css('top', oScroll + 200);


        elem.find('#btnCancel,#btnSave').bind('click', function () {
            elem.hide();
        }) 
    }

    return directive;
});;


app.directive('addChapter', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        name: '=',
        knowledge: '=',
        knowledges: '=',
        importance: '=',
        importances: '=',
        onSaveAdd: '&',
        onSave: '&',
        onCancel: '&'
    }

    directive.templateUrl = '/Components/templates/addChapter.html';

    directive.link = function (scope, elem, iAttrs) {
        //弹出右键菜单
        var oHeight = $(document).height();
        var oScroll = $(window).scrollTop();
        var bgCls = '.' + scope.bgClass;
        var popCls = '.' + scope.popClass;
        elem.find('.pop_bg').show().css('height', oHeight);
        elem.find('.pop_400').show().css('top', oScroll + 200); 

        elem.find('#btnCancel,#btnSave').bind('click', function () {
            elem.hide();
        })
    }

    return directive;
});