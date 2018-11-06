// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    public class MessageSample
    {
        private const int MessageCount = 5;
        private const int TemperatureThreshold = 30;
        private static Random s_randomGenerator = new Random();

        private DeviceClient _deviceClient;

        public MessageSample(DeviceClient deviceClient)
        {
            _deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
        }

        public async Task RunSampleAsync()
        {
            await SendEvent().ConfigureAwait(false);
            await ReceiveCommands().ConfigureAwait(false);
        }

        private async Task SendEvent()
        {
            string dataBuffer;

            int count = 0;

            while(true)
            { 
                
                // sleep for random a number of milliseconds
                Random random = new Random();
                int mseconds=random.Next(5, 20) * 1000;   
                System.Threading.Thread.Sleep(mseconds);

                var Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                dataBuffer = $"{{\"ticketId\":{Guid.NewGuid()},\"entryTime\":{Timestamp}}}";
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                eventMessage.Properties.Add("turnstileAlert", "true" );
                Console.WriteLine("\t{0}> User entered turnstile: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), count++, dataBuffer);
                
                await _deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
                
               
                
            }
        }

        private async Task ReceiveCommands()
        {
            Console.WriteLine("\nDevice waiting for commands from IoTHub...\n");
            Console.WriteLine("Use the IoT Hub Azure Portal to send a message to this device.\n");

            Message receivedMessage;
            string messageData;

            receivedMessage = await _deviceClient.ReceiveAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            if (receivedMessage != null)
            {
                messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

                int propCount = 0;
                foreach (var prop in receivedMessage.Properties)
                {
                    Console.WriteLine("\t\tProperty[{0}> Key={1} : Value={2}", propCount++, prop.Key, prop.Value);
                }

                await _deviceClient.CompleteAsync(receivedMessage).ConfigureAwait(false);
            }
            else
            {
                Console.WriteLine("\t{0}> Timed out", DateTime.Now.ToLocalTime());
            }
        }
    }
}
