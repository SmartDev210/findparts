using Findparts.Models;
using Findparts.Models.Vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IVendorService
    {
        VendorIndexPageViewModel GetVendorIndexPageViewModel(string vendorID);
        void UpdateVendorGeneral(VendorGeneralTabViewModel input);
        void UpdateVendorRFQPrefs(RFQPreferencesViewModel input);
        void AddVendorCert(CertsViewModel input);
        void UpdateVendorAddress(AddressViewModel input);
        void UpdateVendorOEM(OEMsViewModel input);
        void DeleteVendorCert(int certId);
    }
}
