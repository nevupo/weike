﻿'use strict';

var appResource = angular.module('app.resource.controllers', [
    'app.res.services'
]);

appResource.controller('ResourceCtrl', ['$scope', 'resourceService', function ($scope, resourceService) {
    $scope.$emit('onActived', 1);
    $scope.$on("onActived", function (event, active) {
        $scope.actived = active;
    });
    $scope.fileTypes = [];//文件类型
    $scope.timePass = [];//上传时间
    $scope.shareRange = []; //使用权限
    $scope.fileSelection = -1;
    $scope.timeSelection = -1;
    $scope.shareSelection = -1;

    $scope.fileChanged = function (v) {
        $scope.typeSelection = v;
    }
    $scope.timeChanged = function (v) {
        $scope.timeSelection = v;
    }
    $scope.shareChanged = function (v) {
        $scope.shareSelection = v;
    }

    //文件类型初始化
    resourceService.Resource_Dict_FileType_Get(function (data) {
        if (data.d) {
            var item = {};
            angular.copy(data.d[0], item);
            item.id = -1;
            item.name = '不限'; 
            $scope.fileTypes = data.d;
            $scope.fileTypes.insert(0, item);
        }
    });
    //上传时间初始化
    resourceService.Resource_Dict_TimePass_Get(function (data) {
        if (data.d) {
            var item = {};
            angular.copy(data.d[0], item);
            item.id = -1;
            item.name = '不限';
            $scope.timePass = data.d;
            $scope.timePass.insert(0, item);
        }
    });
    //使用权限初始化
    resourceService.Resource_Dict_ShareRange_Get(function (data) {
        if (data.d) {
            var item = {};
            angular.copy(data.d[0], item);
            item.id = -1;
            item.name = '不限';
            $scope.shareRange = data.d;
            $scope.shareRange.insert(0, item);
        }
    });
}]);

