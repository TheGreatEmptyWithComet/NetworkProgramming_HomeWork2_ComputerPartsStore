using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientApp
{
    public class ComputerStoreClientViewModel
    {
        public ComputerStoreClient ComputerStoreClient { get; private set; }

        public ICommand AtExitComand { get; private set; }
        public ICommand ConnectToTheServerCommand { get; private set; }
        public ICommand GetComputerPartsListCommand { get; private set; }
        public ICommand GetComputerPartPriceCommand { get; private set; }

        public ComputerStoreClientViewModel()
        {
            // Added for synchronization with server app in a case both programs start concurently in one solution
            Thread.Sleep(1000);

            ComputerStoreClient = new ComputerStoreClient();

            // Release server resourses at exit
            ConnectToTheServerCommand = new RelayCommand(() => ComputerStoreClient.ConnectToStoreServer());
            GetComputerPartsListCommand = new RelayCommand(() => ComputerStoreClient.GetComputerPartsList());
            GetComputerPartPriceCommand = new RelayCommand<object>((param) => GetComputerPartsPrice(param));
            AtExitComand = new RelayCommand(() => ComputerStoreClient.AtExit());
        }

        private void GetComputerPartsPrice(object param)
        {
            if (param is int index)
            {
                ComputerStoreClient.GetComputerPartsPrice(index);
            }
        }
    }
}
