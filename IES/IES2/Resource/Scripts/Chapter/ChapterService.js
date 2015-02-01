﻿'use strict';

var aService = angular.module('app.chapter.services', []);

aService.factory('chapterService', ['httpService', function (httpService) {
    var service = {};

    var chapterProviderUrl = '/DataProvider/Chapter/ChapterProvider.aspx'; 

    service.Chapter_Get = function (callback) {
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_Get', null, callback);
    }

    service.Chapter_List = function (model, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_List', { model: model }, callback);
    }

    service.Chapter_File_List = function (chapterId, createUserId, kenId, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_File_List', { chapterId: chapterId, createUserId: createUserId, kenId: kenId }, callback);
    }
    
    service.Chapter_Exercise_List = function (chapterId, creaeUserId, kenId, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_Exercise_List', { chapterId: chapterId, createUserId: createUserId, kenId: kenId }, callback);
    }

    service.Chapter_ADD = function (chapters, model, callback) {
        var param = { chapters: chapters, model: model };
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_ADD', param, callback);
    }

    service.Chapter_Upd = function (model, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_Upd', { model: model }, callback);
    } 
   
    service.Chapter_Del = function (model, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_Del', { model: model }, callback);
    } 

    service.Chapter_Batch_Upd = function (models, callback) { 
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_Batch_Upd', { models: models }, callback);
    } 

    service.save = function (model, callback) { 
        httpService.ajaxPost(chapterProviderUrl, 'Save', { model: model }, callback);
    } 

    service.Chapter_Move = function (chapter, direction, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_Move', { chapter: chapter, direction: direction}, callback);
    }

    return service;
}])