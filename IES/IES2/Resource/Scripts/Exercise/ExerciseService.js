﻿'use strict';

var aService = angular.module('app.exercise.services', [
    'app.common.services'
]);

aService.factory('exerciseService', ['httpService', function (httpService) {
    var service = {};
    var url = '/DataProvider/Exercise/ExerciseProvider.aspx';
    

    //service.Resource_Dict_Requirement_Get = function (callback) {
    //    httpService.ajaxPost(exerciseProviderUrl, 'Resource_Dict_Requirement_Get', null, callback);
    //}

    //习题新增
    service.Exercise_ADD = function (model, callback) {
        httpService.ajaxPost(url, 'Exercise_ADD', { model: model }, callback);
    }

    return service;
}]);