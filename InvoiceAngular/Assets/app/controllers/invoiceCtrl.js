(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoiceCtrl', invoiceCtrl);

    invoiceCtrl.$inject = ['$scope', 'Restangular', 'ngTableParams', 'Upload', '$timeout'];
    function invoiceCtrl($scope, Restangular, ngTableParams, Upload, $timeout) {

        /* jshint validthis:true */
        var vm = this;

        vm.search = '';

        vm.tableParams = new ngTableParams({
            page: 1,
            count: 10,
            sorting: {
                customername: 'asc'
            }
        },
        {
            getData: function ($defer, params) {
                if (params.settings().$scope == null) {
                    params.settings().$scope = $scope;
                }
                // Load the data from the API
                Restangular.all('invoice/GetItems').getList({
                    pageNo: params.page(),
                    pageSize: params.count(),
                    sort: params.orderBy(),
                    search: vm.search
                }).then(function (invoice) {
                    // Tell ngTable how many records we have (so it can set up paging)
                    params.total(invoice.paging.totalRecordCount);

                    // Return the customers to ngTable
                    $defer.resolve(invoice);
                }, function (response) {
                    // Notify of error
                });
            }
        });
        //
        $scope.reloadtabel = function (vm) {
            vm.tableParams.reload();
        };


        // Watch for changes to the search text, so we can reload the table
        $scope.$watch(angular.bind(vm, function () {
            return vm.search;
        }), function (value) {
            vm.tableParams.reload();
        });

        $scope.deleteinvoice = function (invoice, vm) {
            bootbox.confirm("Are you sure you want to delete this entity ?", function (confirmation) {
                if (confirmation) {
                    Restangular.one("invoice/DeletItem").remove({
                        oID: invoice.oid
                    });
                    vm.tableParams.reload();
                }

            }
           );
        };

        //PDF File upload
    
        $scope.uploadPDF = function (file, errFiles, vm, invoice) {
            $scope.f = file;
            $scope.errFile = errFiles && errFiles[0];
            if (file) {
                file.upload = Upload.upload({
                    url: 'api/invoice/UploadPDFFile',
                    data: {
                        file: file,
                        oid: invoice.oid
                    }
                });

                file.upload.then(function (response) {
                    $timeout(function () {
                        file.result = response.data;
                    });
                }, function (response) {
                    if (response.status > 0)
                        $scope.errorMsg = response.status + ': ' + response.data;
                }, function (evt) {
                    file.progress = Math.min(100, parseInt(100.0 *
                                             evt.loaded / evt.total));
                });

            }
            vm.tableParams.reload();
        }


        //Excel file Upload
        $scope.uploadFiles = function (file, errFiles, vm) {
            $scope.f = file;
            $scope.errFile = errFiles && errFiles[0];
            if (file) {
                file.upload = Upload.upload({
                    url: 'api/invoice/UploadExcelFile',
                    data: { file: file }
                });

                file.upload.then(function (response) {
                    $timeout(function () {
                        file.result = response.data;
                    });
                }, function (response) {
                    if (response.status > 0)
                        $scope.errorMsg = response.status + ': ' + response.data;
                }, function (evt) {
                    file.progress = Math.min(100, parseInt(100.0 *
                                             evt.loaded / evt.total));
                });

            }
            vm.tableParams.reload();
        }


        //Load CSV file
        $scope.saveCSV = function () {
            var csvInput = extractDetails();

            // File is an angular resource. We call its save method here which
            // accesses the api above which should return the content of csv
            File.save(csvInput, function (content) {
                var dataUrl = 'data:text/csv;utf-8,' + encodeURI(content);
                var hiddenElement = document.createElement('a');
                hiddenElement.setAttribute('href', dataUrl);
                hiddenElement.click();
            });
        };

        $scope.downloadFile = function () {
            window.open('api/invoice/GetCSVFile', '_blank', '');
        }
    }
})();