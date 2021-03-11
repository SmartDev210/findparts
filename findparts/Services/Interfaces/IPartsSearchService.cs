using Findparts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findparts.Services.Interfaces
{
    public interface IPartsSearchService
    {
        void PopulateHomePageViewModel(HomePageViewModel viewModel);
    }
}
