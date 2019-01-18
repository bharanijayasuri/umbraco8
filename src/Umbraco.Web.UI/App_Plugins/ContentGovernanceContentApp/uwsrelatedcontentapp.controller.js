angular.module("umbraco").controller("UWSRelatedContentAppController",

    function ($scope, $http, $routeParams, notificationsService, assetsService, mediaResource, editorService, contentResource, editorState) {
        $scope.loading = true;
        $http.get("backoffice/UWSContentGovernanceRelatedContent/UWSContentGovernanceRelatedContent/GetAllRelations?id=" + /*$routeParams.id*/editorState.current.id).then(function (response) {
    		$scope.content = response.data;
    		$scope.loading = false;
    		var _config = {
    			headers: {
    				'Content-Type': 'application/json'
    			}
    		}

    	}
        );


        $scope.editItem = function (itemId) {
            mediaResource.getById(itemId)
                .then(function (media) {
                    var myMedia = media;
                
                    var mediaEditor = {
                        id: myMedia.id,
                        submit: function (model) {
                            editorService.close();
                            // update the selected media item to match the saved media item
                            // the media picker is using media entities so we get the
                            // entity so we easily can format it for use in the media grid
                            if (model && model.mediaNode) {
                                entityResource.getById(model.mediaNode.id, "media")
                                    .then(function (mediaEntity) {
                                        // if an image is selecting more than once 
                                        // we need to update all the media items
                                        angular.forEach($scope.images, function (image) {
                                            if (image.id === model.mediaNode.id) {
                                                angular.extend(image, mediaEntity);
                                                image.thumbnail = mediaHelper.resolveFileFromEntity(image, true);
                                            }
                                        });
                                    });
                            }
                        },
                        close: function (model) {
                            editorService.close();
                        }
                    };
                    editorService.mediaEditor(mediaEditor);
                });
        };


        $scope.openContentEditor = function (itemId) {

            contentResource.getById(itemId)
                .then(function (content) {
                    var myContent = content;

                    var contentEditor = {
                        id: myContent.id,
                        submit: function (model) {
                            // update the myContent
                            myContent.name = model.contentmyContent.name;
                            myContent.published = model.contentmyContent.hasPublishedVersion;
                            if (entityType !== "Member") {
                                entityResource.getUrl(model.contentmyContent.id, entityType).then(function (data) {
                                    myContent.url = data;
                                });
                            }
                            editorService.close();
                        },
                        close: function () {
                            editorService.close();
                        }
                    };
                    editorService.contentEditor(contentEditor);
                });
        };

    }

);
