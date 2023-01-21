

using EcuDiagSim.App.Definitions;

namespace EcuDiagSim.App.Models
{
    public class VehicleCommunicationInterface
    {       
        public VciApiType Api { get; set; }
        public string ApiShortName { get; set; }
        public string ApiSupplierName { get; set; }
        public string VciName { get; set; }
        public string VciState { get; set; }
    }
}
