using StoreApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace ClientApp
{
    public class ComputerStoreClient : NotifyPropertyChangeHandler
    {
        #region Variables & properties
        /********************************************************************************/
        private readonly string computerPartsListRequest = "GetPartsList";
        private readonly string computerPartPriseRequest = "GetPartsPrice";

        private IPEndPoint remoteEndpoint;
        private UdpClient udpServer;
        public ObservableCollection<ComputerPart> ComputerParts { get; private set; } = new ObservableCollection<ComputerPart>();

        private string statusMessage = string.Empty;
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }
        #endregion

        #region Constructor
        /********************************************************************************/
        public ComputerStoreClient()
        {
            remoteEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            udpServer = new UdpClient();
        }

        public void ConnectToStoreServer()
        {
            try
            {
                udpServer.Connect(remoteEndpoint);
                StatusMessage = "Connected to store's server";
            }
            // Catch an error when udpServer is closed from other thread at app exit
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public async Task GetComputerPartsList()
        {
            // Create request to server
            RequestToServer requestToServer = new RequestToServer() { RequestCode = computerPartsListRequest };
            // Send request
            await SendRequestToServer(requestToServer);
            // Get result
            string result = await GetResponseFromServer();

            List<string>? partsList = JsonConvert.DeserializeObject<List<string>>(result);

            if (partsList != null)
            {
                ComputerParts.Clear();

                foreach (string part in partsList)
                {
                    ComputerParts.Add(new ComputerPart() { Name = part });
                }
                StatusMessage = "Computer parts list received";
            }
            else
            {
                StatusMessage = "Wrong parts list. Bad response from the server";
            }
        }

        public async Task GetComputerPartsPrice(int partslistIndex)
        {
            // Find part's name
            string partsName = ComputerParts[partslistIndex].Name;
            // Create request to server
            RequestToServer requestToServer = new RequestToServer() { RequestCode = computerPartPriseRequest, Argument = partsName };
            // Send request
            await SendRequestToServer(requestToServer);
            // Get result
            string result = await GetResponseFromServer();

            if (double.TryParse(result, out double price))
            {
                ComputerParts[partslistIndex].Price = price;
                StatusMessage = "Computer parts price received";
            }
            else
            {
                StatusMessage = "Wrong price. Bad response from the server";
            }
        }

        private async Task SendRequestToServer(RequestToServer requestToServer)
        {
            string request = JsonConvert.SerializeObject(requestToServer);
            byte[] buffer = Encoding.UTF8.GetBytes(request);
            await udpServer.SendAsync(buffer, buffer.Length);
        }

        private async Task<string> GetResponseFromServer()
        {
            Task<UdpReceiveResult> task = udpServer.ReceiveAsync();
            string response = Encoding.UTF8.GetString((await task).Buffer);
            return response;
        }

        // Release server resourses at exit
        public void AtExit()
        {
            udpServer.Close();
            udpServer.Dispose();
        }
        #endregion
    }
}
