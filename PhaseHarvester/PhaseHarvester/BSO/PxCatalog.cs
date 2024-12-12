using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;

namespace PhaseHarvester
{
    internal class PxCatalog
    {
        //https://dev-ws.cso.ie/public/api.restful/PxStat.Data.Cube_API.ReadLiveAll
        //https://dev-ws.cso.ie/public/api.restful/PxStat.Data.Cube_API.ReadMetadata/SMRT03/JSON-stat/2.0/en

        string? response;

        public static List<Release> Releases { get; set; }

        internal static dynamic Read(string url)
        {
            string result;
            var socketHandler = new SocketsHttpHandler()
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    // Use DNS to look up the IP addresses of the target host:
                    // - IP v4: AddressFamily.InterNetwork
                    // - IP v6: AddressFamily.InterNetworkV6
                    // - IP v4 or IP v6: AddressFamily.Unspecified
                    // note: this method throws a SocketException when there is no IP address for the host
                    var entry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host, AddressFamily.InterNetwork, cancellationToken);

                    // Open the connection to the target host/port
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                    // Turn off Nagle's algorithm since it degrades performance in most HttpClient scenarios.
                    socket.NoDelay = true;

                    try
                    {
                        await socket.ConnectAsync(entry.AddressList, context.DnsEndPoint.Port, cancellationToken);

                        // If you want to choose a specific IP address to connect to the server
                        // await socket.ConnectAsync(
                        //    entry.AddressList[Random.Shared.Next(0, entry.AddressList.Length)],
                        //    context.DnsEndPoint.Port, cancellationToken);

                        // Return the NetworkStream to the caller
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }

            };

            using (var httpClient = new HttpClient(socketHandler))
            {
                result = GetStringData(httpClient, url).Result;
            }

            return result;

        }

        private static async Task<string> GetStringData(HttpClient httpClient, string url)
        {
            var httpResult = await httpClient.GetAsync(url);

            var result = await httpResult.Content.ReadAsStringAsync();

            return result;
        }




    }
}
