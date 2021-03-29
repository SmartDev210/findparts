using DAL;
using Findparts.Core;
using Findparts.Extensions;
using Findparts.Models;
using Findparts.Models.Vendor;
using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Findparts.Services.Services
{
    public class VendorService : IVendorService
    {
        private readonly FindPartsEntities _context;
        public VendorService(FindPartsEntities context)
        {
            _context = context;
        }

        public void AddVendorCert(CertsViewModel input)
        {
            _context.VendorCertInsert(input.VendorId.ToNullableInt(), input.Cert, input.Number);
        }

        public void DeleteVendorCert(int certId)
        {
            _context.VendorCertDelete(certId);
        }

        public VendorIndexPageViewModel GetVendorIndexPageViewModel(string vendorID)
        {

            VendorGeneralTabViewModel generalTabModel = new VendorGeneralTabViewModel() { VendorId = vendorID };
            AddressViewModel addressModel = new AddressViewModel() { VendorId = vendorID };
            RFQPreferencesViewModel rfqViewModel = new RFQPreferencesViewModel() { VendorId = vendorID };
            OEMsViewModel oemViewModel = new OEMsViewModel() { VendorId = vendorID };

            CertsViewModel certsViewModel = new CertsViewModel() { VendorId = vendorID };

            VendorIndexPageViewModel viewModel = new VendorIndexPageViewModel()
            {
                VendorId = vendorID,
                VendorGeneralTabViewModel = generalTabModel,
                AddressViewModel = addressModel,
                RFQPreferencesViewModel = rfqViewModel,
                OEMsViewModel = oemViewModel,
                CertsViewModel = certsViewModel
            };

            addressModel.CountryList = Constants.Countries.Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList();

            var vendor = _context.VendorGetByID(vendorID.ToNullableInt()).FirstOrDefault();
            if (vendor != null)
            {
                // initialize general model
                generalTabModel.VendorName = vendor.VendorName;
                generalTabModel.Country = vendor.Country;
                generalTabModel.WebsiteUrl = vendor.WebsiteURL;
                generalTabModel.DefaultCurrency = vendor.DefaultCurrency;
                if (string.IsNullOrEmpty(generalTabModel.DefaultCurrency))
                    generalTabModel.DefaultCurrency = "USD";

                // initialize address
                addressModel.Address1 = vendor.Address1;
                addressModel.Address2 = vendor.Address2;
                addressModel.Address3 = vendor.Address3;
                addressModel.City = vendor.City;
                addressModel.State = vendor.State;
                addressModel.Zipcode = vendor.Zipcode;
                addressModel.Country = vendor.Country;

                if (!addressModel.CountryList.Any(x => x.Value == vendor.Country))
                {
                    addressModel.CountryList.Insert(1, new SelectListItem { Value = vendor.Country, Text = vendor.Country });
                }

                addressModel.Phone = vendor.Phone;

                // RFQ

                rfqViewModel.RFQPhone = vendor.RFQPhone;
                rfqViewModel.RFQEmail = vendor.RFQEmail;
                rfqViewModel.RFQWebEmails = vendor.RFQWebEmails;
                rfqViewModel.RFQFax = vendor.RFQFax;

                // OEM
                oemViewModel.IsOEM = vendor.OEM ?? false;
                oemViewModel.OEMExclusive = vendor.OEMExclusive ?? false;
                oemViewModel.OEMRequiresRMA = vendor.OEMRequiresRMA ?? false;
            }

            certsViewModel.CertList =  _context.VendorCertGetByVendorID(vendorID.ToNullableInt()).ToList();
            certsViewModel.CertSelectList = new List<SelectListItem>()
            {   
                new SelectListItem() { Value = "FAA", Text = "FAA" },
                new SelectListItem() { Value = "EASA", Text = "EASA" },
                new SelectListItem() { Value = "TCCA", Text = "TCCA" },
                new SelectListItem() { Value = "CAAC", Text = "CAAC" },
                new SelectListItem() { Value = "DGCA", Text = "DGCA" },
                new SelectListItem() { Value = "CAA", Text = "CAA" },
                new SelectListItem() { Value = "CAAV", Text = "CAAV" },
                new SelectListItem() { Value = "GACA", Text = "GACA" },
                new SelectListItem() { Value = "JCAB", Text = "JCAB" },
                new SelectListItem() { Value = "ANAB", Text = "ANAB" },
                new SelectListItem() { Value = "DCAT", Text = "DCAT" },
                new SelectListItem() { Value = "", Text = "Other" }
            };
            return viewModel;
        }

        public void UpdateVendorAddress(AddressViewModel input)
        {
            _context.VendorUpdateAddress2(input.VendorId.ToNullableInt(), input.Address1, input.Address2, input.Address3, input.City, input.State, input.Zipcode, input.Country, input.Phone);
        }

        public void UpdateVendorGeneral(VendorGeneralTabViewModel input)
        {
            _context.VendorUpdateGeneral(input.VendorId.ToNullableInt(), input.WebsiteUrl, input.DefaultCurrency);
        }

        public void UpdateVendorOEM(OEMsViewModel input)
        {
            _context.VendorUpdateOEM(input.VendorId.ToNullableInt(), input.IsOEM, input.OEMExclusive, input.OEMRequiresRMA);
        }

        public void UpdateVendorRFQPrefs(RFQPreferencesViewModel input)
        {
            _context.VendorUpdateRFQPrefs(input.VendorId.ToNullableInt(), input.RFQPhone, input.RFQEmail, input.RFQWebEmails, input.RFQFax);
        }
    }
}