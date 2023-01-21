#region License

// // MIT License
// //
// // Copyright (c) 2023 Joerg Frank
// // http://www.diagprof.com/
// //
// // Permission is hereby granted, free of charge, to any person obtaining a copy
// // of this software and associated documentation files (the "Software"), to deal
// // in the Software without restriction, including without limitation the rights
// // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// // copies of the Software, and to permit persons to whom the Software is
// // furnished to do so, subject to the following conditions:
// //
// // The above copyright notice and this permission notice shall be included in all
// // copies or substantial portions of the Software.
// //
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// // SOFTWARE.

#endregion

using Windows.Graphics;
using EcuDiagSim.App.Helpers;
using EcuDiagSim.App.Interfaces;
using Microsoft.UI.Xaml;
using EcuDiagSim.App.Models;
using System.Collections.Generic;
using EcuDiagSim.App.Definitions;
using System.Linq;
using CommunityToolkit.Mvvm.Collections;
using ISO22900.II;
using System.Text.RegularExpressions;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EcuDiagSim.App.Services
{
    public class ApiWithAssociatedVciService : IApiWithAssociatedVciService
    {
        private readonly ISettingsService _settingsService;
        private Window _window;

        public ApiWithAssociatedVciService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            SettingsName = $"{GetType().Namespace}.{GetType().Name}";
        }

        protected string SettingsName { get; init; }

        protected string ApiShortNameKey { get; set; } = "ApiShortName";

        protected string VciNameKey { get; set; } = "VciName";

        public void Initialize(Window window)
        {
            _window = window;

            //if (LoadVciOnApiSettings() is (string ApiShortName, string VciName) && Width > 0 && Height > 0)
            //{
            //    SetVciOnApi(ApiShortName, VciName);
            //}
        }

        public (string ApiShortName, string VciName)? LoadVciOnApiSettings()
        {
            if ((_settingsService.TryLoad(SettingsName, ApiShortNameKey, out string apiShortName) is true &&
                 (_settingsService.TryLoad(SettingsName, VciNameKey, out string vciName) is true)))
            {
                return (apiShortName, vciName);
            }

            return null;
        }

        public bool SaveVciOnApiSettings(string apiShortName, string vciName = "")
        {
            return
                _settingsService.TrySave(SettingsName, ApiShortNameKey, apiShortName) &&
                _settingsService.TrySave(SettingsName, VciNameKey, vciName);
        }

        public IEnumerable<IGrouping<ApiForVehicleCommunication, VehicleCommunicationInterface>> GetAllInstalledApisWithRelatedVcis()
        {
            foreach ( var apiDetail in DiagPduApiHelper.InstalledMvciPduApiDetails() )
            {
                //https://www.kenneth-truyers.net/2016/05/12/yield-return-in-c/
                yield return new Grouping<ApiForVehicleCommunication, VehicleCommunicationInterface>(
                    new ApiForVehicleCommunication()
                    {
                        Api = VciApiType.ISO229002,
                        ApiShortName = apiDetail.ShortName,
                        ApiSupplierName = apiDetail.SupplierName,
                        ApiDescription = apiDetail.Description,
                        CableDescriptionFile = apiDetail.CableDescriptionFile,
                    },
                    GetAllVcisBelongToOneApi(apiDetail)
                );
            }
        }

        private static IEnumerable<VehicleCommunicationInterface> GetAllVcisBelongToOneApi(DiagPduApiHelper.MvciPduApiDetail apiDetail)
        {
            using ( var sys = DiagPduApiOneFactory.GetApi(apiDetail.LibraryFile) )
            {
                var dataSets = sys.PduModuleDataSets;
                foreach ( var moduleData in dataSets)
                {
                    yield return new VehicleCommunicationInterface
                    {
                        //Also some API data to the VCI object to make it easier for us to connect to the VCI later.
                        Api = VciApiType.ISO229002, 
                        ApiShortName = apiDetail.ShortName,
                        //VCI-Data
                        VciName = moduleData.VendorModuleName, 
                        VciState = moduleData.ModuleStatus.ToString()
                    };
                }
            }
        }

        public void SetVciOnApi(string apiShortName, string vciName = "")
        {
            //if (_window is not null && width > 0 && height > 0)
            //{
            //    _window.GetAppWindow().Resize(new SizeInt32(width, height));
            //}
        }

    }
}
