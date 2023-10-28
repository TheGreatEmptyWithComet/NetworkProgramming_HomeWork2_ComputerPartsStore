using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StoreApp
{
    public class ComputerPartsStore
    {
        #region Delegates & Events
        /********************************************************************************/
        public delegate void NotifyDialogWithClient(string message);
        public event NotifyDialogWithClient? OnNotifyDialogWithClient;
        #endregion


        #region Variables & properties
        /********************************************************************************/
        private const string dataFileName = "Files/ComputerParts.txt";
        private readonly string computerPartsListRequest = "GetPartsList";
        private readonly string computerPartPriseRequest = "GetPartsPrice";
        private readonly string wrongRequestMessage = "Wrong request";

        private IPEndPoint localEndpoint;
        private UdpClient udpServer;
        private List<ComputerPart> computerParts = new List<ComputerPart>();
        #endregion


        #region Constructor
        /********************************************************************************/
        public ComputerPartsStore()
        {
            localEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            udpServer = new UdpClient(localEndpoint);

            InitComputerPartsList();

            WaitForClientsAsync();
        }
        #endregion


        #region Methods
        /********************************************************************************/
        private void InitComputerPartsList()
        {
            string partsAsJson = File.ReadAllText(dataFileName);
            List<ComputerPart>? computerPartsTemp = JsonConvert.DeserializeObject<List<ComputerPart>>(partsAsJson);
            if (computerPartsTemp != null)
            {
                computerParts = computerPartsTemp;
            }
        }

        private async Task WaitForClientsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    SendLogMessage($"Store is waiting for clients...");

                    while (true)
                    {
                        // Create client. Ip adress and port in constructor set a connection limit for clients
                        IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);

                        // Receive connection from client
                        byte[] buffer = udpServer.Receive(ref clientEndpoint);
                        SendLogMessage($"Client {clientEndpoint} connected");
                        string clientRequest = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                        ProcessClientRequestAsync(clientRequest, clientEndpoint);
                    }
                }
                catch (Exception ex)
                {

                }
            });
        }

        private async Task ProcessClientRequestAsync(string clientRequest, IPEndPoint clientEndpoint)
        {
            await Task.Run(() =>
            {
                string responseMessage = wrongRequestMessage;
                RequestToServer? request = JsonConvert.DeserializeObject<RequestToServer>(clientRequest);

                // Bad request case
                if (request == null || string.IsNullOrEmpty(request.RequestCode))
                {
                    responseMessage = wrongRequestMessage;
                    SendLogMessage($"Client {clientEndpoint} sent bad request");
                }
                // Computer parts list is needed
                else if (request.RequestCode == computerPartsListRequest)
                {
                    List<string> partsList = computerParts.Select(i => i.Name).ToList();
                    responseMessage = JsonConvert.SerializeObject(partsList, Formatting.Indented);
                    SendLogMessage($"Client {clientEndpoint} is asking for the parts list");
                }
                // Computer part's price is needed
                else if (request.RequestCode == computerPartPriseRequest)
                {
                    if (!string.IsNullOrEmpty(request.Argument) && (computerParts.Find((part) => part.Name == request.Argument)) != null)
                    {
                        double price = computerParts.Where(i => i.Name == request.Argument).Select(i => i.Price).FirstOrDefault();
                        responseMessage = price.ToString();
                        SendLogMessage($"Client {clientEndpoint} is asking for the part {request.Argument} price");
                    }
                    else
                    {
                        responseMessage = wrongRequestMessage;
                        SendLogMessage($"Client {clientEndpoint} is asking for the absent part price");
                    }
                }

                AnswerToClient(responseMessage, clientEndpoint);
            });
        }

        private void AnswerToClient(string responseMessage, IPEndPoint clientEndpoint)
        {
            byte[] answer = Encoding.UTF8.GetBytes(responseMessage);
            udpServer.Send(answer, answer.Length, clientEndpoint);
            SendLogMessage($"Client {clientEndpoint} was sent a response");
        }

        private void SendLogMessage(string message)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                OnNotifyDialogWithClient?.Invoke(message);
            });
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
