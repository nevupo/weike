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

    service.Chapter_ADD = function (model, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'Chapter_ADD', { model: model }, callback);
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
    service.MoveLeft = function (allChapters, chapter, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'MoveLeft', { allChapters: allChapters, chapter: chapter }, callback);
    }
    service.MoveRight = function (allChapters, chapter, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'MoveRight', { allChapters: allChapters, chapter: chapter }, callback);
    }
    service.MoveUp = function (allChapters, chapter, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'MoveUp', { allChapters: allChapters, chapter: chapter }, callback);
    }
    service.MoveDown = function (allChapters, chapter, callback) {
        httpService.ajaxPost(chapterProviderUrl, 'MoveDown', { allChapters: allChapters, chapter: chapter }, callback);
    }
    return service;
}])