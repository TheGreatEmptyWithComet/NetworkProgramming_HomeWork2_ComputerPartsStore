using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StoreApp
{
    public class ComputerPartsStoreViewModel
    {
        ComputerPartsStore computerPartsStore;
        public ObservableCollection<string> ComputerPartsStoreLog { get; private set; } = new ObservableCollection<string>();

        public ICommand AtExitComand { get; private set; }

        public ComputerPartsStoreViewModel()
        {
            computerPartsStore = new ComputerPartsStore();
            computerPartsStore.OnNotifyDialogWithClient += (message) => { ComputerPartsStoreLog.Add(message); };

            // Release server resourses at exit
            AtExitComand = new RelayCommand(() => computerPartsStore.AtExit());
        }
    }
}
