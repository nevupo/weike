﻿'use strict';

var app = angular.module('app.custom.directives', ['app.assist.services', 'ui.tree']);


app.directive('moreCourse', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.replace = true;

    directive.scope = {
        onSelected: '&',
        selection: '=',
        course: '=',
        courseIndex: '='
    }

    directive.templateUrl = '/Components/templates/moreCourse.html';

    directive.link = function (scope, elem, iAttrs) {
        //查看更多
        elem.hover(function () {
            var len = $('.second_nav').length;
            if (len > 0) {
                $(this).find('i').addClass('slide_up');
                $(this).find('.second_nav').show();
            }
        }, function () {
            $(this).find('i').removeClass('slide_up');
            $(this).find('.second_nav').hide();
        })
    }

    return directive;
});

app.directive('fileOperation', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.replace = true;

    directive.scope = {
        onProperty: '&',
        onRename: '&',
        onDownload: '&',
        onVideo: '&',
        onRemove: '&',
        onReturnPage: '&',
        onMobile: '&',
        shareRanges: '=',
        folderrelation: '='
    }

    directive.templateUrl = '/Components/templates/fileOperation.html';

    directive.link = function (scope, elem, iAttrs) {
        //弹出右键菜单
        elem.parent().hover(function () {
            $(this).find('.mouse_right').toggle();
        });

        //右键菜单表现形式
        elem.parent().find('.mouse_right li').hover(function () {
            $(this).addClass('active').siblings().removeClass('active');
            $(this).find('.right_obj').show();
        }, function () {
            $(this).removeClass('active');
            $(this).find('.right_obj').hide();
        });
    }

    return directive;
});
app.directive('fileShare', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        onShare: '&',
        shareItem: '='
    }

    directive.templateUrl = '/Components/templates/fileShared.html';

    directive.link = function (scope, elem, iAttrs) {
        //弹出右键菜单
        elem.parent().parent().hover(function () {
            $(this).find('.permissions').show();
        }, function () {
            $(this).find('.permissions').hide();
        });
    }

    return directive;
});


app.directive('addKnowledge', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        knowledge: '=',
        chapter: '=',
        chapters: '=',
        requireMent: '=',
        requireMents: '=',
        onSaveNew: '&',
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


        elem.find('#btnCancel,#btnSave,.close_pop').bind('click', function () {
            elem.hide();
        })
    }

    return directive;
});;


app.directive('addChapter', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        chapterName: '=',
        knowledge: '=',
        knowledges: '=',
        requireMent: '=',
        requireMents: '=',
        onSaveNew: '&',
        onSave: '&',
        onCancel: '&'
    }

    directive.replace = true;

    directive.templateUrl = '/Components/templates/addChapter.html';

    directive.link = function (scope, elem, iAttrs) {
        //弹出右键菜单
        var oHeight = $(document).height();
        var oScroll = $(window).scrollTop();
        var bgCls = '.' + scope.bgClass;
        var popCls = '.' + scope.popClass;
        elem.find('.pop_bg').show().css('height', oHeight);
        elem.find('.pop_400').show().css('top', oScroll + 200);

        elem.find('#btnCancel,#btnSave,.close_pop').bind('click', function () {
            elem.hide();
        })
    }

    return directive;
});

app.directive('folder', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        onOpen: '&',
        onBlur: '&',
        folderName: '=',
        folderExt: '='
    }

    directive.templateUrl = '/Components/templates/folder.html';

    directive.link = function (scope, elem, iAttrs) {
        //重命名表现形式
        var e = elem.find('.data_tit');
        e.bind('dblclick', function (e) {
            $(this).hide();
            $(this).next().show().select();
        });
    }

    return directive;
});

app.directive('batchOperation', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        onFireRemoveAll: '&',
        onFireMobileBatch: '&',
        shareRanges: '='
    }

    directive.templateUrl = '/Components/templates/batchOperation.html';

    directive.link = function (scope, elem, iAttrs) {
        elem.find('.batch_list li').hover(function () {
            $(this).addClass('active').siblings().removeClass('active');
        }, function () {
            $(this).removeClass('active');
        })
        elem.find('.permissions li').hover(function () {
            $(this).addClass('current').siblings().removeClass('current');
        }, function () {
            $(this).removeClass('current');
        })
    }

    return directive;
});

app.directive('exerciseBatch', function () {
    var directive = {};

    directive.restrict = 'EA';

    directive.scope = {
        onFireRemoveAll: '&',
        onFireShared: '&'
    }

    directive.templateUrl = '/Components/templates/exerciseBatch.html';

    directive.link = function (scope, elem, iAttrs) {
        elem.find('.batch_list li').hover(function () {
            $(this).addClass('active').siblings().removeClass('active');
        }, function () {
            $(this).removeClass('active');
        })
        elem.find('.permissions li').hover(function () {
            $(this).addClass('current').siblings().removeClass('current');
        }, function () {
            $(this).removeClass('current');
        })
    }

    return directive;
});


app.directive('exerciseList', ['assistService', function (assistService) {
    var directive = {};

    directive.restrict = 'EA';

    directive.replace = true;

    directive.scope = {
        exercise: '=',
        shareExercise: '&',
        editExercise: '&',
        deleteExercise: '&'
    }

    directive.templateUrl = '/Components/templates/exerciseList.html';

    directive.link = function (scope, elem, iAttrs) {
        elem.hover(function () { $(this).find('.topic_icon').show(); },
                   function () { $(this).find('.topic_icon').hide(); }
                  );
        elem.find('.icon.share_topic,.icon.delete_topic,.icon.edit_topic').hover(
            function () { $(this).find('.icon_content').show(); },
            function () { $(this).find('.icon_content').hide(); }
            );
    }

    directive.controller = function ($scope, assistService) { 

        $scope.getDifficultName = function (difficult) { 
            var name = '';
            assistService.Resource_Dict_Diffcult_Get(function (data) {
                if (data.length > 0) {
                    var length = data.length;
                    for (var i = 0; i < length; i++) {
                        if ( data[i].id == difficult) {
                            name = data[i].name;
                            break;
                        }
                    }
                }
            }); 
            return name;
        } 
    }

    return directive;
}]);

//移动文件
app.directive('moveFolder', function () {
    var directive = {};

    directive.restrict = 'EA';
    directive.replace = true;

    directive.scope = {
        onMoveFileSubmit: '&',
        onClose: '&',
        onSelectedMove: '&',
        files: '='
    }

    directive.templateUrl = '/Components/templates/moveFolder.html';

    directive.link = function (scope, elem, iAttrs) {
    }

    return directive;
});
